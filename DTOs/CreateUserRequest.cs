using LUTE_Server.Models;

namespace LUTE_Server.DTOs
{
    /// <summary>
    /// Admin-only DTO for creating a user via POST /api/user.
    /// </summary>
    public class CreateUserRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.GameDeveloper;
    }
}
