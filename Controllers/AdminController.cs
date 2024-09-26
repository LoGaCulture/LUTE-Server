using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LUTE_Server.Data;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using LUTE_Server.ViewModels;
using LUTE_Server.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using LUTE_Server.Models;
using System.Security.Cryptography;



namespace LUTE_Server.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminController> _logger;
        private readonly IUserService _userService;

        private readonly JwtService _jwtService;

        public AdminController(ApplicationDbContext context, ILogger<AdminController> logger, IUserService userService, JwtService jwtService)
        {
            _context = context;
            _logger = logger;
            _userService = userService;
            _jwtService = jwtService;
        }


        public async Task<IActionResult> DownloadAllData()
        {
            var csv = new StringBuilder();


            Console.WriteLine("DownloadAllData called");
            // Download Users
            csv.AppendLine("Users");
            csv.AppendLine("Id,Username,Role");
            var users = await _userService.GetUsersAsync();
            foreach (var user in users)
            {
                csv.AppendLine($"{user.Id},{user.Username},{user.Role}");
            }

            Console.WriteLine("Users downloaded");
            csv.AppendLine();

            // Download Games
            csv.AppendLine("Games");
            csv.AppendLine("Id,Name,Description,CreatedAt,CreatedBy");
            var games = _context.Games.ToList();
            foreach (var game in games)
            {
                csv.AppendLine($"{game.Id},{game.Name},{game.Description},{game.CreatedAt},{game.CreatedBy}");
            }
            csv.AppendLine();

            // Download Game Shared Variables
            csv.AppendLine("GameSharedVariables");
            csv.AppendLine("Id,GameId,UUID,VariableName,Data,CreatedAt");
            var sharedVariables = _context.GameSharedVariables.ToList();
            foreach (var variable in sharedVariables)
            {
                csv.AppendLine($"{variable.Id},{variable.GameId},{variable.UUID},{variable.VariableName},{variable.Data},{variable.CreatedAt}");
            }
            csv.AppendLine();

            csv.AppendLine("UserLogs");
            csv.AppendLine("Id,UUID,GameId,LogLevel,Message,Timestamp,AdditionalData");
            var userLogs = _context.UserLogs.ToList();
            foreach (var log in userLogs)
            {
                csv.AppendLine($"{log.Id},{log.UUID},{log.GameId},{log.LogLevel},{log.Message},{log.Timestamp},{log.AdditionalData}");
            }
            csv.AppendLine();

            var fileName = "AllData.csv";
            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", fileName);
        }

        public IActionResult Dashboard()
        {
            return View();
        }
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10)
        {

            Console.WriteLine("Admin Index called with pageNumber: " + pageNumber + " and pageSize: " + pageSize);

            var users = await _userService.GetUsersAsync();

            if (users == null)
            {
                _logger.LogError("UserService returned null. Unable to retrieve user data.");
                return View("Error", "User data could not be retrieved.");
            }

            Console.WriteLine("Users retrieved from UserService: " + users.Count());




            var pagedUsers = users.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            var totalUsers = users.Count();
            var viewModel = new PagedUserViewModel
            {
                Users = pagedUsers,
                CurrentPage = pageNumber,
                TotalPages = (int)Math.Ceiling(totalUsers / (double)pageSize)
            };


            if (viewModel.Users == null)
            {
                _logger.LogError("ViewModel Users collection is null.");
                return View("Error", "User data could not be retrieved.");
            }

            return View(viewModel);

        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            // Sign out the user and remove the authentication cookie
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Redirect to the login page
            return Redirect("/login");
        }

        public async Task<IActionResult> Sessions()
        {
            var sessions = await _context.Sessions.ToListAsync();
            return View(sessions);
        }

        public IActionResult Games()
        {
            var games = _context.Games.ToList();
            return View(games);
        }
        [HttpPost]
        public IActionResult CreateGame(string name, string description)
        {


            //this is a jwt token, with the userId as a claim
            var cookie = Request.Cookies["auth_token"];
            if (string.IsNullOrEmpty(cookie))
            {
                // Handle the case where the cookie is null or empty
                return BadRequest("Authentication token is missing.");
            }
            var userIdClaim = _jwtService.GetClaimFromToken("userId", cookie);
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest("Invalid user ID in token.");
            }


            var newGame = new Game
            {
                Id = Guid.NewGuid().ToString(),  // Generate a new GUID as a string for Id
                Name = name,
                Description = description,
                CreatedBy = userId, 
                CreatedAt = DateTime.UtcNow,
                SecretKey = GenerateSecureToken()  // Generate secure token during game creation
            };

            // Log the new game
            Console.WriteLine("New Game Created: " + newGame.Id);

            _context.Games.Add(newGame);
            _context.SaveChanges();

            return RedirectToAction("Games");
        }

        // Helper to generate a secure token (private key)
        private string GenerateSecureToken()
        {
            var tokenData = new byte[32];  // 256-bit token
            RandomNumberGenerator.Fill(tokenData);  // Fill the byte array with cryptographically secure random numbers
            return Convert.ToBase64String(tokenData);  // Return as base64 string
        }

        // Regenerate token for a game
        [HttpPost]
        public IActionResult RegenerateToken(string gameId)
        {
            var game = _context.Games.FirstOrDefault(g => g.Id == gameId);
            if (game == null)
            {
                return NotFound();
            }

            // Generate a secure token (256-bit, 32 bytes)
            var tokenData = new byte[32];  // 256-bit token
            RandomNumberGenerator.Fill(tokenData);

            // Convert token to a base64 string
            var secureToken = Convert.ToBase64String(tokenData);
            game.SecretKey = secureToken;

            _context.SaveChanges();

            return RedirectToAction("Games");  // Redirect back to games list
        }


        // Download secrets.txt for a game
        [HttpGet("download-secrets/{gameId}")]
        public IActionResult DownloadSecrets(Guid gameId)
        {
            // Log incoming gameId for debug
            Console.WriteLine($"DownloadSecrets called with gameId: {gameId}");

            var game = _context.Games.FirstOrDefault(g => g.Id == gameId.ToString());
            if (game == null)
            {
                return NotFound("Game not found.");
            }

            // Prepare the secrets.txt content
            var content = $"ServerAddress={Request.Host}\nSecretKey={game.SecretKey}";
            var fileName = "secrets.txt";
            var fileBytes = System.Text.Encoding.UTF8.GetBytes(content);

            return File(fileBytes, "text/plain", fileName);
        }

        public IActionResult GameSharedVariables()
        {
            var sharedVariables = _context.GameSharedVariables.ToList();
            return View(sharedVariables);
        }
        public IActionResult UserLogs(int pageNumber = 1, string? uuid = null, string? gameId = null)
        {
            int pageSize = 10;  // Number of logs per page
            IQueryable<UserLog> query = _context.UserLogs;

            // Apply filters if UUID or GameId is provided
            if (!string.IsNullOrEmpty(uuid))
            {
                query = query.Where(log => log.UUID == uuid);
            }

            if (!string.IsNullOrEmpty(gameId))
            {
                query = query.Where(log => log.GameId == gameId);
            }

            // Pagination logic
            int totalLogs = query.Count();
            var logs = query.Skip((pageNumber - 1) * pageSize)
                            .Take(pageSize)
                            .ToList();

            // Transform UserLogs to UserLogWithGameName
            var logsWithGameNames = logs.Select(log => new UserLogWithGameName
            {
                Log = log,
                GameName = _context.Games.FirstOrDefault(g => g.Id == log.GameId)?.Name ?? "Unknown Game" // Fetch game name by GameId
            }).ToList();

            var viewModel = new PagedUserLogViewModel
            {
                Logs = logsWithGameNames,  // Use logs with game names
                CurrentPage = pageNumber,
                TotalPages = (int)Math.Ceiling((double)totalLogs / pageSize),
                UUIDFilter = uuid ?? string.Empty,
                GameIdFilter = gameId ?? string.Empty
            };

            return View(viewModel);  // Return view with filtered and paginated logs
        }

        public IActionResult DownloadGameSharedVariables()
        {
            var sharedVariables = _context.GameSharedVariables.ToList();

            var csv = new StringBuilder();
            csv.AppendLine("Id,GameId,UUID,VariableName,Data,CreatedAt");

            foreach (var variable in sharedVariables)
            {
                csv.AppendLine($"{variable.Id},{variable.GameId},{variable.UUID},{variable.VariableName},{variable.Data},{variable.CreatedAt}");
            }

            var fileName = "GameSharedVariables.csv";
            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", fileName);
        }

        [HttpGet("edit/{gameId}")]
        public IActionResult EditGame(Guid gameId)
        {
            var game = _context.Games.FirstOrDefault(g => g.Id == gameId.ToString());
            if (game == null)
            {
                return NotFound("Game not found.");
            }

            return View(game); // Return the view with the game model
        }

        [HttpPost("edit/{gameId}")]
        public IActionResult EditGame(Guid gameId, Game updatedGame)
        {
            var game = _context.Games.FirstOrDefault(g => g.Id == gameId.ToString());
            if (game == null)
            {
                return NotFound("Game not found.");
            }

            // Update the game fields
            game.Name = updatedGame.Name;
            game.Description = updatedGame.Description;

            _context.SaveChanges();

            return RedirectToAction("Games");
        }

        [HttpPost("delete/{gameId}")]
        public IActionResult DeleteGame(Guid gameId)
        {
            var game = _context.Games.FirstOrDefault(g => g.Id == gameId.ToString());
            if (game == null)
            {
                return NotFound("Game not found.");
            }

            _context.Games.Remove(game);
            _context.SaveChanges();

            return RedirectToAction("Games");
        }




    }
}
