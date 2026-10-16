using System;

namespace YMInc.StoreDeploymentApp.Models
{
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

        public string DateDisplay => CalendarDate.ToString("yyyy-MM-dd");
        public string DayOfWeekName => CalendarDate.ToString("dddd");
        public string QuarterDisplay => $"Q{FiscalQuarter}";
        public string PeriodDisplay => $"Period {FiscalMonth}";
        public string WeekDisplay => $"Wk {FiscalWeek}";
        public string HolidayDisplay => IsHoliday ? (string.IsNullOrEmpty(HolidayName) ? "Retail Holiday" : HolidayName) : "—";
        public string HolidayBadge => IsHoliday ? $"⭐ {HolidayDisplay}" : "";
        public string HolidayStatusColor => IsHoliday ? "#F59E0B" : "#64748B";
    }
}
