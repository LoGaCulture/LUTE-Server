using System.Collections.Generic;
using System.Threading.Tasks;
using LUTE_Server.DTOs;
using LUTE_Server.Models;
using LUTE_Server.Repositories;
using Microsoft.AspNetCore.Identity;

namespace LUTE_Server.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly JwtService _jwtService;
        private readonly IPasswordHasher<User> _passwordHasher;

        public UserService(IUserRepository userRepository, JwtService jwtService, IPasswordHasher<User> passwordHasher)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
            _passwordHasher = passwordHasher;
        }

        public async Task<IEnumerable<User>> GetUsersAsync()
        {
            return await _userRepository.GetUsersAsync();
        }

        public async Task<User> GetUserByIdAsync(int id)
        {
            return await _userRepository.GetUserByIdAsync(id);
        }

        public async Task<User?> GetUserByUsernameAsync(string userName)
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
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

            await _userRepository.AddUserAsync(user);

            var token = _jwtService.GenerateJwtToken(user);
            return new AuthResult { Success = true, Token = token };
        }

        public async Task<AuthResult> LoginUserAsync(LoginRequest request)
        {

            var user = await _userRepository.GetUserByUsernameAsync(request.Username);
            if (user == null)
            {
                return new AuthResult { Success = false, ErrorMessage = "Invalid username or password" };
            }

            var verification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (verification == PasswordVerificationResult.Failed)
            {
                return new AuthResult { Success = false, ErrorMessage = "Invalid username or password" };
            }

            // PasswordHasher signals it should be rehashed (e.g. iteration count upgraded)
            if (verification == PasswordVerificationResult.SuccessRehashNeeded)
            {
                user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
                await _userRepository.UpdateUserAsync(user);
            }

            var token = _jwtService.GenerateJwtToken(user);
            return new AuthResult { Success = true, Token = token };
        }
    }
}
