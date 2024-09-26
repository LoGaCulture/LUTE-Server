using Microsoft.AspNetCore.Mvc;
using LUTE_Server.Services;
using LUTE_Server.Models;

namespace LUTE_Server.Controllers
{
    public class LoginController : Controller
    {
        private readonly JwtService _jwtService;

        public LoginController(JwtService jwtService)
        {
            _jwtService = jwtService;
        }

        [HttpPost]
        public IActionResult Login(LoginModel model)
        {
            // Authenticate the user (not shown here)
            var user = AuthenticateUser(model);

            if (user != null)
            {
                var token = _jwtService.GenerateJwtToken(user);

                // Set the JWT token in a cookie
                Response.Cookies.Append("auth_token", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict
                });

                return RedirectToAction("Index", "Admin");
            }

            return View("Login");
        }

        private User AuthenticateUser(LoginModel model)
        {
            // Authentication logic here (e.g., validate user credentials)
            return new User(); // Replace with actual user lookup and validation
        }
    }

    public class LoginModel
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
    }
}
