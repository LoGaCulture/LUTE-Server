namespace LUTE_Server.Models
{
    public enum UserRole
    {
        Admin,
        GameDeveloper,
        User
    }

    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.GameDeveloper;  // Default role is GameDeveloper
    }
}
