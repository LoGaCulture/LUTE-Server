using LUTE_Server.Models;

namespace LUTE_Server.DTOs
{
    public class SharedVariableDto
    {
        public required string UUID { get; set; }             // User's unique identifier
        public required string VariableName { get; set; }     // e.g., "Stone1"
        public required string VariableType { get; set; }     // e.g., "Stone", "Score"
        public required string Data { get; set; }             // JSON string representing the variable data
        public DateTime CreatedAt { get; set; }      // Timestamp of when the variable was created or last updated
    }
}