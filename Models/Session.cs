using System;

namespace LUTE_Server.Models
{
    public class Session
    {
        public int Id { get; set; }
        public string UUID { get; set; } = string.Empty;  // User's unique identifier
        public string GameId { get; set; }  // FK to Games table
        public DateTime StartTime { get; set; } = DateTime.UtcNow;
        public DateTime LastActivity { get; set; } = DateTime.UtcNow;  
        public bool IsActive { get; set; } = true;  
    }
}
