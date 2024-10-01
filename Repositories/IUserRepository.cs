using System.Collections.Generic;
using System.Threading.Tasks;
using LUTE_Server.Models;

namespace LUTE_Server.Repositories
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetUsersAsync();
        Task<User> GetUserByIdAsync(int id);
        Task<User?> GetUserByUsernameAsync(string username); 
        Task AddUserAsync(User user);
        Task UpdateUserAsync(User user);
        Task DeleteUserAsync(int id);


        
    }
}
