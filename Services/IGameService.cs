using System.Collections.Generic;
using System.Threading.Tasks;
using LUTE_Server.Models;

namespace LUTE_Server.Services
{
    public interface IGameService
    {
        Task<IEnumerable<Game>> GetGamesAsync(int userId);
        Task<Game> GetGameByIdAsync(int id);
        Task AddGameAsync(Game game, int userId);
        Task UpdateGameAsync(Game game, int userId);
        Task DeleteGameAsync(int id, int userId);
        
    }
}
