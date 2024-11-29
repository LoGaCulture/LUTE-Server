using System.Collections.Generic;
using System.Threading.Tasks;
using LUTE_Server.Models;

namespace LUTE_Server.Services
{
    public interface ISharedVariableService
    {
        Task SaveSharedVariableAsync(SharedVariable sharedVariable);
        Task SaveSharedVariablesAsync(IEnumerable<SharedVariable> sharedVariables);
        Task<List<SharedVariable>> GetSharedVariablesAsync(string gameId, int count = 10);
        Task<List<SharedVariable>> GetSharedVariablesForUserAsync(string gameId, string uuid);
        Task<bool> DeleteSharedVariableAsync(string id);
    }
}
