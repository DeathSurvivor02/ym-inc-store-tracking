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
        public string ColorHex { get; set; } = "#15803D";
    }

    public class WeekProgressionPoint
    {
        public int FiscalWeek { get; set; }
        public string WeekShortLabel => $"WK {FiscalWeek}";
        public string WeekLabel => WeekShortLabel;
        public string DateRangeShort { get; set; } = string.Empty;
        public int CumulativeProgressPct { get; set; }
        public int ProgressPct => CumulativeProgressPct;
        public int WeekSpecificExecutionPct { get; set; }
        public int TasksCompletedInWeek { get; set; }
        public int TasksTotalInWeek { get; set; }
        public string TaskRatioDisplay => $"{TasksCompletedInWeek}/{TasksTotalInWeek}";
        public bool IsSelected { get; set; }
        public double BarHeightPx => System.Math.Max(12, CumulativeProgressPct * 0.85);
        public string BarBackground => IsSelected
            ? "#0F6CBD"
            : (Services.ThemeService.IsDarkTheme ? "#3E3E42" : "#CBD5E1");
        public string BarBorder => IsSelected
            ? "#0C4A82"
            : (Services.ThemeService.IsDarkTheme ? "#52525B" : "#94A3B8");
        public string BarFillBrush => BarBackground;
        public string BarBorderBrush => BarBorder;
        public string LabelForeground => IsSelected
            ? (Services.ThemeService.IsDarkTheme ? "#60A5FA" : "#0F6CBD")
            : (Services.ThemeService.IsDarkTheme ? "#D1D5DB" : "#4B5563");
        public string LabelColor => LabelForeground;
    }

    public class WeeklyProgressReport
    {
        public int FiscalYear { get; set; } = 2026;
        public int FiscalWeek { get; set; } = 42;
        public string WeekLabel { get; set; } = "Fiscal 2026 — Week 42";
        public string DateRange { get; set; } = "Oct 18 – Oct 24, 2026";
        public string WeekDateRange => DateRange;
        public int TotalActiveProjects { get; set; } = 24;
        public int TotalActiveFleet => TotalActiveProjects;
        public int CompletedThisWeek { get; set; } = 17;
        public int InProgress { get; set; } = 5;
        public int ScheduledPending { get; set; } = 1;
        public int BlockedInJeopardy { get; set; } = 1;
        public int OverallProgressPct { get; set; } = 84;

        public int WeekTasksTotal { get; set; }
        public int WeekTasksCompleted { get; set; }
        public int WeekTasksInProgress { get; set; }
        public int WeekTasksScheduled { get; set; }
        public int WeekTasksBlocked { get; set; }
        public int WeekExecutionPct => WeekTasksTotal > 0 ? (int)System.Math.Round((double)WeekTasksCompleted / WeekTasksTotal * 100.0) : OverallProgressPct;

        public int TtuCount { get; set; }
        public int CircuitInstallCount { get; set; }
        public int CablingCount { get; set; }
        public int PowerPoleCount { get; set; }
        public int SurveyCount { get; set; }
        public int OtherTaskCount { get; set; }

        public int DeltaVsPriorWeekPct { get; set; }
        public string DeltaDisplay => DeltaVsPriorWeekPct >= 0 ? $"+{DeltaVsPriorWeekPct}% vs Wk {FiscalWeek - 1}" : $"{DeltaVsPriorWeekPct}% vs Wk {FiscalWeek - 1}";
        public string WeekNarrative { get; set; } = string.Empty;

        public List<MilestoneProgress> Milestones { get; set; } = new();
        public List<DonutSlice> Slices { get; set; } = new();
        public List<WeekProgressionPoint> WeeklyHistoryTrend { get; set; } = new();
        public List<CalendarWorkItem> WeekActivities { get; set; } = new();
    }
}
