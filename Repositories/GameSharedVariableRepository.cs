using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LUTE_Server.Data;
using LUTE_Server.Models;
using Microsoft.EntityFrameworkCore;

namespace LUTE_Server.Repositories
{
    public class GameSharedVariableRepository : IGameSharedVariableRepository
    {
        private readonly ApplicationDbContext _context;

        public GameSharedVariableRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<GameSharedVariable>> GetSharedVariablesAsync(int gameId)
        {
            return await _context.GameSharedVariables
                .Where(v => v.GameId == gameId)
                .ToListAsync();
        }

        public async Task<IEnumerable<GameSharedVariable>> GetSharedVariablesForUserAsync(int gameId, string uuid)
        {
            return await _context.GameSharedVariables
                .Where(v => v.GameId == gameId && v.UUID == uuid)
                .ToListAsync();
        }

        public async Task<IEnumerable<GameSharedVariable>> GetSharedVariablesByNameAsync(int gameId, string variableName)
        {
            return await _context.GameSharedVariables
                .Where(v => v.GameId == gameId && v.VariableName == variableName)
                .ToListAsync();
        }

        public async Task<GameSharedVariable> GetSharedVariableAsync(int id)
        {
            var sharedVariable = await _context.GameSharedVariables.FindAsync(id);
            if (sharedVariable == null)
            {
                throw new KeyNotFoundException($"GameSharedVariable with id {id} not found.");
            }
            return sharedVariable;
        }

        public async Task AddSharedVariableAsync(GameSharedVariable sharedVariable)
        {
            await _context.GameSharedVariables.AddAsync(sharedVariable);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateSharedVariableAsync(GameSharedVariable sharedVariable)
        {
            _context.GameSharedVariables.Update(sharedVariable);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteSharedVariableAsync(int id)
        {
            var sharedVariable = await _context.GameSharedVariables.FindAsync(id);
            if (sharedVariable != null)
            {
                _context.GameSharedVariables.Remove(sharedVariable);
                await _context.SaveChangesAsync();
            }
        }
    }
}
