using System;

namespace LUTE_Server.Models
{
    public class GameSharedVariable
    {
        public int Id { get; set; }                         // Primary key.
        public int GameId { get; set; }                       // Foreign key to the game that this variable belongs to.
        public required string UUID { get; set; }              // Unique user identifier, might be useful for auditing.
        public required string VariableName { get; set; }     // Name of the variable, e.g., "Stone1", "Score".
        public required string VariableType { get; set; }     // Type of the variable, e.g., "Stone", "Score".
        public required string Data { get; set; }             // JSON data representing the actual variable state.
        public DateTime CreatedAt { get; set; }      // Timestamp of when the variable was created or last updated.
    }
}
