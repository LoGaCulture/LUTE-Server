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

        public UserService(IUserRepository userRepository, JwtService jwtService)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
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

            var existingUser = await _userRepository.GetUserByUsernameAsync(request.Username);
            if (existingUser != null)
            {
                return new AuthResult { Success = false, ErrorMessage = "Username already exists" };
            }

            UserRole role = UserRole.User;

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

            var user = await _userRepository.GetUserByUsernameAsync(request.Username);
            if (user == null || !user.CheckPassword(request.Password))
            {
                return new AuthResult { Success = false, ErrorMessage = "Invalid username or password" };
            }

            var token = _jwtService.GenerateJwtToken(user);
            return new AuthResult { Success = true, Token = token };
        }
    }
}
