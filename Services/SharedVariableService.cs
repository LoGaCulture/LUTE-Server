using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LUTE_Server.Models;
using LUTE_Server.Data;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace LUTE_Server.Services
{
    public class SharedVariableService : ISharedVariableService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SharedVariableService> _logger;

        public SharedVariableService(ApplicationDbContext context, ILogger<SharedVariableService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Save a single shared variable
        public async Task SaveSharedVariableAsync(SharedVariable sharedVariable)
        {
            _logger.LogInformation("Saving shared variable: {VariableName} for GameId: {GameId}", sharedVariable.VariableName, sharedVariable.GameId);
            await _context.SharedVariables.AddAsync(sharedVariable);
            await _context.SaveChangesAsync();
        }

        // Save multiple shared variables in bulk
        public async Task SaveSharedVariablesAsync(IEnumerable<SharedVariable> sharedVariables)
        {
            _logger.LogInformation("Saving {Count} shared variables", sharedVariables.Count());

            foreach (var variable in sharedVariables)
            {
                _context.SharedVariables.Add(variable);
            }

            await _context.SaveChangesAsync();
        }

        // Get shared variables for a specific game
        public async Task<List<SharedVariable>> GetSharedVariablesAsync(string gameId, int count = 10)
        {
            _logger.LogInformation("Retrieving up to {Count} shared variables for GameId: {GameId}", count, gameId);
            return await _context.SharedVariables
                .Where(v => v.GameId == gameId)
                .OrderByDescending(v => v.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        // Get shared variables for a specific user
        public async Task<List<SharedVariable>> GetSharedVariablesForUserAsync(string gameId, string uuid)
        {
            _logger.LogInformation("Retrieving shared variables for GameId: {GameId}, UUID: {UUID}", gameId, uuid);
            return await _context.SharedVariables
                .Where(v => v.GameId == gameId && v.UUID == uuid)
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync();
        }

        // Delete a shared variable
        public async Task<bool> DeleteSharedVariableAsync(string id)
        {
            var variable = await _context.SharedVariables.FindAsync(id);

            if (variable == null)
            {
                _logger.LogWarning("Attempted to delete a non-existent shared variable with Id: {Id}", id);
                return false;
            }

            _context.SharedVariables.Remove(variable);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Deleted shared variable with Id: {Id}", id);
            return true;
        }
    }
}
