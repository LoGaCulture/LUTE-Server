using System.Collections.Generic;
using System.Threading.Tasks;
using LUTE_Server.DTOs;
using LUTE_Server.Models;
using LUTE_Server.Repositories;

namespace LUTE_Server.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly JwtService _jwtService;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository userRepository, JwtService jwtService, ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
            _logger = logger;
        }

        public async Task<IEnumerable<User>> GetUsersAsync()
        {
            return await _userRepository.GetUsersAsync();
        }

        public async Task<User> GetUserByIdAsync(int id)
        {
            return await _userRepository.GetUserByIdAsync(id);
        }

        public async Task<User> GetUserByUsernameAsync(string userName)
        {
            return await _userRepository.GetUserByUsernameAsync(userName);
        }
        public async Task AddUserAsync(User user)
        {
            await _userRepository.AddUserAsync(user);
        }

        public async Task UpdateUserAsync(User user)
        {
            await _userRepository.UpdateUserAsync(user);
        }

        public async Task DeleteUserAsync(int id)
        {
            await _userRepository.DeleteUserAsync(id);
        }

        public async Task<AuthResult> RegisterUserAsync(RegisterRequest request)
        {

            _logger.LogInformation("Registering user with username {Username}", request.Username);

            var existingUser = await _userRepository.GetUserByUsernameAsync(request.Username);
            if (existingUser != null)
            {
                _logger.LogWarning("Username {Username} already exists", request.Username);
                return new AuthResult { Success = false, ErrorMessage = "Username already exists" };
            }

            if(!Enum.TryParse(request.Role, true, out UserRole role))
            {
                _logger.LogWarning("Invalid role {Role}", request.Role);
                return new AuthResult { Success = false, ErrorMessage = "Invalid role" };
            }

            _logger.LogInformation("Assigned role: {Role}", role);
            var user = new User
            {
                Username = request.Username,
                Role = role
            };
            user.SetPassword(request.Password);

            await _userRepository.AddUserAsync(user);

            var token = _jwtService.GenerateJwtToken(user);
            return new AuthResult { Success = true, Token = token };
        }

        public async Task<AuthResult> LoginUserAsync(LoginRequest request)
        {

            _logger.LogInformation("Logging in user with username {Username}", request.Username);

            var user = await _userRepository.GetUserByUsernameAsync(request.Username);
            if (user == null || !user.CheckPassword(request.Password))
            {
                _logger.LogWarning("Invalid username or password for user with username {Username}", request.Username);
                return new AuthResult { Success = false, ErrorMessage = "Invalid username or password" };
            }

            var token = _jwtService.GenerateJwtToken(user);
            return new AuthResult { Success = true, Token = token };
        }
    }
}
