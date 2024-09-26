using System.Threading.Tasks;
using LUTE_Server.Models;
using LUTE_Server.Data;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace LUTE_Server.Services
{
    public class LoggingService : ILoggingService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LoggingService> _logger;

        public LoggingService(ApplicationDbContext context, ILogger<LoggingService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Log a single entry
        public async Task LogAsync(UserLog log)
        {
            _logger.LogInformation("Storing log entry for UUID: {UUID}, GameId: {GameId}", log.UUID, log.GameId);
            await _context.UserLogs.AddAsync(log);
            await _context.SaveChangesAsync();
        }

        // Log multiple entries in bulk
        public async Task LogBulkAsync(UserLog[] logs)
        {
            _logger.LogInformation("Storing {Count} log entries", logs.Length);

            // Validate each log before storing
            foreach (var log in logs)
            {
                if (string.IsNullOrWhiteSpace(log.Message) || string.IsNullOrWhiteSpace(log.LogLevel))
                {
                    _logger.LogWarning("Invalid log data. Skipping log with UUID: {UUID}", log.UUID);
                    continue;
                }

                _context.UserLogs.Add(log);
            }

            // Use a single call to save all logs
            await _context.SaveChangesAsync();
        }
    }
}
