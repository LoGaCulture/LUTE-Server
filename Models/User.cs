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

        public void SetPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                PasswordHash = System.BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        public bool CheckPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                var hashedPassword = System.BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
                return PasswordHash == hashedPassword;
            }
        }
    }
}
