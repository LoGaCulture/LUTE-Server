using System.Collections.Generic;
using System.Threading.Tasks;
using LUTE_Server.DTOs;
using LUTE_Server.Models;

namespace LUTE_Server.Services
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetUsersAsync();
        Task<User> GetUserByIdAsync(int id);
        Task<User?> GetUserByUsernameAsync(string userName); 
        Task AddUserAsync(User user);
        Task UpdateUserAsync(User user);
        Task DeleteUserAsync(int id);
        Task<AuthResult> RegisterUserAsync(RegisterRequest request);
        Task<AuthResult> LoginUserAsync(LoginRequest request);
    }
}