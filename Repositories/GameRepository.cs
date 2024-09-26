using System.Collections.Generic;
using System.Threading.Tasks;
using LUTE_Server.Data;
using LUTE_Server.Models;
using Microsoft.EntityFrameworkCore;

namespace LUTE_Server.Repositories
{
    public class GameRepository : IGameRepository
    {
        private readonly ApplicationDbContext _context;

        public GameRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        // Filter games based on the userId (CreatedBy field)
        public async Task<IEnumerable<Game>> GetGamesByUserIdAsync(int userId)
        {
            return await _context.Games.Where(g => g.CreatedBy == userId).ToListAsync();
        }
        public async Task<IEnumerable<Game>> GetGamesAsync()
        {
            return await _context.Games.ToListAsync();
        }

        public async Task<Game> GetGameByIdAsync(int id)
        {
            var game = await _context.Games.FindAsync(id);
            if (game == null)
            {
                throw new KeyNotFoundException($"Game with ID {id} not found.");
            }
            return game;
        }

        public async Task AddGameAsync(Game game)
        {
            await _context.Games.AddAsync(game);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateGameAsync(Game game)
        {
            _context.Games.Update(game);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteGameAsync(int id)
        {
            var game = await _context.Games.FindAsync(id);
            if (game != null)
            {
                _context.Games.Remove(game);
                await _context.SaveChangesAsync();
            }
        }
    }
}
