using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using LUTE_Server.Models;
using LUTE_Server.Services;
using Microsoft.Extensions.Logging;
using System.Linq;
using LUTE_Server.Data;
using Microsoft.AspNetCore.Authorization;
using LUTE_Server.ViewModels;
using System.Text;
using Microsoft.EntityFrameworkCore;  // For FirstOrDefault

namespace LUTE_Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserLogController : Controller
    {
        private readonly ILoggingService _loggingService;
        private readonly ILogger<UserLogController> _logger;
        private readonly ApplicationDbContext _context;  // Add DbContext to access games and their secret keys

        public UserLogController(ILoggingService loggingService, ILogger<UserLogController> logger, ApplicationDbContext context)
        {
            _loggingService = loggingService;
            _logger = logger;
            _context = context;  // Initialize DbContext
        }

        [HttpPost]
        public async Task<ActionResult> ReceiveLogs([FromHeader(Name = "X-Secret-Key")] string secretKey, [FromBody] UserLogDto[] logs)
        {
            // Validate secret key
            if (string.IsNullOrEmpty(secretKey))
            {
                _logger.LogWarning("Missing secret key.");
                return BadRequest("Missing secret key.");
            }

            // Find game with matching secret key
            var game = _context.Games.FirstOrDefault(g => g.SecretKey == secretKey);
            if (game == null)
            {
                _logger.LogWarning("Invalid secret key: {SecretKey}", secretKey);
                return Unauthorized("Invalid secret key.");
            }

            // Check if logs are provided
            if (logs == null || logs.Length == 0)
            {
                _logger.LogWarning("Empty log array received");
                return BadRequest("No logs were provided.");
            }

            // Convert the DTO logs into the actual UserLog model, adding the GameId
            var logsToSave = logs.Select(logDto => new UserLog
            {
                UUID = logDto.UUID,
                LogLevel = logDto.LogLevel,
                Message = logDto.Message,
                Timestamp = logDto.Timestamp,
                AdditionalData = logDto.AdditionalData,
                GameId = game.Id  // Associate with the correct GameId
            }).ToList();

            // Print received logs for debugging
            foreach (var log in logsToSave)
            {
                _logger.LogInformation("Received log for Game ID {GameId}: {Log}", game.Id, log);
            }

            try
            {
                // Process the logs and associate them with the game
                await _loggingService.LogBulkAsync(logsToSave.ToArray());
                _logger.LogInformation("{Count} logs processed successfully for Game ID {GameId}", logsToSave.Count, game.Id);
                return Ok(new { message = "Logs received successfully", count = logsToSave.Count });
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error while processing logs for Game ID {GameId}", game.Id);
                return StatusCode(500, "An error occurred while processing logs.");
            }
        }

        // Admin endpoint to get logs with pagination and filtering
        [HttpGet]
        public async Task<IActionResult> Index(int pageNumber = 1, string? uuid = null, string? gameId = null)
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

            // Add an explicit order, for example by Timestamp, to ensure a consistent order when paginating
            query = query.OrderByDescending(log => log.Timestamp);  // You can adjust the ordering as needed

            // Pagination logic
            int totalLogs = await query.CountAsync();
            var logs = await query.Skip((pageNumber - 1) * pageSize)
                                  .Take(pageSize)
                                  .ToListAsync();

            // Fetch the game names for the logs
            var logsWithGameNames = logs
                .Select(log => new UserLogWithGameName
                {
                    Log = log,
                    GameName = _context.Games.FirstOrDefault(g => g.Id == log.GameId)?.Name ?? "Unknown Game"  // Fetch the game name using GameId
                }).ToList();

            var viewModel = new PagedUserLogViewModel
            {
                Logs = logsWithGameNames,
                CurrentPage = pageNumber,
                TotalPages = (int)Math.Ceiling((double)totalLogs / pageSize),
                UUIDFilter = uuid ?? string.Empty,
                GameIdFilter = gameId ?? string.Empty
            };

            return View(viewModel);  // Return view with filtered and paginated logs
        }


        [HttpPost("delete/{logId}")]
        [Authorize(Roles = "Admin")]  // Only admins can delete logs
        public IActionResult DeleteLog(int logId)
        {
            var log = _context.UserLogs.FirstOrDefault(l => l.Id == logId);
            if (log == null)
            {
                return NotFound("Log not found.");
            }

            _context.UserLogs.Remove(log);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Log deleted successfully!";
            return RedirectToAction("UserLogs","Admin");  // Redirect back to the logs page
        }

    }
}
