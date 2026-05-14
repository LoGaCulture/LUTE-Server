using LUTE_Server.Models;

namespace LUTE_Server.DTOs
{
    /// <summary>
    /// Admin-only DTO for updating a user via PUT /api/user/{id}.
    /// Only Role is mutable — callers cannot overwrite PasswordHash or other fields.
    /// </summary>
    public class UpdateUserRequest
    {
        public UserRole Role { get; set; }
    }
}
