using System;

namespace LUTE_Server.Models
{
    public class UserLog
    {
        public int Id { get; set; }
        public string UUID { get; set; } = string.Empty;  // This property is essential
        public required string GameId { get; set; }  // FK to Games table
        public string LogLevel { get; set; } = "Information";  // Log levels: Information, Warning, Error
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string AdditionalData { get; set; } = string.Empty;  // Optional: store extra info as JSON
    }
}
