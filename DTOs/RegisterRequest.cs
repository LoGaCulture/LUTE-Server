using LUTE_Server.Models;

namespace LUTE_Server.DTOs
{
    public class RegisterRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        // Role is intentionally omitted — all self-registered accounts get GameDeveloper.
        // Admin assigns roles via the admin panel.
    }
}
