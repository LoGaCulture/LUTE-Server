using System.Collections.Generic;
using System.Threading.Tasks;
using LUTE_Server.Models;
using LUTE_Server.Repositories;
using Microsoft.Extensions.Logging;

namespace LUTE_Server.Services
{
    public class GameService : IGameService
    {
        private readonly IGameRepository _gameRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<GameService> _logger;

        public GameService(IGameRepository gameRepository, IUserRepository userRepository, ILogger<GameService> logger)
        {
            _gameRepository = gameRepository;
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<Game>> GetGamesAsync(int userId)
        {
            return await _gameRepository.GetGamesByUserIdAsync(userId);
        }

        public async Task<Game> GetGameByIdAsync(int id)
        {
            return await _gameRepository.GetGameByIdAsync(id);
        }

        public async Task AddGameAsync(Game game, int userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null || (user.Role != UserRole.Admin && user.Role != UserRole.GameDeveloper))
            {
                _logger.LogWarning("Unauthorized attempt to add a game by user ID: {UserId}", userId);
                return;
            }

            game.CreatedBy = userId;
            await _gameRepository.AddGameAsync(game);
        }

        public async Task UpdateGameAsync(Game game, int userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null || (user.Role != UserRole.Admin && user.Role != UserRole.GameDeveloper))
            {
                _logger.LogWarning("Unauthorized attempt to update a game by user ID: {UserId}", userId);
                return;
            }

            if (user.Role == UserRole.GameDeveloper && game.CreatedBy != userId)
            {
                _logger.LogWarning("Game developer user ID: {UserId} attempting to update a game not created by them", userId);
                return;
            }

            // Ensure the game is actually being updated in the database
            await _gameRepository.UpdateGameAsync(game);
        }


        public async Task DeleteGameAsync(int id, int userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null || (user.Role != UserRole.Admin && user.Role != UserRole.GameDeveloper))
            {
                _logger.LogWarning("Unauthorized attempt to delete a game by user ID: {UserId}", userId);
                return;
            }

            var game = await _gameRepository.GetGameByIdAsync(id);
            if (game == null || (user.Role == UserRole.GameDeveloper && game.CreatedBy != userId))
            {
                _logger.LogWarning("Game developer user ID: {UserId} attempting to delete a game not created by them or game does not exist", userId);
                return;
            }

            await _gameRepository.DeleteGameAsync(id);
        }
    }
}
