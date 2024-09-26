using System.Collections.Generic;
using LUTE_Server.Models;

namespace LUTE_Server.ViewModels
{
    public class PagedUserViewModel
    {
        public required List<User> Users { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}
