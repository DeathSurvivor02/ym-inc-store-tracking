using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YMInc.StoreDeploymentApp.Models;

namespace YMInc.StoreDeploymentApp.Services
{
    /// <summary>
    /// Standalone Dummy Database Service for the isolated Dummy Project C# application.
    /// Does NOT reference PostgreSQL, Npgsql, Excel rosters, or the main production project.
    /// </summary>
    public class DatabaseService
    {
        public bool IsConnected { get; private set; } = true;
        public string StatusMessage { get; private set; } = "Connected: Standalone Dummy Project DB (dummy_project_db.json — Isolated from Main Project)";
        public string? LastError { get; private set; }

        public Task<bool> TestConnectionAsync()
        {
            DummyProjectService.LoadOrInitialize();
            IsConnected = true;
            StatusMessage = "Connected: Standalone Dummy Project DB (dummy_project_db.json — Isolated from Main Project)";
            return Task.FromResult(true);
        }

        public Task<List<StoreDeployment>> LoadStoresAsync()
        {
            DummyProjectService.LoadOrInitialize();
            return Task.FromResult(DummyProjectService.GetStores());
        }

        public Task<List<MallProperty>> LoadMallsAsync()
        {
            DummyProjectService.LoadOrInitialize();
            return Task.FromResult(DummyProjectService.GetMalls());
        }

        public Task<bool> SaveMallAsync(MallProperty mall)
        {
            LastError = null;
            return Task.FromResult(DummyProjectService.SaveMall(mall));
        }

        public Task<bool> SaveStoreAsync(StoreDeployment store)
        {
            LastError = null;
            return Task.FromResult(DummyProjectService.SaveStore(store));
        }

        public Task<List<CalendarWorkItem>> LoadCalendarWorkItemsAsync(List<StoreDeployment>? fleetStores = null)
        {
            DummyProjectService.LoadOrInitialize();
            return Task.FromResult(DummyProjectService.GetWorkItems());
        }

        public Task SaveCalendarWorkItemAsync(CalendarWorkItem item)
        {
            item.FiscalWeek = DummyProjectService.GetFiscalWeekForDate(item.ActivityDate);
            item.FiscalYear = DummyProjectService.GetFiscalYearForDate(item.ActivityDate);
            DummyProjectService.SaveWorkItem(item);
            return Task.CompletedTask;
        }

        public Task DeleteCalendarWorkItemAsync(string workItemId)
        {
            DummyProjectService.DeleteWorkItem(workItemId);
            return Task.CompletedTask;
        }

        public async Task<List<FiscalCalendarDay>> LoadFiscalCalendarAsync(int? fiscalYear = null)
        {
            var list = GenerateNrfCalendarDays(fiscalYear);
            var workItems = await LoadCalendarWorkItemsAsync();
            var byDate = workItems
                .GroupBy(w => w.ActivityDate.Date)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var day in list)
            {
                if (byDate.TryGetValue(day.CalendarDate.Date, out var itemsForDay))
                {
                    day.WorkItems = itemsForDay;
                }
            }

            return list;
        }

        private static List<FiscalCalendarDay> GenerateNrfCalendarDays(int? filterYear)
        {
            var list = new List<FiscalCalendarDay>();
            for (int fy = 2025; fy <= 2027; fy++)
            {
                if (filterYear.HasValue && filterYear.Value != fy) continue;

                DateTime fyStart = fy switch
                {
                    2025 => new DateTime(2025, 1, 5),
                    2026 => new DateTime(2026, 1, 4),
                    _ => new DateTime(2027, 1, 3)
                };

                for (int wk = 1; wk <= 52; wk++)
                {
                    int quarter = ((wk - 1) / 13) + 1;
                    int monthInQuarter = ((wk - 1) % 13) < 4 ? 1 : (((wk - 1) % 13) < 9 ? 2 : 3);
                    int fiscalMonth = (quarter - 1) * 3 + monthInQuarter;

                    for (int dow = 1; dow <= 7; dow++)
                    {
                        DateTime d = fyStart.AddDays((wk - 1) * 7 + (dow - 1));
                        var (isHol, holName) = CheckRetailHoliday(d);
                        list.Add(new FiscalCalendarDay
                        {
                            CalendarDate = d,
                            FiscalYear = fy,
                            FiscalQuarter = quarter,
                            FiscalMonth = fiscalMonth,
                            FiscalWeek = wk,
                            DayOfFiscalWeek = dow,
                            IsHoliday = isHol,
                            HolidayName = holName
                        });
                    }
                }
            }
            return list;
        }

