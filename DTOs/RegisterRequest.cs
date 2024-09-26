using LUTE_Server.Models;

namespace LUTE_Server.DTOs
{
    public class RegisterRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = "GameDeveloper";
    }
}
