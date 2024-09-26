using System.Collections.Generic;
using System.Threading.Tasks;
using LUTE_Server.Data;
using LUTE_Server.Models;
using Microsoft.EntityFrameworkCore;

namespace LUTE_Server.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<User>> GetUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User> GetUserByIdAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                // Handle the case where the user is not found, e.g., throw an exception or return a default value
                throw new KeyNotFoundException($"User with ID {id} not found.");
            }
            return user;
        }


        public async Task AddUserAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        // Query user by username
        public async Task<User> GetUserByUsernameAsync(string userName)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == userName);
           
            return user;
        }        

        public async Task UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
        }
    }
}
