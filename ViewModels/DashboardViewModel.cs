using LUTE_Server.Models;
using System.Collections.Generic;

namespace LUTE_Server.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalGames { get; set; }
        public int TotalLogs { get; set; }
        public int LogsLast7Days { get; set; }
        public IEnumerable<Game> Games { get; set; } = new List<Game>();
    }
}