        private static (bool IsHoliday, string? Name) CheckRetailHoliday(DateTime d)
        {
            if (d.Month == 1 && d.Day == 1) return (true, "New Year's Day");
            if (d.Month == 7 && d.Day == 1) return (true, "Canada Day (CA Fleet)");
            if (d.Month == 7 && d.Day == 4) return (true, "Independence Day (US Fleet)");
            if (d.Month == 9 && d.DayOfWeek == DayOfWeek.Monday && d.Day <= 7) return (true, "Labor Day / Back-to-School Peak");
            if (d.Month == 10 && d.DayOfWeek == DayOfWeek.Monday && d.Day >= 8 && d.Day <= 14) return (true, "Canadian Thanksgiving / Indigenous Peoples Day");
            if (d.Month == 10 && d.Day == 31) return (true, "Halloween Retail Event");
            if (d.Month == 11 && d.DayOfWeek == DayOfWeek.Thursday && d.Day >= 22 && d.Day <= 28) return (true, "US Thanksgiving — Deployment Freeze");
            if (d.Month == 11 && d.DayOfWeek == DayOfWeek.Friday && d.Day >= 23 && d.Day <= 29) return (true, "Black Friday Peak Freeze (All Tech Work Halted)");
            if (d.Month == 12 && d.Day == 25) return (true, "Christmas Day — Holiday Freeze");
            if (d.Month == 12 && d.Day == 26) return (true, "Boxing Day Peak (CA Fleet)");
            return (false, null);
        }

