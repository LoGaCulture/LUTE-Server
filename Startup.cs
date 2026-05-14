using LUTE_Server.Data;
using LUTE_Server.Repositories;
using LUTE_Server.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using LUTE_Server.Models;


public class Startup
{
    public Startup(IConfiguration configuration, IWebHostEnvironment env)
    {
        Configuration = configuration;
        Env = env;
    }

    public IConfiguration Configuration { get; }
    public IWebHostEnvironment Env { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));

        services.AddControllersWithViews(options =>
        {
            // Automatically validate antiforgery tokens on all state-changing MVC actions.
            // API controllers opt out via [IgnoreAntiforgeryToken] since they receive JSON, not forms.
            options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());
        });
        services.AddRazorPages();
        services.AddSwaggerGen();

        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserService, UserService>();
        services.AddSingleton<JwtService>();

        services.AddScoped<IGameRepository, GameRepository>();
        services.AddScoped<IGameService, GameService>();
        services.AddScoped<ILoggingService, LoggingService>();
        services.AddScoped<ISharedVariableService, SharedVariableService>();
        //services.AddScoped<IGameSharedVariableService, GameSharedVariableService>();
        //services.AddScoped<IGameSharedVariableRepository, GameSharedVariableRepository>();
        services.AddScoped<IUserService, UserService>();

        var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") ?? Configuration["Jwt:Key"];
        var knownPlaceholders = new[] { "__SET_ME__", "SecretDevKeyVeryLongHasToBeForSecurity", "" };
        if (string.IsNullOrWhiteSpace(jwtKey) || knownPlaceholders.Contains(jwtKey) || Encoding.UTF8.GetByteCount(jwtKey) < 32)
            throw new InvalidOperationException(
                "Jwt:Key is not configured, is a placeholder, or is too short (minimum 32 bytes). " +
                "Set a strong secret via the JWT_KEY environment variable or Jwt:Key in appsettings.json.");
        var key = Encoding.UTF8.GetBytes(jwtKey);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = !Env.IsDevelopment();
            options.MapInboundClaims = false; // Keep claim names as issued — no auto-remapping
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = Configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = Configuration["Jwt:Issuer"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(1),
                RoleClaimType = "role",     // matches claim name used in JwtService
                NameClaimType = "username"  // matches claim name used in JwtService
            };
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = ctx =>
                {
                    // Allow the JWT to be read from the auth_token cookie (set by AuthController)
                    if (string.IsNullOrEmpty(ctx.Token) &&
                        ctx.Request.Cookies.TryGetValue("auth_token", out var cookieToken))
                        ctx.Token = cookieToken;
                    return Task.CompletedTask;
                }
            };
        });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin"));
            options.AddPolicy("GameDeveloperPolicy", policy => policy.RequireRole("GameDeveloper"));
            options.AddPolicy("UserPolicy", policy => policy.RequireRole("User"));
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseSwagger();

        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "LUTE API V1");
            c.DefaultModelsExpandDepth(-1);
            c.RoutePrefix = "api";
        });

        var serviceScopeFactory = app.ApplicationServices.GetService<IServiceScopeFactory>();
        if (serviceScopeFactory == null)
        {
            throw new InvalidOperationException("IServiceScopeFactory is not available.");
        }

        using (var serviceScope = serviceScopeFactory.CreateScope())
        {
            var context = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            context.Database.Migrate();
        }

        app.UseHttpsRedirection();

        app.UseDefaultFiles(new DefaultFilesOptions
        {
            DefaultFileNames = new[] { "index.html" }
        });

        app.UseStaticFiles();
        app.UseRouting();

        // Seed default admin on first boot if configured
        using (var serviceScope = app.ApplicationServices.CreateScope())
        {
            var context = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            context.Database.EnsureCreated();

            var config = serviceScope.ServiceProvider.GetRequiredService<IConfiguration>();
            var defaultAdminConfig = config.GetSection("DefaultAdmin");
            var username = defaultAdminConfig["Username"];
            var password = defaultAdminConfig["Password"];
            var enabled = bool.Parse(defaultAdminConfig["Enabled"] ?? "false");

            if (enabled)
            {
                var userService = serviceScope.ServiceProvider.GetRequiredService<IUserService>();
                var passwordHasher = serviceScope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();

                if (string.IsNullOrEmpty(username))
                    throw new ArgumentNullException(nameof(username), "DefaultAdmin:Username cannot be null or empty.");

                var knownPasswordPlaceholders = new[] { "__SET_ME__", "admin123", "" };
                if (string.IsNullOrEmpty(password) || knownPasswordPlaceholders.Contains(password))
                    throw new InvalidOperationException(
                        "DefaultAdmin:Password is missing or is a placeholder. Set a real password before enabling admin seeding.");

                var existingAdmin = userService.GetUserByUsernameAsync(username).Result;
                if (existingAdmin == null)
                {
                    logger.LogInformation("Creating default admin user.");

                    var adminUser = new User
                    {
                        Username = username,
                        Role = UserRole.Admin
                    };
                    adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, password);
                    userService.AddUserAsync(adminUser).Wait();

                    logger.LogInformation("Default admin user created with username: {Username}", username);
                }
            }
        }

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            endpoints.MapRazorPages();

            endpoints.MapControllers();

            app.Use(async (context, next) =>
            {
                if (context.Request.Path == "/")
                {
                    context.Response.Redirect("/login");
                    return;
                }
                await next();
            });

            // Serve the login and register pages from wwwroot
            endpoints.MapGet("/login", async context =>
            {
                if (context.User.Identity?.IsAuthenticated == true)
                {
                    context.Response.Redirect("/admin/dashboard");
                    return;
                }
                context.Response.ContentType = "text/html";
                await context.Response.SendFileAsync("wwwroot/login.html");
            });

            endpoints.MapGet("/register", async context =>
            {
                context.Response.ContentType = "text/html";
                await context.Response.SendFileAsync("wwwroot/register.html");
            });
        });

        logger.LogInformation("Application started");
    }

}
