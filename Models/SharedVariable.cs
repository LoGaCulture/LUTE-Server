using System;

namespace LUTE_Server.Models
{
    public class SharedVariable
    {
        public int Id { get; set; }                         // Primary key.
        public string GameId { get; set; }                       // Foreign key to the game that this variable belongs to - it's a GUID.
        public required string UUID { get; set; }              // User's unique identifier.
        public required string VariableName { get; set; }     // Name of the variable, e.g., "Stone1", "Score".
        public required string VariableType { get; set; }     // Type of the variable, e.g., "Stone", "Score".
        public required string Data { get; set; }             // JSON data representing the actual variable state.
        public DateTime CreatedAt { get; set; }      // Timestamp of when the variable was created or last updated.
    }
}
