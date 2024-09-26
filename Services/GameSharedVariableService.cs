using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LUTE_Server.Models;
using LUTE_Server.Repositories;
using Microsoft.Extensions.Logging;
using LUTE_Server.DTOs;

namespace LUTE_Server.Services
{
    public class GameSharedVariableService : IGameSharedVariableService
    {
        private readonly IGameSharedVariableRepository _repository;
        private readonly ILogger<GameSharedVariableService> _logger;

        public GameSharedVariableService(IGameSharedVariableRepository repository, ILogger<GameSharedVariableService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        // Get shared variables for a game
        public async Task<IEnumerable<GameSharedVariable>> GetSharedVariablesAsync(int gameId, string? variableType = null, int? limit = null)
        {
            var variables = await _repository.GetSharedVariablesAsync(gameId);

            if (!string.IsNullOrEmpty(variableType))
            {
                variables = variables.Where(v => v.VariableType == variableType);
            }

            if (limit.HasValue)
            {
                variables = variables.Take(limit.Value);
            }

            return variables;
        }

        // Get shared variables for a specific user in a game
        public async Task<IEnumerable<GameSharedVariable>> GetSharedVariablesForUserAsync(int gameId, string uuid, string? variableType = null, int? limit = null)
        {
            var variables = await _repository.GetSharedVariablesForUserAsync(gameId, uuid);

            if (!string.IsNullOrEmpty(variableType))
            {
                variables = variables.Where(v => v.VariableType == variableType);
            }

            if (limit.HasValue)
            {
                variables = variables.Take(limit.Value);
            }

            return variables;
        }

        // Create or update a shared variable
        public async Task CreateOrUpdateSharedVariable(int gameId, SharedVariableDto sharedVariableDto)
        {
            var existingVariable = await _repository.GetSharedVariablesByNameAsync(gameId, sharedVariableDto.VariableName);

            if (existingVariable != null && existingVariable.Any())
            {
                var updatedVariable = existingVariable.FirstOrDefault();
                if (updatedVariable != null)
                {
                    updatedVariable.Data = sharedVariableDto.Data;
                    await _repository.UpdateSharedVariableAsync(updatedVariable);
                    return;
                }
            }

            // If no existing variable, create a new one
            var newVariable = new GameSharedVariable
            {
                GameId = gameId,
                UUID = sharedVariableDto.VariableName,  // Assuming UUID and VariableName are the same, otherwise adapt it
                VariableName = sharedVariableDto.VariableName,
                VariableType = sharedVariableDto.VariableType,
                Data = sharedVariableDto.Data,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddSharedVariableAsync(newVariable);
        }

        // Delete a shared variable by name
        public async Task DeleteSharedVariable(int gameId, string variableName)
        {
            var variables = await _repository.GetSharedVariablesByNameAsync(gameId, variableName);
            var variableToDelete = variables.FirstOrDefault();
            if (variableToDelete != null)
            {
                await _repository.DeleteSharedVariableAsync(variableToDelete.Id);
            }
        }

        // Add a new shared variable
        public async Task AddSharedVariableAsync(GameSharedVariable sharedVariable)
        {
            await _repository.AddSharedVariableAsync(sharedVariable);
        }

        // Update an existing shared variable
        public async Task<GameSharedVariable?> UpdateSharedVariableAsync(GameSharedVariable sharedVariable)
        {
            var existingVariable = (await _repository.GetSharedVariableAsync(sharedVariable.Id));
            
            if (existingVariable == null)
            {
                return null;  // Return null if the shared variable doesn't exist
            }

            // Update the existing variable's properties
            existingVariable.VariableName = sharedVariable.VariableName;
            existingVariable.Data = sharedVariable.Data;
            existingVariable.CreatedAt = sharedVariable.CreatedAt;  // Update other necessary fields

            await _repository.UpdateSharedVariableAsync(existingVariable);

            return existingVariable;  // Return the updated variable
        }

        // Delete a shared variable by ID
        public async Task DeleteSharedVariableAsync(int id)
        {
            await _repository.DeleteSharedVariableAsync(id);
        }

        // Get shared variables by name for a game
        public async Task<IEnumerable<GameSharedVariable>> GetSharedVariablesByNameAsync(int gameId, string variableName)
        {
            return await _repository.GetSharedVariablesByNameAsync(gameId, variableName);
        }

        // Get a single shared variable by ID
        public async Task<GameSharedVariable> GetSharedVariableAsync(int id)
        {
            return await _repository.GetSharedVariableAsync(id);
        }
    }
}
