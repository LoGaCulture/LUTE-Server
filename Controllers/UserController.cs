using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LUTE_Server.DTOs;
using LUTE_Server.Models;
using LUTE_Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LUTE_Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    [Microsoft.AspNetCore.Mvc.IgnoreAntiforgeryToken]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IPasswordHasher<User> _passwordHasher;

        public UserController(IUserService userService, IPasswordHasher<User> passwordHasher)
        {
            _userService = userService;
            _passwordHasher = passwordHasher;
        }

        // GET /api/user — returns all users without PasswordHash
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            var users = await _userService.GetUsersAsync();
            return Ok(users.Select(UserDto.FromUser));
        }

        // GET /api/user/{id} — returns a single user without PasswordHash
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(UserDto.FromUser(user));
        }

        // POST /api/user — admin creates a user with an explicit role and hashed password
        [HttpPost]
        public async Task<ActionResult<UserDto>> AddUser([FromBody] CreateUserRequest request)
        {
            var user = new User
            {
                Username = request.Username,
                Role = request.Role
            };
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

            await _userService.AddUserAsync(user);
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, UserDto.FromUser(user));
        }

        // PUT /api/user/{id} — admin updates only the role (no other fields accepted)
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.Role = request.Role;
            await _userService.UpdateUserAsync(user);
            return NoContent();
        }

        // DELETE /api/user/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteUser(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            await _userService.DeleteUserAsync(id);
            return NoContent();
        }
    }
}
