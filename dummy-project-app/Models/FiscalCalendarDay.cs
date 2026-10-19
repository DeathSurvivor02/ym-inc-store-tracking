using System;
using System.Collections.Generic;
using System.Linq;

namespace YMInc.StoreDeploymentApp.Models
{
    public class CalendarWorkItem
    {
        public string WorkItemId { get; set; } = Guid.NewGuid().ToString("N").Substring(0, 8);
        public DateTime ActivityDate { get; set; } = DateTime.Today;
        public int FiscalYear { get; set; } = 2026;
        public int FiscalWeek { get; set; } = 42;
        public string ActivityType { get; set; } = "TTU (Test & Turn-Up)";
        public string Title { get; set; } = string.Empty;
        public string StoreNumber { get; set; } = string.Empty;
        public string StoreName { get; set; } = string.Empty;
        public string BannerCode { get; set; } = string.Empty;
        public string Status { get; set; } = "Scheduled";
        public string AssignedTo { get; set; } = string.Empty;
        public string TimeWindow { get; set; } = "08:00 - 12:00";
        public string Notes { get; set; } = string.Empty;
        public bool IsSystemDerived { get; set; } = false;

        public string Id
        {
            get => WorkItemId;
            set => WorkItemId = value;
        }

        public string DateIso
        {
            get => ActivityDate.ToString("yyyy-MM-dd");
            set
            {
                if (DateTime.TryParse(value, out var dt))
                    ActivityDate = dt;
            }
        }

        public string TechnicianOrVendor
        {
            get => AssignedTo;
            set => AssignedTo = value;
        }

        public bool IsCompleted => Status.Contains("Completed", StringComparison.OrdinalIgnoreCase);

        public string DateDisplay
        {
            get => DateIso;
            set => DateIso = value;
        }
        public string ShortDateDisplay => ActivityDate.ToString("MMM dd");
        public string WeekBadge => $"WK {FiscalWeek}";
        public string StoreDisplay => string.IsNullOrWhiteSpace(StoreNumber) || StoreNumber == "ALL" || StoreNumber == "FLEET"
            ? "Fleet / General"
            : $"#{StoreNumber} {StoreName}".Trim();

        public string TypeShortLabel => ActivityType switch
        {
            var t when t.Contains("TTU", StringComparison.OrdinalIgnoreCase) => "TTU",
            var t when t.Contains("Circuit", StringComparison.OrdinalIgnoreCase) => "CIRCUIT",
            var t when t.Contains("Cabling", StringComparison.OrdinalIgnoreCase) => "CABLING",
            var t when t.Contains("Pole", StringComparison.OrdinalIgnoreCase) => "POLES",
            var t when t.Contains("Survey", StringComparison.OrdinalIgnoreCase) => "SURVEY",
            var t when t.Contains("POS", StringComparison.OrdinalIgnoreCase) => "POS QA",
            var t when t.Contains("Landlord", StringComparison.OrdinalIgnoreCase) || t.Contains("Permit", StringComparison.OrdinalIgnoreCase) => "PERMIT",
            _ => "TASK"
        };

        public string ShortTag => TypeShortLabel;

        // Clean Enterprise Light / Dark Theme Badge Colors
        public string TypeBadgeBg => Services.ThemeService.IsDarkTheme
            ? TypeShortLabel switch
            {
                "TTU" => "#133A25",
                "CIRCUIT" => "#172B4D",
                "CABLING" => "#2E1A47",
                "POLES" => "#3E2709",
                "SURVEY" => "#163042",
                "POS QA" => "#133836",
                _ => "#2D2D30"
            }
            : TypeShortLabel switch
            {
                "TTU" => "#DCFCE7",
                "CIRCUIT" => "#DBEAFE",
                "CABLING" => "#EDE9FE",
                "POLES" => "#FEF3C7",
                "SURVEY" => "#E0F2FE",
                "POS QA" => "#CCFBF1",
                _ => "#F3F4F6"
            };

        public string TypeBadgeFg => Services.ThemeService.IsDarkTheme
            ? TypeShortLabel switch
            {
                "TTU" => "#4ADE80",
                "CIRCUIT" => "#60A5FA",
                "CABLING" => "#C4B5FD",
                "POLES" => "#FBBF24",
                "SURVEY" => "#38BDF8",
                "POS QA" => "#2DD4BF",
                _ => "#D1D5DB"
            }
            : TypeShortLabel switch
            {
                "TTU" => "#166534",
                "CIRCUIT" => "#1E40AF",
                "CABLING" => "#5B21B6",
                "POLES" => "#92400E",
                "SURVEY" => "#075985",
                "POS QA" => "#115E59",
                _ => "#374151"
            };

        public string BadgeBgColor => TypeBadgeBg;
        public string BadgeTextColor => TypeBadgeFg;

        public string StatusBadgeBg => Services.ThemeService.IsDarkTheme
            ? Status switch
            {
                var s when s.Contains("Completed", StringComparison.OrdinalIgnoreCase) => "#133A25",
                var s when s.Contains("Progress", StringComparison.OrdinalIgnoreCase) => "#172B4D",
                var s when s.Contains("Blocked", StringComparison.OrdinalIgnoreCase) || s.Contains("Delay", StringComparison.OrdinalIgnoreCase) => "#451A1A",
                _ => "#2D2D30"
            }
            : Status switch
            {
                var s when s.Contains("Completed", StringComparison.OrdinalIgnoreCase) => "#DCFCE7",
                var s when s.Contains("Progress", StringComparison.OrdinalIgnoreCase) => "#DBEAFE",
                var s when s.Contains("Blocked", StringComparison.OrdinalIgnoreCase) || s.Contains("Delay", StringComparison.OrdinalIgnoreCase) => "#FEE2E2",
                _ => "#F3F4F6"
            };

