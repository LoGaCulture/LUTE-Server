using System.Threading.Tasks;
using LUTE_Server.DTOs;
using LUTE_Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace LUTE_Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
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
            return Ok(new { token = result.Token });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
           if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //print the request for debugging
            Console.WriteLine($"Login request: {request.Username}, {request.Password}");

            var result = await _userService.LoginUserAsync(request);
            if (!result.Success)
            {
                return Unauthorized(result.ErrorMessage);
            }
            return Ok(new { token = result.Token });
        }
    }
}
