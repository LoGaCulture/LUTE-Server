namespace LUTE_Server.DTOs
{
    public class AuthResult
    {
        public string Token { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
