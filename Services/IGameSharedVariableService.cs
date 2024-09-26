using System.Collections.Generic;
using System.Threading.Tasks;
using LUTE_Server.Models;
using LUTE_Server.DTOs;

namespace LUTE_Server.Services
{
    public interface IGameSharedVariableService
    {
        Task<IEnumerable<GameSharedVariable>> GetSharedVariablesAsync(int gameId, string? variableType = null, int? limit = null);  
        Task<IEnumerable<GameSharedVariable>> GetSharedVariablesForUserAsync(int gameId, string uuid, string? variableType = null, int? limit = null); 

        Task CreateOrUpdateSharedVariable(int gameId, SharedVariableDto sharedVariableDto);  
        Task DeleteSharedVariable(int gameId, string variableName);  

        Task AddSharedVariableAsync(GameSharedVariable sharedVariable);  
        Task<GameSharedVariable?> UpdateSharedVariableAsync(GameSharedVariable sharedVariable); 

        Task DeleteSharedVariableAsync(int id);  
        Task<IEnumerable<GameSharedVariable>> GetSharedVariablesByNameAsync(int gameId, string variableName); 
        
        Task<GameSharedVariable> GetSharedVariableAsync(int id);  
    }
}
