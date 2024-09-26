using LUTE_Server.Data;
using LUTE_Server.Repositories;
using LUTE_Server.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.Cookies;
using LUTE_Server.Models;


public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));

        services.AddControllersWithViews();
        services.AddRazorPages();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserService, UserService>();
        services.AddSingleton<JwtService>();

        services.AddScoped<IGameRepository, GameRepository>();
        services.AddScoped<IGameService, GameService>();
        services.AddScoped<ILoggingService, LoggingService>();
        services.AddScoped<IGameSharedVariableService, GameSharedVariableService>();
        services.AddScoped<IGameSharedVariableRepository, GameSharedVariableRepository>();
        services.AddScoped<IUserService, UserService>();

        var jwtKey = Configuration["Jwt:Key"] ?? Environment.GetEnvironmentVariable("JWT_KEY") ?? throw new ArgumentNullException("JWT key is not configured.");
        var key = Encoding.UTF8.GetBytes(jwtKey);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultSignOutScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
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


        var serviceScopeFactory = app.ApplicationServices.GetService<IServiceScopeFactory>();
        if (serviceScopeFactory == null)
        {
            throw new InvalidOperationException("IServiceScopeFactory is not available.");
        }

        using (var serviceScope = serviceScopeFactory.CreateScope())
        {
            var context = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            context.Database.EnsureCreated();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();


         // Create service scope for dependency injection
    using (var serviceScope = app.ApplicationServices.CreateScope())
    {
        var context = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Database.EnsureCreated(); // Ensure the database is created

        // Check appsettings for DefaultAdmin section
        var config = serviceScope.ServiceProvider.GetRequiredService<IConfiguration>();
        var defaultAdminConfig = config.GetSection("DefaultAdmin");
        var username = defaultAdminConfig["Username"];
        var password = defaultAdminConfig["Password"];
        var enabled = bool.Parse(defaultAdminConfig["Enabled"] ?? "false");

        if (enabled)
        {
            var userService = serviceScope.ServiceProvider.GetRequiredService<IUserService>();

            // Check if admin already exists
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException(nameof(username), "Username cannot be null or empty.");
            }
            var existingAdmin = userService.GetUserByUsernameAsync(username).Result;
            if (existingAdmin == null)
            {
                // If admin doesn't exist, create the admin user
                logger.LogInformation("Creating default admin user.");

                var adminUser = new User
                {
                    Username = username,
                    Role = UserRole.Admin // Assuming Admin is an enum value 1
                };
                if (string.IsNullOrEmpty(password))
                {
                    throw new ArgumentNullException(nameof(password), "Password cannot be null or empty.");
                }
                adminUser.SetPassword(password);
                userService.AddUserAsync(adminUser).Wait();

                logger.LogInformation("Default admin user created with username: {Username}", username);
            }
        }
    }


        app.Use(async (context, next) =>
        {
            var token = context.Request.Cookies["auth_token"];
            var secretKey = context.Request.Headers["X-Secret-Key"].ToString();





            // Use a service scope to get the ApplicationDbContext
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                if (context.Request.Path.StartsWithSegments("/api/userlog") && !string.IsNullOrEmpty(secretKey))
                {
                    logger.LogInformation("Secret key provided. Skipping JWT validation for Unity logging.");

                    // Validate secret key against stored game secret (optional validation step).
                    var game = _context.Games.FirstOrDefault(g => g.SecretKey == secretKey);
                    if (game == null)
                    {
                        logger.LogWarning("Invalid secret key provided.");
                        context.Response.StatusCode = 401; // Unauthorized
                        await context.Response.WriteAsync("Invalid secret key.");
                        return;
                    }

                    // Set an internal flag for Unity request (optional).
                    context.Items["IsUnityRequest"] = true;
                }
                else if (!string.IsNullOrEmpty(token))
                {
                    // Proceed with JWT validation as normal.
                    logger.LogInformation("Validating JWT token...");

                    var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                    var jwtKey = Configuration["Jwt:Key"] ?? throw new ArgumentNullException("Jwt:Key");
                    var key = Encoding.UTF8.GetBytes(jwtKey);

                    try
                    {
                        tokenHandler.ValidateToken(token, new TokenValidationParameters
                        {
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = new SymmetricSecurityKey(key),
                            ValidateIssuer = false,
                            ValidateAudience = false,
                            ClockSkew = TimeSpan.Zero
                        }, out var validatedToken);

                        logger.LogInformation($"Validated Token: {validatedToken}");

                        var jwtToken = (System.IdentityModel.Tokens.Jwt.JwtSecurityToken)validatedToken;

                        var username = jwtToken.Claims.FirstOrDefault(x => x.Type == "username")?.Value;
                        var userId = jwtToken.Claims.FirstOrDefault(x => x.Type == "userId")?.Value;
                        var role = jwtToken.Claims.FirstOrDefault(x => x.Type == "role")?.Value;

                        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(role))
                        {
                            throw new ArgumentNullException("Username or role claims are missing from the token.");
                        }

                        var claims = new List<Claim>
                        {
                        new Claim(ClaimTypes.Name, username),
                        new Claim(ClaimTypes.NameIdentifier, userId ?? throw new ArgumentNullException("userId")),
                        new Claim(ClaimTypes.Role, role)
                        };

                        var identity = new ClaimsIdentity(claims, "jwt");
                        context.User = new ClaimsPrincipal(identity);

                        if (context.User.Identity != null)
                        {
                            logger.LogInformation($"User: {context.User.Identity.Name}");
                        }
                        else
                        {
                            logger.LogWarning("User identity is null.");
                        }
                        logger.LogInformation($"Roles: {string.Join(", ", context.User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value))}");
                    }
                    catch (Exception e)
                    {
                        logger.LogWarning("Token validation failed.");
                        logger.LogError(e, "Token validation error");
                    }
                }
                else
                {
                    logger.LogInformation("No token or secret key found.");
                }
            }

            await next();
        });

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
