using System.Collections.Generic;
using System.Threading.Tasks;
using LUTE_Server.Models;

namespace LUTE_Server.Repositories
{
    public interface IGameRepository
    {
        Task<IEnumerable<Game>> GetGamesByUserIdAsync(int userId); 
        Task<Game> GetGameByIdAsync(int id);
        Task AddGameAsync(Game game);
        Task UpdateGameAsync(Game game);
        Task DeleteGameAsync(int id);
    }
}
