public class UserLogDto
{
    public string UUID { get; set; } = string.Empty;
    public string LogLevel { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string AdditionalData { get; set; } = string.Empty;
}