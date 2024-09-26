using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using LUTE_Server.Models;
using Microsoft.EntityFrameworkCore;
using LUTE_Server.Data;

namespace LUTE_Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SessionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SessionController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Start a session or update the last activity if the session is active
        [HttpPost("start")]
        public async Task<ActionResult> StartSession([FromBody] SessionStartRequest request)
        {
            // Look for an existing active session for the user and game
            var session = await _context.Sessions
                .Where(s => s.UUID == request.UUID && s.GameId == request.GameId && s.IsActive)
                .FirstOrDefaultAsync();

            if (session == null)
            {
                // No active session found, create a new one
                session = new Session
                {
                    UUID = request.UUID,
                    GameId = request.GameId,
                    StartTime = DateTime.UtcNow,
                    LastActivity = DateTime.UtcNow,
                    IsActive = true
                };

                await _context.Sessions.AddAsync(session);
            }
            else
            {
                // Update last activity for the existing active session
                session.LastActivity = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            // Retrieve shared variables or other session data here
            var sharedVariables = await _context.GameSharedVariables
                .Where(v => v.GameId == request.GameId)
                .ToListAsync();

            return Ok(new { SharedVariables = sharedVariables });
        }

        // Mark the session as ended (inactive)
        [HttpPost("end")]
        public async Task<ActionResult> EndSession([FromBody] SessionEndRequest request)
        {
            var session = await _context.Sessions
                .Where(s => s.UUID == request.UUID && s.GameId == request.GameId && s.IsActive)
                .FirstOrDefaultAsync();

            if (session == null)
            {
                return NotFound("Session not found.");
            }

            session.IsActive = false;
            session.LastActivity = DateTime.UtcNow;  // Mark as ended

            await _context.SaveChangesAsync();

            return Ok(new { message = "Session ended successfully." });
        }

        // Mark sessions as inactive after a timeout period (12 hours)
        [HttpPost("timeout-check")]
        public async Task<ActionResult> TimeoutInactiveSessions()
        {
            var timeoutThreshold = DateTime.UtcNow.AddHours(-12);  // 12-hour inactivity window

            var inactiveSessions = await _context.Sessions
                .Where(s => s.IsActive && s.LastActivity < timeoutThreshold)
                .ToListAsync();

            foreach (var session in inactiveSessions)
            {
                session.IsActive = false;  // Mark session as inactive
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = $"{inactiveSessions.Count} sessions marked as inactive due to inactivity." });
        }
    }

    public class SessionStartRequest
    {
        public required string UUID { get; set; }
        public int GameId { get; set; }
    }

    public class SessionEndRequest
    {
        public required string UUID { get; set; }
        public int GameId { get; set; }
    }
}