        public static WeeklyProgressReport GetWeeklyReportFromFleet(
            int fiscalWeek,
            List<StoreDeployment> stores,
            List<CalendarWorkItem>? allWorkItems = null)
        {
            int total = Math.Max(1, stores.Count);
            allWorkItems ??= DummyProjectService.GetWorkItems();

            var (weekStart, weekEnd) = DummyProjectService.GetDateRangeForFiscalWeek(fiscalWeek);
            string dateRange = $"{weekStart:MMM dd} – {weekEnd:MMM dd, yyyy}";

            var weekTasks = allWorkItems
                .Where(w => w.FiscalWeek == fiscalWeek || (w.ActivityDate.Date >= weekStart.Date && w.ActivityDate.Date <= weekEnd.Date))
                .OrderBy(w => w.ActivityDate)
                .ThenBy(w => w.TimeWindow)
                .ToList();

            int weekTotal = weekTasks.Count;
            int weekCompleted = weekTasks.Count(w => w.Status.Contains("Completed", StringComparison.OrdinalIgnoreCase));
            int weekInProg = weekTasks.Count(w => w.Status.Contains("Progress", StringComparison.OrdinalIgnoreCase));
            int weekBlocked = weekTasks.Count(w => w.Status.Contains("Blocked", StringComparison.OrdinalIgnoreCase) || w.Status.Contains("Delay", StringComparison.OrdinalIgnoreCase));
            int weekScheduled = Math.Max(0, weekTotal - weekCompleted - weekInProg - weekBlocked);

            int ttuCount = weekTasks.Count(w => w.TypeShortLabel == "TTU");
            int circuitCount = weekTasks.Count(w => w.TypeShortLabel == "CIRCUIT");
            int cablingCount = weekTasks.Count(w => w.TypeShortLabel == "CABLING");
            int poleCount = weekTasks.Count(w => w.TypeShortLabel == "POLES");
            int surveyCount = weekTasks.Count(w => w.TypeShortLabel == "SURVEY");
            int otherCount = Math.Max(0, weekTotal - ttuCount - circuitCount - cablingCount - poleCount - surveyCount);

            int baseCompleted = stores.Count(s => s.WeeklyStatus == "Completed & Certified" || s.OperationalStatus == "Open" || s.OperationalStatus == "Grand Open");
            int baseBlocked = stores.Count(s => s.WeeklyStatus == "Blocked / In Jeopardy" || s.OverallHealth == "At Risk" || s.OverallHealth == "Critical Blocker");
            int baseScheduled = stores.Count(s => s.WeeklyStatus == "Scheduled / Pending" || s.OperationalStatus == "Site Survey");
            int baseInProgress = Math.Max(0, total - baseCompleted - baseBlocked - baseScheduled);

            int baseSurveys = stores.Count(s => s.Survey != null && s.Survey.Status.Contains("Approved"));
            int basePoles = stores.Count(s => s.PowerPoles != null && s.PowerPoles.InstalledCount >= s.PowerPoles.TotalRequired);
            int baseCabling = stores.Count(s => s.Cabling != null && s.Cabling.FlukeCertified);
            int baseCircuits = stores.Count(s => s.Circuits.Count >= 2 && s.Circuits.All(c => c.OrderStatus.Contains("Active") || c.OrderStatus.Contains("Turned Up")));
            int basePos = stores.Count(s => s.PosStagingStatus == "Commissioned" || s.OperationalStatus == "Open" || s.OperationalStatus == "Grand Open");

            double weekFactor = GetHistoricalWeekProgressionFactor(fiscalWeek);

            int completed = Math.Clamp((int)Math.Round(baseCompleted * weekFactor) + (fiscalWeek > 42 ? weekCompleted : 0), 0, total);
            int blocked = fiscalWeek < 39 ? Math.Max(0, baseBlocked - 1) : baseBlocked;
            int remaining = Math.Max(0, total - completed - blocked);
            int inProgress = (int)Math.Round(remaining * (fiscalWeek <= 38 ? 0.45 : 0.72));
            int scheduled = Math.Max(0, remaining - inProgress);

            int siteSurveysCompleted = Math.Clamp((int)Math.Round(baseSurveys * Math.Min(1.0, weekFactor + 0.18)), 0, total);
            int powerPolesInstalled = Math.Clamp((int)Math.Round(basePoles * Math.Min(1.0, weekFactor + 0.06)), 0, total);
            int cablingCertified = Math.Clamp((int)Math.Round(baseCabling * weekFactor), 0, total);
            int circuitsActive = Math.Clamp((int)Math.Round(baseCircuits * Math.Max(0.2, weekFactor - 0.04)), 0, total);
            int posCommissioned = Math.Clamp((int)Math.Round(basePos * Math.Max(0.15, weekFactor - 0.08)), 0, total);

            double completedPct = total > 0 ? (double)completed / total * 100.0 : 0;
            double inProgressPct = total > 0 ? (double)inProgress / total * 100.0 : 0;
            double scheduledPct = total > 0 ? (double)scheduled / total * 100.0 : 0;
            double blockedPct = total > 0 ? (double)blocked / total * 100.0 : 0;

            int overallPct = (int)Math.Round(
                (completedPct * 0.55) +
                ((total > 0 ? (double)(siteSurveysCompleted + powerPolesInstalled + cablingCertified + circuitsActive) / (total * 4.0) * 100.0 : 0) * 0.45));

            if (weekTotal > 0)
            {
                double taskRatio = (double)weekCompleted / weekTotal * 100.0;
                overallPct = (int)Math.Round(overallPct * 0.75 + taskRatio * 0.25);
            }
            overallPct = Math.Clamp(overallPct, 8, 100);

            int prevWeekFactorPct = (int)Math.Round(overallPct * (fiscalWeek <= 36 ? 0.85 : GetHistoricalWeekProgressionFactor(fiscalWeek - 1) / Math.Max(0.1, weekFactor)));
            int deltaVsPrior = Math.Max(0, overallPct - prevWeekFactorPct);

            var trend = new List<WeekProgressionPoint>();
            for (int w = 36; w <= 43; w++)
            {
                var (ws, we) = DummyProjectService.GetDateRangeForFiscalWeek(w);
                var wTasks = allWorkItems.Where(item => item.FiscalWeek == w || (item.ActivityDate.Date >= ws.Date && item.ActivityDate.Date <= we.Date)).ToList();
                int wDone = wTasks.Count(item => item.Status.Contains("Completed", StringComparison.OrdinalIgnoreCase));
                double wf = GetHistoricalWeekProgressionFactor(w);
                int wPct = w == fiscalWeek
                    ? overallPct
                    : Math.Clamp((int)Math.Round((baseCompleted > 0 ? (double)baseCompleted / total * 100.0 : 68.0) * wf), 12, 100);

                trend.Add(new WeekProgressionPoint
                {
                    FiscalWeek = w,
                    DateRangeShort = $"{ws:MMM d}–{we:MMM d}",
                    CumulativeProgressPct = wPct,
                    WeekSpecificExecutionPct = wTasks.Count > 0 ? (int)Math.Round((double)wDone / wTasks.Count * 100.0) : wPct,
                    TasksCompletedInWeek = wDone,
                    TasksTotalInWeek = wTasks.Count,
                    IsSelected = (w == fiscalWeek)
                });
            }

            string narrative = weekTotal > 0
                ? $"Week {fiscalWeek} ({dateRange}): {weekCompleted}/{weekTotal} dummy project field operations completed ({ttuCount} TTUs, {circuitCount} Circuit Installs, {cablingCount} Cabling, {poleCount} Power Poles, {surveyCount} Surveys)."
                : $"Week {fiscalWeek} ({dateRange}): Dummy rollout at {overallPct}% across {total} synthetic stores. Add daily TTU or Circuit tasks in the Calendar to track day-by-day execution.";

            return new WeeklyProgressReport
            {
                FiscalYear = 2026,
                FiscalWeek = fiscalWeek,
                WeekLabel = $"Fiscal 2026 — Week {fiscalWeek}",
                DateRange = dateRange,
                TotalActiveProjects = total,
                CompletedThisWeek = completed,
                InProgress = inProgress,
                ScheduledPending = scheduled,
                BlockedInJeopardy = blocked,
                OverallProgressPct = overallPct,
                WeekTasksTotal = weekTotal,
                WeekTasksCompleted = weekCompleted,
                WeekTasksInProgress = weekInProg,
                WeekTasksScheduled = weekScheduled,
                WeekTasksBlocked = weekBlocked,
                TtuCount = ttuCount,
                CircuitInstallCount = circuitCount,
                CablingCount = cablingCount,
                PowerPoleCount = poleCount,
                SurveyCount = surveyCount,
                OtherTaskCount = otherCount,
                DeltaVsPriorWeekPct = deltaVsPrior,
                WeekNarrative = narrative,
                WeeklyHistoryTrend = trend,
                WeekActivities = weekTasks,
                Milestones = new List<MilestoneProgress>
                {
                    new() { Name = "Site Surveys Approved", Completed = siteSurveysCompleted, Total = total },
                    new() { Name = "Wiremold 25DTP-4 Power Poles Installed", Completed = powerPolesInstalled, Total = total },
                    new() { Name = "Cat6 Plenum Cabling Certified (Fluke)", Completed = cablingCertified, Total = total },
                    new() { Name = "Dual-WAN Telecom Turn-up", Completed = circuitsActive, Total = total },
                    new() { Name = "POS Commissioning & QA", Completed = posCommissioned, Total = total }
                },
                Slices = new List<DonutSlice>
                {
                    new() { Name = "Completed & Certified", Count = completed, Percentage = completedPct, ColorHex = "#107C10" },
                    new() { Name = "In Progress / On Schedule", Count = inProgress, Percentage = inProgressPct, ColorHex = "#0F6CBD" },
                    new() { Name = "Scheduled / Pending", Count = scheduled, Percentage = scheduledPct, ColorHex = "#D97706" },
                    new() { Name = "Blocked / In Jeopardy", Count = blocked, Percentage = blockedPct, ColorHex = "#B91C1C" }
                }
            };
        }

        private static double GetHistoricalWeekProgressionFactor(int fiscalWeek)
        {
            return fiscalWeek switch
            {
                <= 35 => 0.28,
                36 => 0.36,
                37 => 0.46,
                38 => 0.58,
                39 => 0.70,
                40 => 0.81,
                41 => 0.91,
                42 => 1.00,
                43 => 1.04,
                _ => 1.08
            };
        }
    }
}
