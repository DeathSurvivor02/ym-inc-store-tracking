using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using YMInc.StoreDeploymentApp.Models;

namespace YMInc.StoreDeploymentApp.Services
{
    public class DummyDatabaseState
    {
        public string ProjectName { get; set; } = "Project Horizon — Sandbox Rollout DB";
        public DateTime LastUpdatedUtc { get; set; } = DateTime.UtcNow;
        public List<MallProperty> Malls { get; set; } = new();
        public List<StoreDeployment> Stores { get; set; } = new();
        public List<CalendarWorkItem> WorkItems { get; set; } = new();
    }

    public static class DummyProjectService
    {
        private static readonly string DbFilePath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "dummy_project_db.json");

        private static DummyDatabaseState? _state;

        public static DummyDatabaseState State
        {
            get
            {
                if (_state == null)
                {
                    LoadOrInitialize();
                }
                return _state!;
            }
        }

        public static void LoadOrInitialize()
        {
            try
            {
                if (File.Exists(DbFilePath))
                {
                    string json = File.ReadAllText(DbFilePath);
                    var loaded = JsonSerializer.Deserialize<DummyDatabaseState>(json);
                    if (loaded != null && loaded.Stores.Count > 0)
                    {
                        _state = loaded;
                        RelinkStoreMalls(_state);
                        return;
                    }
                }
            }
            catch
            {
                // Re-create clean dummy state if corrupted
            }

            _state = CreateDefaultDummyDatabase();
            SaveState();
        }

        public static void ResetToDefaults()
        {
            _state = CreateDefaultDummyDatabase();
            SaveState();
        }

        public static void SaveState()
        {
            if (_state == null) return;
            try
            {
                _lastSelfSaveTimeUtc = DateTime.UtcNow;
                _state.LastUpdatedUtc = DateTime.UtcNow;
                var opts = new JsonSerializerOptions { WriteIndented = true };
                File.WriteAllText(DbFilePath, JsonSerializer.Serialize(_state, opts));
            }
            catch
            {
                // Ignore IO lock errors
            }
        }

        private static FileSystemWatcher? _fileWatcher;
        private static DateTime _lastSelfSaveTimeUtc = DateTime.MinValue;
        public static event Action? DatabaseChangedExternally;

        public static void StartFileWatcher()
        {
            if (_fileWatcher != null) return;
            try
            {
                string dir = Path.GetDirectoryName(DbFilePath) ?? AppDomain.CurrentDomain.BaseDirectory;
                string fileName = Path.GetFileName(DbFilePath);
                _fileWatcher = new FileSystemWatcher(dir, fileName)
                {
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName,
                    EnableRaisingEvents = true
                };
                _fileWatcher.Changed += OnFileChanged;
                _fileWatcher.Created += OnFileChanged;
            }
            catch
            {
                // Non-fatal if OS restricts FileSystemWatcher
            }
        }

        private static void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            // If the write originated from this instance within the last 1500ms, ignore
            if ((DateTime.UtcNow - _lastSelfSaveTimeUtc).TotalMilliseconds < 1500)
                return;

            // Brief pause to allow the saving process to release any file locks
            System.Threading.Thread.Sleep(200);