        public string StatusBadgeFg => Services.ThemeService.IsDarkTheme
            ? Status switch
            {
                var s when s.Contains("Completed", StringComparison.OrdinalIgnoreCase) => "#4ADE80",
                var s when s.Contains("Progress", StringComparison.OrdinalIgnoreCase) => "#60A5FA",
                var s when s.Contains("Blocked", StringComparison.OrdinalIgnoreCase) || s.Contains("Delay", StringComparison.OrdinalIgnoreCase) => "#F87171",
                _ => "#D1D5DB"
            }
            : Status switch
            {
                var s when s.Contains("Completed", StringComparison.OrdinalIgnoreCase) => "#15803D",
                var s when s.Contains("Progress", StringComparison.OrdinalIgnoreCase) => "#1D4ED8",
                var s when s.Contains("Blocked", StringComparison.OrdinalIgnoreCase) || s.Contains("Delay", StringComparison.OrdinalIgnoreCase) => "#B91C1C",
                _ => "#4B5563"
            };

        public string StatusColor => StatusBadgeFg;
    }

    public class FiscalCalendarDay
    {
        public DateTime CalendarDate { get; set; }
        public int FiscalYear { get; set; }
        public int FiscalQuarter { get; set; }
        public int FiscalMonth { get; set; }
        public int FiscalWeek { get; set; }
        public int DayOfFiscalWeek { get; set; }
        public bool IsHoliday { get; set; }
        public string? HolidayName { get; set; }
        public bool IsCurrentMonth { get; set; } = true;
        public bool IsSelected { get; set; } = false;

        public List<CalendarWorkItem> WorkItems { get; set; } = new();

        public string DateDisplay => CalendarDate.ToString("yyyy-MM-dd");
        public string DayNumberDisplay => CalendarDate.Day.ToString();
        public string MonthDayDisplay => CalendarDate.ToString("MMM d");
        public string ShortDateLabel => CalendarDate.ToString("MMM dd");
        public string DayOfWeekName => CalendarDate.ToString("dddd");
        public string ShortDayName => CalendarDate.ToString("ddd");
        public string DayAbbrev => CalendarDate.ToString("ddd").ToUpperInvariant();
        public string FiscalMonthName => CalendarDate.ToString("MMMM");
        public string QuarterDisplay => $"Q{FiscalQuarter}";
        public string PeriodDisplay => $"Period {FiscalMonth} ({FiscalMonthName})";
        public string WeekDisplay => $"WK {FiscalWeek}";
        public string HolidayDisplay => IsHoliday ? (string.IsNullOrEmpty(HolidayName) ? "Retail Holiday" : HolidayName) : "—";
        public string HolidayBadge => IsHoliday ? HolidayDisplay : "";
        public string HolidayStatusColor => IsHoliday
            ? (Services.ThemeService.IsDarkTheme ? "#FBBF24" : "#B45309")
            : (Services.ThemeService.IsDarkTheme ? "#9CA3AF" : "#6B7280");

        public int TotalActivitiesCount => WorkItems.Count;
        public bool HasActivities => WorkItems.Count > 0;
        public bool HasWorkItems => WorkItems.Count > 0;
        public int CompletedActivitiesCount => WorkItems.Count(w => w.IsCompleted);

        public string ActivityCountBadge => WorkItems.Count > 0 ? $"{WorkItems.Count} {(WorkItems.Count == 1 ? "Item" : "Items")}" : "0 Items";
        public string WorkItemCountBadge => ActivityCountBadge;

        public string DayCardBackground => Services.ThemeService.IsDarkTheme
            ? (HasWorkItems ? "#252526" : "#1E1E1E")
            : (HasWorkItems ? "#FFFFFF" : "#F9FAFB");
        public string DayCardBorder => Services.ThemeService.IsDarkTheme
            ? (HasWorkItems ? "#3B82F6" : (IsHoliday ? "#D97706" : "#3E3E42"))
            : (HasWorkItems ? "#0F6CBD" : (IsHoliday ? "#D97706" : "#D1D5DB"));

        public string DailyActivitiesSummary
        {
            get
            {
                if (WorkItems.Count == 0)
                    return IsHoliday ? HolidayDisplay : "—";

                var preview = WorkItems
                    .Take(3)
                    .Select(w => $"[{w.TypeShortLabel}] {(string.IsNullOrEmpty(w.StoreNumber) || w.StoreNumber == "ALL" ? w.Title : $"{w.StoreNumber}: {(string.IsNullOrEmpty(w.Title) ? w.ActivityType : w.Title)}")}");
                string joined = string.Join(" | ", preview);
                if (WorkItems.Count > 3)
                    joined += $" (+{WorkItems.Count - 3})";
                return joined;
            }
        }

        public List<CalendarWorkItem> TopWorkItems => WorkItems.Take(3).ToList();
        public bool HasMoreItems => WorkItems.Count > 3;
        public string MoreItemsText => WorkItems.Count > 3 ? $"+{WorkItems.Count - 3} more" : "";
    }
}
