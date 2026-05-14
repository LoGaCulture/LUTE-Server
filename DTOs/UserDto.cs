using LUTE_Server.Models;

namespace LUTE_Server.DTOs
{
    /// <summary>
    /// Safe public projection of a User — intentionally omits PasswordHash.
    /// </summary>
    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public UserRole Role { get; set; }

        public static UserDto FromUser(User user) => new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Role = user.Role
        };
    }
}