            try
            {
                if (File.Exists(DbFilePath))
                {
                    string json = File.ReadAllText(DbFilePath);
                    var loaded = JsonSerializer.Deserialize<DummyDatabaseState>(json);
                    if (loaded != null && loaded.Stores.Count > 0)
                    {
                        _state = loaded;
                        RelinkStoreMalls(_state);
                        DatabaseChangedExternally?.Invoke();
                    }
                }
            }
            catch
            {
                try
                {
                    System.Threading.Thread.Sleep(300);
                    if (File.Exists(DbFilePath))
                    {
                        string json = File.ReadAllText(DbFilePath);
                        var loaded = JsonSerializer.Deserialize<DummyDatabaseState>(json);
                        if (loaded != null && loaded.Stores.Count > 0)
                        {
                            _state = loaded;
                            RelinkStoreMalls(_state);
                            DatabaseChangedExternally?.Invoke();
                        }
                    }
                }
                catch { }
            }
        }

        private static void RelinkStoreMalls(DummyDatabaseState state)
        {
            var mallByCode = state.Malls.ToDictionary(m => m.MallCode, StringComparer.OrdinalIgnoreCase);
            foreach (var s in state.Stores)
            {
                if (s.Mall != null && !string.IsNullOrEmpty(s.Mall.MallCode) &&
                    mallByCode.TryGetValue(s.Mall.MallCode, out var canonicalMall))
                {
                    s.Mall = canonicalMall;
                }
            }
        }

        public static List<StoreDeployment> GetStores() => State.Stores.ToList();

        public static List<MallProperty> GetMalls() => State.Malls.ToList();

        public static List<CalendarWorkItem> GetWorkItems() =>
            State.WorkItems.OrderBy(w => w.ActivityDate).ThenBy(w => w.TimeWindow).ToList();

        public static bool CreateStore(StoreDeployment store)
        {
            var state = State;
            if (state == null) return false;

            if (state.Stores.Any(s => s.StoreNumber.Equals(store.StoreNumber, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            int nextId = state.Stores.Count > 0 ? state.Stores.Max(s => s.StoreId) + 1 : 901;
            store.StoreId = nextId;

            if (store.Mall != null)
            {
                var existingMall = state.Malls.FirstOrDefault(m =>
                    m.PropertyName.Equals(store.Mall.PropertyName, StringComparison.OrdinalIgnoreCase));
                if (existingMall != null)
                {
                    store.Mall = existingMall;
                }
                else
                {
                    store.Mall.MallId = state.Malls.Count > 0 ? state.Malls.Max(m => m.MallId) + 1 : 100;
                    if (string.IsNullOrWhiteSpace(store.Mall.MallCode))
                        store.Mall.MallCode = $"MALL-{store.StoreNumber}";
                    state.Malls.Add(store.Mall);
                }
            }

            state.Stores.Add(store);

            string wiId = $"SBX-{store.StoreNumber}-01";
            state.WorkItems.Add(new CalendarWorkItem
            {
                WorkItemId = wiId,
                ActivityDate = DateTime.Now.AddDays(5),
                FiscalYear = GetFiscalYearForDate(DateTime.Now.AddDays(5)),
                FiscalWeek = GetFiscalWeekForDate(DateTime.Now.AddDays(5)),
                ActivityType = "Site Survey",
                Title = $"Physical Infrastructure & Low-Volt Survey — Store #{store.StoreNumber}",
                StoreNumber = store.StoreNumber,
                StoreName = store.StoreName,
                BannerCode = store.BannerCode,
                Status = "Scheduled",
                AssignedTo = "National Site Surveyors",
                TimeWindow = "09:00 - 12:00",
                Notes = $"New store onboarding for {store.StoreName} ({store.BannerName})."
            });

            SaveState();
            return true;
        }

        public static bool SaveStore(StoreDeployment updated)
        {
            var existing = State.Stores.FirstOrDefault(s =>
                s.StoreId == updated.StoreId ||
                s.StoreNumber.Equals(updated.StoreNumber, StringComparison.OrdinalIgnoreCase));

            if (existing != null)
            {
                int idx = State.Stores.IndexOf(existing);
                State.Stores[idx] = updated;
            }
            else
            {
                State.Stores.Add(updated);
            }

            if (updated.Mall != null)
            {
                SaveMall(updated.Mall);
            }

            RelinkStoreMalls(State);
            SaveState();
            return true;
        }

        public static bool SaveMall(MallProperty updated)
        {
            var existing = State.Malls.FirstOrDefault(m =>
                m.MallId == updated.MallId ||
                m.MallCode.Equals(updated.MallCode, StringComparison.OrdinalIgnoreCase));

            if (existing != null)
            {
                int idx = State.Malls.IndexOf(existing);
                State.Malls[idx] = updated;
            }
            else
            {
                if (updated.MallId == 0)
                    updated.MallId = State.Malls.Count > 0 ? State.Malls.Max(m => m.MallId) + 1 : 501;
                State.Malls.Add(updated);
            }

            RelinkStoreMalls(State);
            SaveState();
            return true;
        }

        public static void SaveWorkItem(CalendarWorkItem item)
        {
            item.FiscalWeek = GetFiscalWeekForDate(item.ActivityDate);
            item.FiscalYear = GetFiscalYearForDate(item.ActivityDate);

            var existing = State.WorkItems.FirstOrDefault(w => w.WorkItemId == item.WorkItemId);
            if (existing != null)
            {
                int idx = State.WorkItems.IndexOf(existing);
                State.WorkItems[idx] = item;
            }
            else
            {
                State.WorkItems.Add(item);
            }
            SaveState();
        }

        public static void DeleteWorkItem(string workItemId)
        {
            State.WorkItems.RemoveAll(w => w.WorkItemId == workItemId);
            SaveState();
        }

        public static int GetFiscalWeekForDate(DateTime date)
        {
            // NRF 4-5-4 FY2026 starts Feb 1, 2026 (Week 1 = Feb 1..Feb 7, 2026)
            // Oct 11-17, 2026 = Week 37 in standard Feb 1 count, or aligned so Oct 11-17 = Week 41, Oct 18-24 = Week 42
            // Let's align exactly with the app's active calendar: Oct 11, 2026 = start of Week 41, Oct 18, 2026 = start of Week 42!
            DateTime week41Start = new DateTime(2026, 10, 11);
            int diffDays = (date.Date - week41Start).Days;
            int weekOffset = (int)Math.Floor(diffDays / 7.0);
            int week = 41 + weekOffset;
            return Math.Clamp(week, 1, 52);
        }

        public static int GetFiscalYearForDate(DateTime date)
        {
            if (date.Year < 2025) return 2025;
            if (date.Year > 2027) return 2027;
            return date.Year;
        }

        public static (DateTime Start, DateTime End) GetDateRangeForFiscalWeek(int fiscalWeek)
        {
            DateTime week41Start = new DateTime(2026, 10, 11);
            DateTime start = week41Start.AddDays((fiscalWeek - 41) * 7);
            DateTime end = start.AddDays(6);
            return (start, end);
        }

        private static DummyDatabaseState CreateDefaultDummyDatabase()
        {
            return DummyDataGenerator.Generate200StoreFleet();
        }
    }
}

