using System.Threading.Tasks;
using LUTE_Server.DTOs;
using LUTE_Server.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LUTE_Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IWebHostEnvironment _env;

        public AuthController(IUserService userService, IWebHostEnvironment env)
        {
            _userService = userService;
            _env = env;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _userService.RegisterUserAsync(request);
            if (!result.Success)
            {
                return BadRequest(result.ErrorMessage);
            }

            SetAuthCookie(result.Token!);
            return Ok(new { success = true });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _userService.LoginUserAsync(request);
            if (!result.Success)
            {
                return Unauthorized(result.ErrorMessage);
            }

            SetAuthCookie(result.Token!);
            // Token is still returned in the body for the Unity client (fromUnity flow).
            // Browser clients should rely on the HttpOnly cookie, not this value.
            return Ok(new { token = result.Token });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("auth_token");
            return Redirect("/login");
        }

        private void SetAuthCookie(string token)
        {
            Response.Cookies.Append("auth_token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = _env.IsProduction(), // HTTP-safe in dev, HTTPS-only in prod
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddHours(1)
            });
        }
    }
}
