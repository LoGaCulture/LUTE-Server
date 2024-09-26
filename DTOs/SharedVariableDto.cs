using LUTE_Server.Models;

namespace LUTE_Server.DTOs
{
    public class SharedVariableDto
    {
        public required string VariableName { get; set; }     // e.g., "Stone1"
        public required string VariableType { get; set; }     // e.g., "Stone", "Score"
        public required string Data { get; set; }             // JSON string representing the variable data
    }
}