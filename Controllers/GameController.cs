using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using LUTE_Server.Models;
using LUTE_Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace LUTE_Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameController : ControllerBase
    {
        private readonly IGameService _gameService;
        private readonly ILogger<GameController> _logger;
        private readonly IUserService _userService;

        public GameController(IGameService gameService, IUserService userService, ILogger<GameController> logger)
        {
            _gameService = gameService;
            _logger = logger;
            _userService = userService;
        }

        // Get list of games
        [HttpGet]
        [Authorize(Roles = "Admin,GameDeveloper")]
        public async Task<ActionResult<IEnumerable<object>>> GetGames()
        {
            var userName = User.Claims.FirstOrDefault(c => c.Type == "username")?.Value;
            if (string.IsNullOrEmpty(userName))
            {
                return BadRequest("Invalid user token.");
            }

            var user = await _userService.GetUserByUsernameAsync(userName);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Fetch games created by this user or if Admin, return all games
            var games = await _gameService.GetGamesAsync(user.Id);
            var simplifiedGames = games.Select(g => new
            {
                g.Id,
                g.Name,
                g.Description,
                g.CreatedAt
            });

            return Ok(simplifiedGames);
        }

        // Get a specific game by ID
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,GameDeveloper")]
        public async Task<ActionResult<object>> GetGame(int id)
        {
            var game = await _gameService.GetGameByIdAsync(id);
            if (game == null)
            {
                return NotFound();
            }

            var simplifiedGame = new
            {
                game.Id,
                game.Name,
                game.Description,
                game.CreatedAt
            };

            return Ok(simplifiedGame);
        }

        // Create a new game
        [HttpPost]
        [Authorize(Roles = "Admin,GameDeveloper")]
        public async Task<ActionResult> AddGame([FromBody] Game game)
        {
            var userName = User.Claims.FirstOrDefault(c => c.Type == "username")?.Value;
            if (string.IsNullOrEmpty(userName))
            {
                return BadRequest("Invalid user token.");
            }

            var user = await _userService.GetUserByUsernameAsync(userName);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Set the creator ID as the current user
            game.CreatedBy = user.Id;
            game.CreatedAt = System.DateTime.UtcNow;

            await _gameService.AddGameAsync(game, user.Id);
            return CreatedAtAction(nameof(GetGame), new { id = game.Id }, game);
        }

        // Update an existing game
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,GameDeveloper")]
        public async Task<ActionResult> UpdateGame(int id, [FromBody] Game game)
        {
            var userName = User.Claims.FirstOrDefault(c => c.Type == "username")?.Value;
            if (string.IsNullOrEmpty(userName))
            {
                return BadRequest("Invalid user token.");
            }

            var user = await _userService.GetUserByUsernameAsync(userName);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var existingGame = await _gameService.GetGameByIdAsync(id);
            if (existingGame == null)
            {
                return NotFound();
            }

            // Ensure the game is updated by the creator or an admin
            if (existingGame.CreatedBy != user.Id && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            // Update the game's properties
            existingGame.Name = game.Name;
            existingGame.Description = game.Description;

            await _gameService.UpdateGameAsync(existingGame, user.Id);

            return NoContent();
        }

        // Delete a game
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,GameDeveloper")]
        public async Task<ActionResult> DeleteGame(int id)
        {
            var userName = User.Claims.FirstOrDefault(c => c.Type == "username")?.Value;
            if (string.IsNullOrEmpty(userName))
            {
                return BadRequest("Invalid user token.");
            }

            var user = await _userService.GetUserByUsernameAsync(userName);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var game = await _gameService.GetGameByIdAsync(id);
            if (game == null)
            {
                return NotFound();
            }

            // Ensure only the creator or an admin can delete the game
            if (game.CreatedBy != user.Id && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            await _gameService.DeleteGameAsync(id, user.Id);
            return NoContent();
        }
    }
}
