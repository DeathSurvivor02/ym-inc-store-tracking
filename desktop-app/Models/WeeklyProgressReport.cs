using System.Collections.Generic;

namespace YMInc.StoreDeploymentApp.Models
{
    public class MilestoneProgress
    {
        public string Name { get; set; } = string.Empty;
        public int Completed { get; set; }
        public int Total { get; set; }
        public int Pct => Total > 0 ? (int)((double)Completed / Total * 100) : 0;
    }

    public class DonutSlice
    {
        public string Name { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Percentage { get; set; }
        public string ColorHex { get; set; } = "#10b981";
    }

    public class WeeklyProgressReport
    {
        public int FiscalYear { get; set; } = 2026;
        public int FiscalWeek { get; set; } = 41;
        public string WeekLabel { get; set; } = "Fiscal 2026 — Week 41";
        public string DateRange { get; set; } = "Oct 11 – Oct 17, 2026";
        public int TotalActiveProjects { get; set; } = 24;
        public int CompletedThisWeek { get; set; } = 13;
        public int InProgress { get; set; } = 7;
        public int ScheduledPending { get; set; } = 3;
        public int BlockedInJeopardy { get; set; } = 1;
        public int OverallProgressPct { get; set; } = 76;

        public List<MilestoneProgress> Milestones { get; set; } = new();
        public List<DonutSlice> Slices { get; set; } = new();
    }
}
