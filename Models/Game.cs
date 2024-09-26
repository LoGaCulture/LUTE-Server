using System;

namespace LUTE_Server.Models
{
    public class Game
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string SecretKey { get; set; } = string.Empty;  

        public int CreatedBy { get; set; }  
    }
}