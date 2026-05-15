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

            // Download Users
            csv.AppendLine("Users");
            csv.AppendLine("Id,Username,Role");
            var users = await _userService.GetUsersAsync();
            foreach (var user in users)
            {
                csv.AppendLine(string.Join(",", EscapeCsv(user.Id), EscapeCsv(user.Username), EscapeCsv(user.Role)));
            }

            csv.AppendLine();

            // Download Games
            csv.AppendLine("Games");
            csv.AppendLine("Id,Name,Description,CreatedAt,CreatedBy");
            var games = _context.Games.ToList();
            foreach (var game in games)
            {
                csv.AppendLine(string.Join(",", EscapeCsv(game.Id), EscapeCsv(game.Name), EscapeCsv(game.Description), EscapeCsv(game.CreatedAt), EscapeCsv(game.CreatedBy)));
            }
            csv.AppendLine();

            // Download Game Shared Variables
            csv.AppendLine("SharedVariables");
            csv.AppendLine("Id,GameId,UUID,VariableName,Data,CreatedAt");
            var sharedVariables = _context.SharedVariables.ToList();
            foreach (var variable in sharedVariables)
            {
                csv.AppendLine(string.Join(",", EscapeCsv(variable.Id), EscapeCsv(variable.GameId), EscapeCsv(variable.UUID), EscapeCsv(variable.VariableName), EscapeCsv(variable.Data), EscapeCsv(variable.CreatedAt)));
            }
            csv.AppendLine();

            csv.AppendLine("UserLogs");
            csv.AppendLine("Id,UUID,GameId,LogLevel,Message,Timestamp,AdditionalData");
            var userLogs = _context.UserLogs.ToList();
            foreach (var log in userLogs)
            {
                csv.AppendLine(string.Join(",", EscapeCsv(log.Id), EscapeCsv(log.UUID), EscapeCsv(log.GameId), EscapeCsv(log.LogLevel), EscapeCsv(log.Message), EscapeCsv(log.Timestamp), EscapeCsv(log.AdditionalData)));
            }
            csv.AppendLine();

            var fileName = "AllData.csv";
            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", fileName);
        }

        /// <summary>
        /// Filtered CSV download dispatcher — used by the Dashboard downloads form.
        /// Supports type: "logs", "variables", or "all".
        /// Optional filters: gameId, datePreset (all/today/7days/30days/custom), fromDate, toDate.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Download(
            string? type,
            string? gameId,
            string? datePreset,
            DateTime? fromDate,
            DateTime? toDate)
        {
            // Resolve date range
            DateTime? startDate = null;
            DateTime? endDate = null;
            switch (datePreset)
            {
                case "today":
                    startDate = DateTime.UtcNow.Date;
                    endDate = DateTime.UtcNow.Date.AddDays(1);
                    break;
                case "7days":
                    startDate = DateTime.UtcNow.AddDays(-7);
                    break;
                case "30days":
                    startDate = DateTime.UtcNow.AddDays(-30);
                    break;
                case "custom":
                    startDate = fromDate.HasValue ? fromDate.Value.Date : (DateTime?)null;
                    endDate = toDate.HasValue ? toDate.Value.Date.AddDays(1) : (DateTime?)null;
                    break;
            }

            var gameFilter = !string.IsNullOrEmpty(gameId) ? gameId : null;
            var csv = new StringBuilder();
            var resolvedType = type?.ToLower() switch {
                "variables" => "variables",
                "all"       => "all",
                _           => "logs"
            };

            if (resolvedType == "all")
            {
                // Users (no date/game filter — always full set)
                csv.AppendLine("Users");
                csv.AppendLine("Id,Username,Role");
                var allUsers = await _userService.GetUsersAsync();
                foreach (var u in allUsers ?? Enumerable.Empty<User>())
                    csv.AppendLine(string.Join(",", EscapeCsv(u.Id), EscapeCsv(u.Username), EscapeCsv(u.Role)));
                csv.AppendLine();

                // Games (no date/game filter)
                csv.AppendLine("Games");
                csv.AppendLine("Id,Name,Description,CreatedAt,CreatedBy");
                var allGames = _context.Games.ToList();
                foreach (var g in allGames)
                    csv.AppendLine(string.Join(",", EscapeCsv(g.Id), EscapeCsv(g.Name), EscapeCsv(g.Description), EscapeCsv(g.CreatedAt), EscapeCsv(g.CreatedBy)));
                csv.AppendLine();
            }

            if (resolvedType == "variables" || resolvedType == "all")
            {
                IQueryable<SharedVariable> svQuery = _context.SharedVariables;
                if (gameFilter != null) svQuery = svQuery.Where(v => v.GameId == gameFilter);
                if (startDate.HasValue) svQuery = svQuery.Where(v => v.CreatedAt >= startDate.Value);
                if (endDate.HasValue) svQuery = svQuery.Where(v => v.CreatedAt < endDate.Value);
                var vars = svQuery.ToList();

                if (resolvedType == "all") csv.AppendLine("SharedVariables");
                csv.AppendLine("Id,GameId,UUID,VariableName,Data,CreatedAt");
                foreach (var v in vars)
                    csv.AppendLine(string.Join(",", EscapeCsv(v.Id), EscapeCsv(v.GameId), EscapeCsv(v.UUID), EscapeCsv(v.VariableName), EscapeCsv(v.Data), EscapeCsv(v.CreatedAt)));
                if (resolvedType == "all") csv.AppendLine();
            }

            if (resolvedType == "logs" || resolvedType == "all")
            {
                IQueryable<UserLog> logsQuery = _context.UserLogs;
                if (gameFilter != null) logsQuery = logsQuery.Where(l => l.GameId == gameFilter);
                if (startDate.HasValue) logsQuery = logsQuery.Where(l => l.Timestamp >= startDate.Value);
                if (endDate.HasValue) logsQuery = logsQuery.Where(l => l.Timestamp < endDate.Value);
                var logs = logsQuery.ToList();

                if (resolvedType == "all") csv.AppendLine("UserLogs");
                csv.AppendLine("Id,UUID,GameId,LogLevel,Message,Timestamp,AdditionalData");
                foreach (var log in logs)
                    csv.AppendLine(string.Join(",", EscapeCsv(log.Id), EscapeCsv(log.UUID), EscapeCsv(log.GameId), EscapeCsv(log.LogLevel), EscapeCsv(log.Message), EscapeCsv(log.Timestamp), EscapeCsv(log.AdditionalData)));
            }

            var typeSuffix = resolvedType switch { "variables" => "shared-variables", "all" => "all-data", _ => "logs" };
            var gameSuffix = gameFilter != null ? $"-{gameFilter[..Math.Min(8, gameFilter.Length)]}" : "";
            var fileName = $"{typeSuffix}{gameSuffix}.csv";
            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", fileName);
        }

        public async Task<IActionResult> Dashboard()
        {
            var users = await _userService.GetUsersAsync();
            var totalUsers = users?.Count() ?? 0;
            var totalGames = await _context.Games.CountAsync();
            var totalLogs = await _context.UserLogs.CountAsync();
            var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
            var logsLast7Days = await _context.UserLogs.CountAsync(l => l.Timestamp >= sevenDaysAgo);
            var gamesList = await _context.Games.OrderBy(g => g.Name).ToListAsync();

            var viewModel = new DashboardViewModel
            {
                TotalUsers = totalUsers,
                TotalGames = totalGames,
                TotalLogs = totalLogs,
                LogsLast7Days = logsLast7Days,
                Games = gamesList
            };
            return View(viewModel);
        }

        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10)
        {

            var users = await _userService.GetUsersAsync();

            if (users == null)
            {
                _logger.LogError("UserService returned null. Unable to retrieve user data.");
                return View("Error", "User data could not be retrieved.");
            }


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
        [ValidateAntiForgeryToken]
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
        [ValidateAntiForgeryToken]
        public IActionResult CreateGame(string name, string description)
        {
            //this is a jwt token, with the userId as a claim
            var cookie = Request.Cookies["auth_token"];
            if (string.IsNullOrEmpty(cookie))
            {
                return BadRequest("Authentication token is missing.");
            }
            var userIdClaim = _jwtService.GetClaimFromToken("userId", cookie);
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest("Invalid user ID in token.");
            }

            var newGame = new Game
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                Description = description,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow,
                SecretKey = GenerateSecureToken()
            };

            _context.Games.Add(newGame);
            _context.SaveChanges();

            TempData["SuccessMessage"] = $"Game \"{name}\" created.";
            return RedirectToAction("Games");
        }

        // Helper to generate a secure token (private key)
        private string GenerateSecureToken()
        {
            var tokenData = new byte[32];  // 256-bit token
            RandomNumberGenerator.Fill(tokenData);
            return Convert.ToBase64String(tokenData);
        }

        // Regenerate token for a game
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RegenerateToken(string gameId)
        {
            var game = _context.Games.FirstOrDefault(g => g.Id == gameId);
            if (game == null)
            {
                return NotFound();
            }

            var tokenData = new byte[32];
            RandomNumberGenerator.Fill(tokenData);
            game.SecretKey = Convert.ToBase64String(tokenData);

            _context.SaveChanges();

            TempData["SuccessMessage"] = "Secret key regenerated.";
            return RedirectToAction("Games");
        }

        // Download secrets.txt for a game
        [HttpGet("download-secrets/{gameId}")]
        public IActionResult DownloadSecrets(Guid gameId)
        {
            var game = _context.Games.FirstOrDefault(g => g.Id == gameId.ToString());
            if (game == null)
            {
                return NotFound("Game not found.");
            }

            var content = $"ServerAddress={Request.Host}\nSecretKey={game.SecretKey}";
            var fileName = "secrets.txt";
            var fileBytes = System.Text.Encoding.UTF8.GetBytes(content);

            return File(fileBytes, "text/plain", fileName);
        }

        public IActionResult SharedVariables()
        {
            var games = _context.Games.OrderBy(g => g.Name).ToList();
            return View(games);
        }

        /// <summary>
        /// Partial view for the middle pane of the Shared Variables three-pane layout.
        /// Returns distinct variable names for the given game, loaded by htmx.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> SharedVariablesVariablesPartial(string gameId)
        {
            var variables = await _context.SharedVariables
                .Where(v => v.GameId == gameId)
                .Select(v => v.VariableName)
                .Distinct()
                .OrderBy(v => v)
                .ToListAsync();
            ViewData["GameId"] = gameId;
            return PartialView("_SharedVariablesVariables", (IEnumerable<string>)variables);
        }

        /// <summary>
        /// Partial view for the right pane of the Shared Variables three-pane layout.
        /// Returns data rows for the given game + variable name, loaded by htmx.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> SharedVariablesDataPartial(string gameId, string variableName)
        {
            var data = await _context.SharedVariables
                .Where(v => v.GameId == gameId && v.VariableName == variableName)
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync();
            ViewData["GameId"] = gameId;
            ViewData["VariableName"] = variableName;
            return PartialView("_SharedVariablesData", (IEnumerable<SharedVariable>)data);
        }

        public async Task<IActionResult> UserLogs(
            int pageNumber = 1,
            string? uuid = null,
            string? gameId = null,
            string[]? logLevel = null,
            string? search = null,
            string? datePreset = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null,
            string? sortBy = "timestamp",
            string? sortDir = "desc")
        {
            const int pageSize = 20;
            IQueryable<UserLog> query = _context.UserLogs;

            // --- filters ---
            if (!string.IsNullOrEmpty(uuid))
                query = query.Where(l => l.UUID == uuid);
            if (!string.IsNullOrEmpty(gameId))
                query = query.Where(l => l.GameId == gameId);
            if (logLevel != null && logLevel.Length > 0)
            {
                // Normalize to upper-case on both sides so "info", "Info", and "INFO" all match the chip values.
                var normalizedLevels = logLevel.Select(l => l.ToUpperInvariant()).ToArray();
                query = query.Where(l => l.LogLevel != null && normalizedLevels.Contains(l.LogLevel.ToUpper()));
            }
            if (!string.IsNullOrEmpty(search))
                query = query.Where(l => l.Message != null && l.Message.Contains(search));

            // --- date range ---
            DateTime? startDate = null;
            DateTime? endDate = null;
            switch (datePreset)
            {
                case "today":   startDate = DateTime.UtcNow.Date; endDate = startDate.Value.AddDays(1); break;
                case "7days":   startDate = DateTime.UtcNow.AddDays(-7); break;
                case "30days":  startDate = DateTime.UtcNow.AddDays(-30); break;
                case "custom":
                    startDate = dateFrom?.Date;
                    endDate   = dateTo?.Date.AddDays(1);
                    break;
            }
            if (startDate.HasValue) query = query.Where(l => l.Timestamp >= startDate.Value);
            if (endDate.HasValue)   query = query.Where(l => l.Timestamp < endDate.Value);

            // --- sort ---
            query = (sortBy?.ToLower(), sortDir?.ToLower()) switch {
                ("level", "asc")  => query.OrderBy(l => l.LogLevel).ThenByDescending(l => l.Timestamp),
                ("level", _)      => query.OrderByDescending(l => l.LogLevel).ThenByDescending(l => l.Timestamp),
                (_, "asc")        => query.OrderBy(l => l.Timestamp),
                _                 => query.OrderByDescending(l => l.Timestamp)
            };

            // --- paginate ---
            int totalLogs = await query.CountAsync();
            var logs = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // --- resolve game names ---
            var gameNameMap = await _context.Games.ToDictionaryAsync(g => g.Id, g => g.Name);
            var logsWithGameNames = logs.Select(log => new UserLogWithGameName
            {
                Log = log,
                GameName = gameNameMap.GetValueOrDefault(log.GameId ?? "", "Unknown")
            }).ToList();

            // --- per-game log counts for the dropdown (unfiltered totals) ---
            var rawCounts = await _context.UserLogs
                .GroupBy(l => l.GameId)
                .Select(g => new { GameId = g.Key, Count = g.Count() })
                .ToListAsync();
            var gameCounts = rawCounts
                .Select(rc => new GameLogCount {
                    GameId = rc.GameId ?? "",
                    Name   = gameNameMap.GetValueOrDefault(rc.GameId ?? "", "Unknown"),
                    Count  = rc.Count
                })
                .OrderBy(gc => gc.Name)
                .ToList();

            var viewModel = new PagedUserLogViewModel
            {
                Logs             = logsWithGameNames,
                CurrentPage      = pageNumber,
                TotalPages       = (int)Math.Ceiling((double)totalLogs / pageSize),
                UUIDFilter       = uuid ?? string.Empty,
                GameIdFilter     = gameId ?? string.Empty,
                LogLevelFilters  = logLevel,
                SearchFilter     = search,
                DatePreset       = datePreset,
                DateFrom         = dateFrom,
                DateTo           = dateTo,
                SortBy           = sortBy ?? "timestamp",
                SortDir          = sortDir ?? "desc",
                GameCounts       = gameCounts
            };

            // htmx partial response — return only the table fragment
            if (Request.Headers.ContainsKey("HX-Request"))
                return PartialView("_UserLogsTable", viewModel);

            return View(viewModel);
        }

        public IActionResult DownloadSharedVariables()
        {
            var sharedVariables = _context.SharedVariables.ToList();

            var csv = new StringBuilder();
            csv.AppendLine("Id,GameId,UUID,VariableName,Data,CreatedAt");

            foreach (var variable in sharedVariables)
            {
                csv.AppendLine(string.Join(",", EscapeCsv(variable.Id), EscapeCsv(variable.GameId), EscapeCsv(variable.UUID), EscapeCsv(variable.VariableName), EscapeCsv(variable.Data), EscapeCsv(variable.CreatedAt)));
            }

            var fileName = "SharedVariables.csv";
            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", fileName);
        }

        /// <summary>
        /// Escapes a value for CSV output.
        /// - Prefixes formula-injection characters (=, +, -, @, tab, CR) with a single quote.
        /// - Wraps fields containing commas, double-quotes, or newlines in double-quotes,
        ///   with internal double-quotes doubled per RFC 4180.
        /// </summary>
        private static string EscapeCsv(object? value)
        {
            if (value is null) return "";
            var s = value.ToString() ?? "";
            // Guard against Excel formula injection
            if (s.Length > 0 && "=+-@\t\r".IndexOf(s[0]) >= 0)
                s = "'" + s;
            // Standard CSV quoting when the cell contains delimiter, quote, or newline
            if (s.Contains(',') || s.Contains('"') || s.Contains('\n') || s.Contains('\r'))
                s = "\"" + s.Replace("\"", "\"\"") + "\"";
            return s;
        }

        [HttpGet("edit/{gameId}")]
        public IActionResult EditGame(Guid gameId)
        {
            var game = _context.Games.FirstOrDefault(g => g.Id == gameId.ToString());
            if (game == null)
            {
                return NotFound("Game not found.");
            }

            return View(game);
        }

        [HttpPost("edit/{gameId}")]
        [ValidateAntiForgeryToken]
        public IActionResult EditGame(Guid gameId, Game updatedGame)
        {
            var game = _context.Games.FirstOrDefault(g => g.Id == gameId.ToString());
            if (game == null)
            {
                return NotFound("Game not found.");
            }

            game.Name = updatedGame.Name;
            game.Description = updatedGame.Description;

            _context.SaveChanges();

            TempData["SuccessMessage"] = "Game updated.";
            return RedirectToAction("Games");
        }

        [HttpPost("delete/{gameId}")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteGame(Guid gameId)
        {
            var game = _context.Games.FirstOrDefault(g => g.Id == gameId.ToString());
            if (game == null)
            {
                return NotFound("Game not found.");
            }

            _context.Games.Remove(game);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Game deleted.";
            return RedirectToAction("Games");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUserRole(int userId, int newRole)
        {
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            user.Role = (UserRole)newRole;
            await _userService.UpdateUserAsync(user);

            return RedirectToAction("Index");
        }
    }
}
