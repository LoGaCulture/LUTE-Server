using System.Collections.Generic;
using LUTE_Server.Models;

namespace LUTE_Server.ViewModels
{
    public class PagedUserLogViewModel
    {
        public List<UserLogWithGameName>? Logs { get; set; } 
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string? UUIDFilter { get; set; } 
        public string? GameIdFilter { get; set; }
    }

    public class UserLogWithGameName
    {
        public required UserLog Log { get; set; }
        public required string GameName { get; set; } 
    }
}
