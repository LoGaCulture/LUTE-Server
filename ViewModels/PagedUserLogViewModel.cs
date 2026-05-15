using System;
using System.Collections.Generic;
using LUTE_Server.Models;

namespace LUTE_Server.ViewModels
{
    public class PagedUserLogViewModel
    {
        public List<UserLogWithGameName>? Logs { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }

        // --- existing filters ---
        public string? UUIDFilter { get; set; }
        public string? GameIdFilter { get; set; }

        // --- new filters (Round 4) ---
        public string[]? LogLevelFilters { get; set; }
        public string? SearchFilter { get; set; }
        public string? DatePreset { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        // --- sort ---
        public string SortBy { get; set; } = "timestamp";
        public string SortDir { get; set; } = "desc";

        // --- game dropdown data (with per-game log counts) ---
        public IEnumerable<GameLogCount> GameCounts { get; set; } = new List<GameLogCount>();
    }

    public class GameLogCount
    {
        public string GameId { get; set; } = "";
        public string Name { get; set; } = "";
        public int Count { get; set; }
    }

    public class UserLogWithGameName
    {
        public required UserLog Log { get; set; }
        public required string GameName { get; set; }
    }
}
