using System.Collections.Generic;
using System.Threading.Tasks;
using LUTE_Server.Models;

namespace LUTE_Server.Repositories
{
    public interface IGameSharedVariableRepository
    {
        Task<IEnumerable<GameSharedVariable>> GetSharedVariablesAsync(int gameId);
        Task<IEnumerable<GameSharedVariable>> GetSharedVariablesForUserAsync(int gameId, string uuid);
        Task<IEnumerable<GameSharedVariable>> GetSharedVariablesByNameAsync(int gameId, string variableName);
        Task<GameSharedVariable> GetSharedVariableAsync(int id);
        Task AddSharedVariableAsync(GameSharedVariable sharedVariable);
        Task UpdateSharedVariableAsync(GameSharedVariable sharedVariable);
        Task DeleteSharedVariableAsync(int id);
    }
}
