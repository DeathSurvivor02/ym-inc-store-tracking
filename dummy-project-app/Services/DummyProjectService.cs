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
            var malls = new List<MallProperty>
            {
                new()
                {
                    MallId = 501,
                    MallCode = "MALL-SBX01",
                    PropertyName = "NorthStar Galleria (Sandbox)",
                    LandlordCompany = "Apex Retail REIT",
                    PropertyType = "Enclosed Regional Mall",
                    Address = "100 NorthStar Pkwy",
                    City = "Austin",
                    StateProvince = "TX",
                    CountryCode = "US",
                    CoiRequired = true,
                    MinGeneralLiabilityCad = 5000000m,
                    DeliveryWindow = "06:00 - 09:00",
                    NightWorkOnly = true,
                    BadgingFeePerTech = 20m,
                    CablingVendor = "Velocity Low-Voltage",
                    LeadTechnicianName = "Carlos Mendez",
                    MallContacted = true,
                    AssignedTeamMember = "Marcus Vance",
                    ManagementContactName = "Elena Rostova",
                    ManagementPhone = "(512) 555-0140",
                    ManagementEmail = "erostova@northstargalleria.example",
                    SecurityPhone = "(512) 555-0199",
                    SpecialRules = "Hot work & ceiling core drilling only after 22:00. Security escort required for MPOE room.",
                    RequirementsCircuits = "Dedicated 2-inch innerduct required from South Riser to tenant IDF.",
                    IntermediateSteps = "COI approved. Night shift badges issued for Week 42."
                },
                new()
                {
                    MallId = 502,
                    MallCode = "MALL-SBX02",
                    PropertyName = "Pacific Cascade Center (Sandbox)",
                    LandlordCompany = "Cascade Commercial Group",
                    PropertyType = "Enclosed Regional Mall",
                    Address = "450 Pine St",
                    City = "Seattle",
                    StateProvince = "WA",
                    CountryCode = "US",
                    CoiRequired = true,
                    MinGeneralLiabilityCad = 10000000m,
                    DeliveryWindow = "05:30 - 08:30",
                    NightWorkOnly = false,
                    BadgingFeePerTech = 25m,
                    CablingVendor = "SoundView Cabling",
                    LeadTechnicianName = "Nadia K.",
                    MallContacted = true,
                    AssignedTeamMember = "Claire Tremblay",
                    ManagementContactName = "Devon Brooks",
                    ManagementPhone = "(206) 555-0182",
                    ManagementEmail = "dbrooks@pacificcascade.example",
                    SecurityPhone = "(206) 555-0111",
                    SpecialRules = "Plenum-rated Cat6 CMP mandatory. Scissor lift requires floor mats.",
                    RequirementsCircuits = "Lumen & Comcast fiber handoff located in Sub-Basement Demarc B2.",
                    IntermediateSteps = "Demarc cross-connect authorized by landlord."
                },
                new()
                {
                    MallId = 503,
                    MallCode = "MALL-SBX03",
                    PropertyName = "Harbourfront Promenade (Sandbox)",
                    LandlordCompany = "Northern Crown Properties",
                    PropertyType = "Enclosed Regional Mall",
                    Address = "880 Lakeshore Blvd",
                    City = "Toronto",
                    StateProvince = "ON",
                    CountryCode = "CA",
                    CoiRequired = true,
                    MinGeneralLiabilityCad = 10000000m,
                    DeliveryWindow = "06:00 - 09:30",
                    NightWorkOnly = true,
                    BadgingFeePerTech = 30m,
                    CablingVendor = "Maple Leaf Telecom",
                    LeadTechnicianName = "Liam Foster",
                    MallContacted = true,
                    AssignedTeamMember = "Terry O'Connor",
                    ManagementContactName = "Sophie Laurent",
                    ManagementPhone = "(416) 555-0164",
                    ManagementEmail = "slaurent@harbourfront.example",
                    SecurityPhone = "(416) 555-0100",
                    SpecialRules = "Union electrical sign-off required for Wiremold 25DTP-4 drop poles.",
                    RequirementsCircuits = "Bell Canada fiber tray access requires 48h advance ticket.",
                    IntermediateSteps = "Electrical permit submitted; awaiting final union clearance."
                },
                new()
                {
                    MallId = 504,
                    MallCode = "MALL-SBX04",
                    PropertyName = "Mile High Crossing (Sandbox)",
                    LandlordCompany = "Summit Retail Partners",
                    PropertyType = "Outlet Centre",
                    Address = "2200 Rockies Ave",
                    City = "Denver",
                    StateProvince = "CO",
                    CountryCode = "US",
                    CoiRequired = true,
                    MinGeneralLiabilityCad = 5000000m,
                    DeliveryWindow = "07:00 - 10:00",
                    NightWorkOnly = false,
                    BadgingFeePerTech = 15m,
                    CablingVendor = "FrontRange Structured Nets",
                    LeadTechnicianName = "Brett Collins",
                    MallContacted = true,
                    AssignedTeamMember = "Danielle Levy",
                    ManagementContactName = "Grant Mercer",
                    ManagementPhone = "(303) 555-0128",
                    ManagementEmail = "gmercer@milehighcrossing.example",
                    SecurityPhone = "(303) 555-0129",
                    SpecialRules = "Standard daytime work allowed inside backroom IDF; sales floor ladder work before 10:00 AM.",
                    RequirementsCircuits = "Direct conduit to suite rear wall.",
                    IntermediateSteps = "All permits cleared."
                },
                new()
                {
                    MallId = 505,
                    MallCode = "MALL-SBX05",
                    PropertyName = "Saint-Laurent Quartier (Sandbox)",
                    LandlordCompany = "Mont-Royal Immobilier",
                    PropertyType = "Enclosed Regional Mall",
                    Address = "1250 Rue Sainte-Catherine",
                    City = "Montreal",
                    StateProvince = "QC",
                    CountryCode = "CA",
                    CoiRequired = true,
                    MinGeneralLiabilityCad = 5000000m,
                    DeliveryWindow = "06:30 - 09:30",
                    NightWorkOnly = true,
                    BadgingFeePerTech = 25m,
                    CablingVendor = "Nordique Cabling Inc.",
                    LeadTechnicianName = "Jean-Luc Picard",
                    MallContacted = false,
                    AssignedTeamMember = "Hector Ruiz",
                    ManagementContactName = "Chantal Moreau",
                    ManagementPhone = "(514) 555-0177",
                    ManagementEmail = "cmoreau@stlaurentquartier.example",
                    SecurityPhone = "(514) 555-0170",
                    SpecialRules = "",
                    RequirementsCircuits = "",
                    IntermediateSteps = "Initial outreach email sent; awaiting landlord operations packet."
                },
                new()
                {
                    MallId = 506,
                    MallCode = "MALL-SBX06",
                    PropertyName = "Bayside Sun Plaza (Sandbox)",
                    LandlordCompany = "Atlantic Coast Realty",
                    PropertyType = "Power Centre / Strip Mall",
                    Address = "740 Ocean Drive",
                    City = "Miami",
                    StateProvince = "FL",
                    CountryCode = "US",
                    CoiRequired = false,
                    MinGeneralLiabilityCad = 5000000m,
                    DeliveryWindow = "07:00 - 11:00",
                    NightWorkOnly = false,
                    BadgingFeePerTech = 0m,
                    CablingVendor = "Sunbelt Data Systems",
                    LeadTechnicianName = "Diego Morales",
                    MallContacted = true,
                    AssignedTeamMember = "Sarah Jenkins",
                    ManagementContactName = "Valerie Vega",
                    ManagementPhone = "(305) 555-0192",
                    ManagementEmail = "vvega@baysidesun.example",
                    SecurityPhone = "(305) 555-0190",
                    SpecialRules = "No special restrictions. Rear loading bay accessible 24/7.",
                    RequirementsCircuits = "AT&T fiber terminal on exterior back wall.",
                    IntermediateSteps = "Confirmed no special landlord rules required."
                }
            };

            var mallMap = malls.ToDictionary(m => m.MallCode);

            var stores = new List<StoreDeployment>
            {
                new()
                {
                    StoreId = 901, StoreNumber = "9101", BannerCode = "CR", BannerName = "Charlotte Russe",
                    StoreName = "Austin Flagship (Sandbox)", ProjectType = "New Store Opening",
                    OperationalStatus = "Grand Open", OverallHealth = "Complete", CountryCode = "US",
                    Region = "US South Central", District = "Texas Central",
                    Address = "100 NorthStar Pkwy Ste 110", City = "Austin", StateProvince = "TX", PostalCode = "78701",
                    GrossSqft = 7400, SalesSqft = 6100, PosTerminalCount = 5, PosStagingStatus = "Commissioned",
                    TargetOpenDate = "2026-10-05", OpeningDate = "2026-10-05", PhoneNumber = "(512) 555-9101",
                    Mall = mallMap["MALL-SBX01"],
                    Survey = new SiteSurvey { StoreId = 901, SurveyVendor = "Apex Field Techs", SurveyorName = "Ryan Cooper", ScheduledDate = "2026-09-08", ActualDate = "2026-09-08", Status = "Completed & Approved", CeilingType = "Drop Ceiling (2x4 Acoustic)", CeilingHeightFeet = 14.0, DemarcLocation = "South Riser IDF-2", DemarcConduitAvailable = true, MdfRackLocation = "Back Office Rack A", StructureNotes = "Dummy sandbox store — all inspections passed." },
                    PowerPoles = new PowerPole { StoreId = 901, Status = "Fully Installed & Certified", TotalRequired = 2, InstalledCount = 2, FinishColor = "Matte Black", CeilingMountType = "T-Bar Grid Clamp", ElectricalContractor = "LoneStar Electric", CashWrapFeedDrops = 4, ScheduledInstallDate = "2026-09-16", ActualInstallDate = "2026-09-16" },
                    Cabling = new CablingOperation { StoreId = 901, Vendor = "Velocity Low-Voltage", LeadTechName = "Carlos Mendez", Status = "Fluke Certified & Closed", TotalDataDrops = 26, PosDrops = 5, ApDrops = 4, FlukeCertified = true, ScheduledStartDate = "2026-09-18", ScheduledCompletionDate = "2026-09-22", ActualCompletionDate = "2026-09-21" },
                    Circuits = new List<TelecomCircuit>
                    {
                        new() { CircuitId = 9001, StoreId = 901, Role = "Primary WAN", CarrierName = "AT&T Enterprise", ServiceType = "Commercial Fiber DIA", OrderStatus = "Tested & Turned Up (Active)", BandwidthDownMbps = 300, BandwidthUpMbps = 300, FocDate = "2026-09-24", TurnupDate = "2026-09-26", DemarcExtended = true },
                        new() { CircuitId = 9002, StoreId = 901, Role = "Secondary Failover WAN", CarrierName = "Verizon 5G", ServiceType = "Enterprise 5G Wireless", OrderStatus = "Tested & Turned Up (Active)", BandwidthDownMbps = 100, BandwidthUpMbps = 30, TurnupDate = "2026-09-25", DemarcExtended = true }
                    },
                    WeeklyCompletionPct = 100, WeeklyStatus = "Completed & Certified",
                    WeeklySummaryNote = "Sandbox Store #9101 fully commissioned in Week 39."
                },
                new()
                {
                    StoreId = 902, StoreNumber = "9102", BannerCode = "UP", BannerName = "Urban Planet",
                    StoreName = "Seattle Cascade (Sandbox)", ProjectType = "Major Remodel",
                    OperationalStatus = "Grand Open", OverallHealth = "On Track", CountryCode = "US",
                    Region = "US Pacific Northwest", District = "Puget Sound Metro",
                    Address = "450 Pine St Suite 204", City = "Seattle", StateProvince = "WA", PostalCode = "98101",
                    GrossSqft = 11200, SalesSqft = 9400, PosTerminalCount = 6, PosStagingStatus = "Commissioned",
                    TargetOpenDate = "2026-10-12", OpeningDate = "2026-10-12", PhoneNumber = "(206) 555-9102",
                    Mall = mallMap["MALL-SBX02"],
                    Survey = new SiteSurvey { StoreId = 902, SurveyVendor = "NW Site Surveys", SurveyorName = "kimberly Chen", ScheduledDate = "2026-09-12", ActualDate = "2026-09-12", Status = "Completed & Approved", CeilingType = "Open Plenum Deck", CeilingHeightFeet = 16.5, DemarcLocation = "Sub-Basement Demarc B2", DemarcConduitAvailable = true, MdfRackLocation = "Rear Corridor MDF", StructureNotes = "16.5ft deck cleared for beam clamps." },
                    PowerPoles = new PowerPole { StoreId = 902, Status = "Fully Installed & Certified", TotalRequired = 3, InstalledCount = 3, FinishColor = "Anodized Aluminum", CeilingMountType = "Structural Beam Clamp", ElectricalContractor = "Cascade Spark Ltd", CashWrapFeedDrops = 6, ScheduledInstallDate = "2026-09-24", ActualInstallDate = "2026-09-24" },
                    Cabling = new CablingOperation { StoreId = 902, Vendor = "SoundView Cabling", LeadTechName = "Nadia K.", Status = "Fluke Certified & Closed", TotalDataDrops = 32, PosDrops = 6, ApDrops = 6, FlukeCertified = true, ScheduledStartDate = "2026-09-25", ScheduledCompletionDate = "2026-09-30", ActualCompletionDate = "2026-09-30" },
                    Circuits = new List<TelecomCircuit>
                    {
                        new() { CircuitId = 9003, StoreId = 902, Role = "Primary WAN", CarrierName = "Lumen Fiber", ServiceType = "Commercial Fiber DIA", OrderStatus = "Tested & Turned Up (Active)", BandwidthDownMbps = 500, BandwidthUpMbps = 500, FocDate = "2026-10-02", TurnupDate = "2026-10-04", DemarcExtended = true },
                        new() { CircuitId = 9004, StoreId = 902, Role = "Secondary Failover WAN", CarrierName = "T-Mobile Business", ServiceType = "Enterprise 5G Wireless", OrderStatus = "Tested & Turned Up (Active)", BandwidthDownMbps = 150, BandwidthUpMbps = 40, TurnupDate = "2026-10-03", DemarcExtended = true }
                    },
                    WeeklyCompletionPct = 100, WeeklyStatus = "Completed & Certified",
                    WeeklySummaryNote = "Completed TTU and POS QA in Week 40."
                },
                new()
                {
                    StoreId = 903, StoreNumber = "9103", BannerCode = "BN", BannerName = "Bluenotes",
                    StoreName = "Harbourfront East (Sandbox)", ProjectType = "New Store Opening",
                    OperationalStatus = "IT Staging & Telecom", OverallHealth = "On Track", CountryCode = "CA",
                    Region = "Central Ontario", District = "Downtown Toronto",
                    Address = "880 Lakeshore Blvd Unit 105", City = "Toronto", StateProvince = "ON", PostalCode = "M5J 2V1",
                    GrossSqft = 8200, SalesSqft = 6900, PosTerminalCount = 5, PosStagingStatus = "Commissioned",
                    TargetOpenDate = "2026-10-24", OpeningDate = "2026-10-24", PhoneNumber = "(416) 555-9103",
                    Mall = mallMap["MALL-SBX03"],
                    Survey = new SiteSurvey { StoreId = 903, SurveyVendor = "Ontario Field Ops", SurveyorName = "Evan Wright", ScheduledDate = "2026-09-18", ActualDate = "2026-09-19", Status = "Completed & Approved", CeilingType = "Drop Ceiling (2x4 Acoustic)", CeilingHeightFeet = 14.0, DemarcLocation = "Main Telco Vault L1", DemarcConduitAvailable = true, MdfRackLocation = "Backroom Wall Rack", StructureNotes = "All pathways verified." },
                    PowerPoles = new PowerPole { StoreId = 903, Status = "Fully Installed & Certified", TotalRequired = 2, InstalledCount = 2, FinishColor = "Matte Black", CeilingMountType = "T-Bar Grid Clamp", ElectricalContractor = "Metro North Electric", CashWrapFeedDrops = 4, ScheduledInstallDate = "2026-10-08", ActualInstallDate = "2026-10-09" },
                    Cabling = new CablingOperation { StoreId = 903, Vendor = "Maple Leaf Telecom", LeadTechName = "Liam Foster", Status = "Fluke Certified & Closed", TotalDataDrops = 28, PosDrops = 5, ApDrops = 4, FlukeCertified = true, ScheduledStartDate = "2026-10-10", ScheduledCompletionDate = "2026-10-14", ActualCompletionDate = "2026-10-14" },
                    Circuits = new List<TelecomCircuit>
                    {
                        new() { CircuitId = 9005, StoreId = 903, Role = "Primary WAN", CarrierName = "Bell Canada", ServiceType = "Commercial Fiber DIA", OrderStatus = "Tested & Turned Up (Active)", BandwidthDownMbps = 250, BandwidthUpMbps = 250, FocDate = "2026-10-14", TurnupDate = "2026-10-16", DemarcExtended = true },
                        new() { CircuitId = 9006, StoreId = 903, Role = "Secondary Failover WAN", CarrierName = "Rogers Business", ServiceType = "Enterprise 5G Wireless", OrderStatus = "Tested & Turned Up (Active)", BandwidthDownMbps = 100, BandwidthUpMbps = 30, TurnupDate = "2026-10-15", DemarcExtended = true }
                    },
                    WeeklyCompletionPct = 100, WeeklyStatus = "Completed & Certified",
                    WeeklySummaryNote = "Bell Fiber TTU completed in Week 41. Ready for soft opening."
                },
                new()
                {
                    StoreId = 904, StoreNumber = "9104", BannerCode = "R21", BannerName = "rue21",
                    StoreName = "Denver Mile High (Sandbox)", ProjectType = "New Store Opening",
                    OperationalStatus = "IT Staging & Telecom", OverallHealth = "On Track", CountryCode = "US",
                    Region = "US Mountain West", District = "Colorado Front Range",
                    Address = "2200 Rockies Ave Spc 312", City = "Denver", StateProvince = "CO", PostalCode = "80202",
                    GrossSqft = 6600, SalesSqft = 5400, PosTerminalCount = 4, PosStagingStatus = "Commissioned",
                    TargetOpenDate = "2026-10-28", OpeningDate = "2026-10-28", PhoneNumber = "(303) 555-9104",
                    Mall = mallMap["MALL-SBX04"],
                    Survey = new SiteSurvey { StoreId = 904, SurveyVendor = "Summit Surveys", SurveyorName = "cole Hudson", ScheduledDate = "2026-09-22", ActualDate = "2026-09-22", Status = "Completed & Approved", CeilingType = "Drop Ceiling (2x2 Tiles)", CeilingHeightFeet = 13.5, DemarcLocation = "Suite Rear Conduit", DemarcConduitAvailable = true, MdfRackLocation = "Storage IDF Closet", StructureNotes = "Clean conduit path." },
                    PowerPoles = new PowerPole { StoreId = 904, Status = "Fully Installed & Certified", TotalRequired = 2, InstalledCount = 2, FinishColor = "Anodized Aluminum", CeilingMountType = "T-Bar Grid Clamp", ElectricalContractor = "Peak Volt Inc", CashWrapFeedDrops = 4, ScheduledInstallDate = "2026-10-12", ActualInstallDate = "2026-10-12" },
                    Cabling = new CablingOperation { StoreId = 904, Vendor = "FrontRange Structured Nets", LeadTechName = "Brett Collins", Status = "Fluke Certified & Closed", TotalDataDrops = 24, PosDrops = 4, ApDrops = 4, FlukeCertified = true, ScheduledStartDate = "2026-10-13", ScheduledCompletionDate = "2026-10-17", ActualCompletionDate = "2026-10-17" },
                    Circuits = new List<TelecomCircuit>
                    {
                        new() { CircuitId = 9007, StoreId = 904, Role = "Primary WAN", CarrierName = "Comcast Business", ServiceType = "Commercial Fiber DIA", OrderStatus = "Tested & Turned Up (Active)", BandwidthDownMbps = 200, BandwidthUpMbps = 200, FocDate = "2026-10-17", TurnupDate = "2026-10-19", DemarcExtended = true },
                        new() { CircuitId = 9008, StoreId = 904, Role = "Secondary Failover WAN", CarrierName = "AT&T Wireless", ServiceType = "Enterprise 5G Wireless", OrderStatus = "Tested & Turned Up (Active)", BandwidthDownMbps = 100, BandwidthUpMbps = 25, TurnupDate = "2026-10-18", DemarcExtended = true }
                    },
                    WeeklyCompletionPct = 100, WeeklyStatus = "Completed & Certified",
                    WeeklySummaryNote = "Primary Fiber TTU and Fluke certification completed in Week 42."
                },
                new()
                {
                    StoreId = 905, StoreNumber = "9105", BannerCode = "M", BannerName = "Mandee",
                    StoreName = "Bayside Miami (Sandbox)", ProjectType = "Major Remodel",
                    OperationalStatus = "Low-Voltage & Power Poles", OverallHealth = "On Track", CountryCode = "US",
                    Region = "US Southeast", District = "Miami-Dade Coastal",
                    Address = "740 Ocean Drive Ste 18", City = "Miami", StateProvince = "FL", PostalCode = "33139",
                    GrossSqft = 7100, SalesSqft = 5900, PosTerminalCount = 4, PosStagingStatus = "Hardware Staged",
                    TargetOpenDate = "2026-11-08", OpeningDate = "2026-11-08", PhoneNumber = "(305) 555-9105",
                    Mall = mallMap["MALL-SBX06"],
                    Survey = new SiteSurvey { StoreId = 905, SurveyVendor = "Sunbelt Surveys", SurveyorName = "Marco Diaz", ScheduledDate = "2026-09-29", ActualDate = "2026-09-30", Status = "Completed & Approved", CeilingType = "Drop Ceiling (2x4 Acoustic)", CeilingHeightFeet = 14.0, DemarcLocation = "Rear Exterior Terminal", DemarcConduitAvailable = true, MdfRackLocation = "Backroom Office Wall", StructureNotes = "Ready for low-voltage pull." },
                    PowerPoles = new PowerPole { StoreId = 905, Status = "Fully Installed & Certified", TotalRequired = 2, InstalledCount = 2, FinishColor = "Matte Black", CeilingMountType = "T-Bar Grid Clamp", ElectricalContractor = "Biscayne Electric", CashWrapFeedDrops = 4, ScheduledInstallDate = "2026-10-18", ActualInstallDate = "2026-10-18" },
                    Cabling = new CablingOperation { StoreId = 905, Vendor = "Sunbelt Data Systems", LeadTechName = "Diego Morales", Status = "Pathway & J-Hooks Installed", TotalDataDrops = 24, PosDrops = 4, ApDrops = 4, FlukeCertified = false, ScheduledStartDate = "2026-10-19", ScheduledCompletionDate = "2026-10-23" },
                    Circuits = new List<TelecomCircuit>
                    {
                        new() { CircuitId = 9009, StoreId = 905, Role = "Primary WAN", CarrierName = "AT&T Fiber", ServiceType = "Commercial Fiber DIA", OrderStatus = "FOC Confirmed", BandwidthDownMbps = 300, BandwidthUpMbps = 300, FocDate = "2026-10-21", TurnupDate = "2026-10-22", DemarcExtended = true },
                        new() { CircuitId = 9010, StoreId = 905, Role = "Secondary Failover WAN", CarrierName = "Verizon 5G", ServiceType = "Enterprise 5G Wireless", OrderStatus = "Tested & Turned Up (Active)", BandwidthDownMbps = 100, BandwidthUpMbps = 25, TurnupDate = "2026-10-17", DemarcExtended = true }
                    },
                    WeeklyCompletionPct = 78, WeeklyStatus = "In Progress / On Schedule",
                    WeeklySummaryNote = "Power poles installed Oct 18. Cabling pull & AT&T circuit TTU scheduled this week (Week 42)."
                },
                new()
                {
                    StoreId = 906, StoreNumber = "9106", BannerCode = "UP", BannerName = "Urban Planet",
                    StoreName = "Quartier Montreal (Sandbox)", ProjectType = "New Store Opening",
                    OperationalStatus = "Low-Voltage & Power Poles", OverallHealth = "Critical Blocker", CountryCode = "CA",
                    Region = "Quebec Metro", District = "Greater Montreal",
                    Address = "1250 Rue Sainte-Catherine #210", City = "Montreal", StateProvince = "QC", PostalCode = "H3B 1B4",
                    GrossSqft = 12400, SalesSqft = 10200, PosTerminalCount = 6, PosStagingStatus = "Pending",
                    TargetOpenDate = "2026-11-18", OpeningDate = "2026-11-18", PhoneNumber = "(514) 555-9106",
                    Mall = mallMap["MALL-SBX05"],
                    Survey = new SiteSurvey { StoreId = 906, SurveyVendor = "Quebec Site Eng", SurveyorName = "Luc Bouchard", ScheduledDate = "2026-10-04", ActualDate = "2026-10-05", Status = "Completed & Approved", CeilingType = "Gypsum Bulkhead + Open Bay", CeilingHeightFeet = 15.5, DemarcLocation = "Level B1 Riser Room", DemarcConduitAvailable = false, MdfRackLocation = "Backroom Rack B", StructureNotes = "Riser conduit blocked; needs landlord riser access." },
                    PowerPoles = new PowerPole { StoreId = 906, Status = "Blocked / On Hold", TotalRequired = 3, InstalledCount = 1, FinishColor = "Matte Black", CeilingMountType = "Structural Tie-Wire", ElectricalContractor = "Electra QC", CashWrapFeedDrops = 6, ScheduledInstallDate = "2026-10-20", BlockerReason = "Landlord security halted night work pending updated COI endorsement." },
                    Cabling = new CablingOperation { StoreId = 906, Vendor = "Nordique Cabling Inc.", LeadTechName = "Jean-Luc Picard", Status = "Blocked", TotalDataDrops = 34, PosDrops = 6, ApDrops = 6, FlukeCertified = false, ScheduledStartDate = "2026-10-21", ScheduledCompletionDate = "2026-10-27" },
                    Circuits = new List<TelecomCircuit>
                    {
                        new() { CircuitId = 9011, StoreId = 906, Role = "Primary WAN", CarrierName = "Videotron Business", ServiceType = "Commercial Fiber DIA", OrderStatus = "Carrier Delay / Jeopardy", BandwidthDownMbps = 250, BandwidthUpMbps = 250, FocDate = "2026-10-25", JeopardyReason = "Landlord riser room key clearance pending.", DemarcExtended = false }
                    },
                    WeeklyCompletionPct = 42, WeeklyStatus = "Blocked / In Jeopardy",
                    WeeklySummaryNote = "Blocked by landlord COI & riser clearance at Saint-Laurent Quartier."
                },
                new()
                {
                    StoreId = 907, StoreNumber = "9107", BannerCode = "CR", BannerName = "Charlotte Russe",
                    StoreName = "NorthStar Annex (Sandbox)", ProjectType = "Minor Refresh",
                    OperationalStatus = "IT Staging & Telecom", OverallHealth = "On Track", CountryCode = "US",
                    Region = "US South Central", District = "Texas Central",
                    Address = "100 NorthStar Pkwy Ste 340", City = "Austin", StateProvince = "TX", PostalCode = "78701",
                    GrossSqft = 6200, SalesSqft = 5100, PosTerminalCount = 4, PosStagingStatus = "Hardware Staged",
                    TargetOpenDate = "2026-11-02", OpeningDate = "2026-11-02", PhoneNumber = "(512) 555-9107",
                    Mall = mallMap["MALL-SBX01"],
                    Survey = new SiteSurvey { StoreId = 907, SurveyVendor = "Apex Field Techs", SurveyorName = "Ryan Cooper", ScheduledDate = "2026-10-01", ActualDate = "2026-10-01", Status = "Completed & Approved", CeilingType = "Drop Ceiling (2x4 Acoustic)", CeilingHeightFeet = 14.0, DemarcLocation = "North Riser IDF", DemarcConduitAvailable = true, MdfRackLocation = "Backroom Closet", StructureNotes = "Existing conduit verified." },
                    PowerPoles = new PowerPole { StoreId = 907, Status = "Fully Installed & Certified", TotalRequired = 2, InstalledCount = 2, FinishColor = "Anodized Aluminum", CeilingMountType = "T-Bar Grid Clamp", ElectricalContractor = "LoneStar Electric", CashWrapFeedDrops = 4, ScheduledInstallDate = "2026-10-15", ActualInstallDate = "2026-10-15" },
                    Cabling = new CablingOperation { StoreId = 907, Vendor = "Velocity Low-Voltage", LeadTechName = "Carlos Mendez", Status = "Fluke Certified & Closed", TotalDataDrops = 22, PosDrops = 4, ApDrops = 4, FlukeCertified = true, ScheduledStartDate = "2026-10-16", ScheduledCompletionDate = "2026-10-20", ActualCompletionDate = "2026-10-20" },
                    Circuits = new List<TelecomCircuit>
                    {
                        new() { CircuitId = 9012, StoreId = 907, Role = "Primary WAN", CarrierName = "Spectrum Enterprise", ServiceType = "Commercial Fiber DIA", OrderStatus = "Tested & Turned Up (Active)", BandwidthDownMbps = 200, BandwidthUpMbps = 200, FocDate = "2026-10-19", TurnupDate = "2026-10-20", DemarcExtended = true },
                        new() { CircuitId = 9013, StoreId = 907, Role = "Secondary Failover WAN", CarrierName = "AT&T 5G", ServiceType = "Enterprise 5G Wireless", OrderStatus = "Tested & Turned Up (Active)", BandwidthDownMbps = 100, BandwidthUpMbps = 25, TurnupDate = "2026-10-19", DemarcExtended = true }
                    },
                    WeeklyCompletionPct = 95, WeeklyStatus = "Completed & Certified",
                    WeeklySummaryNote = "Cabling & Dual-WAN circuits certified on Oct 20 (Week 42)."
                },
                new()
                {
                    StoreId = 908, StoreNumber = "9108", BannerCode = "BN", BannerName = "Bluenotes",
                    StoreName = "Pacific Promenade (Sandbox)", ProjectType = "New Store Opening",
                    OperationalStatus = "Site Survey", OverallHealth = "On Track", CountryCode = "US",
                    Region = "US Pacific Northwest", District = "Puget Sound Metro",
                    Address = "450 Pine St Suite 415", City = "Seattle", StateProvince = "WA", PostalCode = "98101",
                    GrossSqft = 7800, SalesSqft = 6400, PosTerminalCount = 4, PosStagingStatus = "Pending",
                    TargetOpenDate = "2026-11-25", OpeningDate = "2026-11-25", PhoneNumber = "(206) 555-9108",
                    Mall = mallMap["MALL-SBX02"],
                    Survey = new SiteSurvey { StoreId = 908, SurveyVendor = "NW Site Surveys", SurveyorName = "Kimberly Chen", ScheduledDate = "2026-10-21", ActualDate = null, Status = "Scheduled", CeilingType = "Drop Ceiling (2x4 Acoustic)", CeilingHeightFeet = 14.5, DemarcLocation = "Sub-Basement Demarc B2", DemarcConduitAvailable = true, MdfRackLocation = "Back Office IDF", StructureNotes = "Site survey scheduled for Week 42." },
                    PowerPoles = new PowerPole { StoreId = 908, Status = "Delivered On Site", TotalRequired = 2, InstalledCount = 0, FinishColor = "Matte Black", CeilingMountType = "T-Bar Grid Clamp", ElectricalContractor = "Cascade Spark Ltd", CashWrapFeedDrops = 4, ScheduledInstallDate = "2026-10-28" },
                    Cabling = new CablingOperation { StoreId = 908, Vendor = "SoundView Cabling", LeadTechName = "Nadia K.", Status = "Vendor Assigned", TotalDataDrops = 24, PosDrops = 4, ApDrops = 4, FlukeCertified = false, ScheduledStartDate = "2026-10-29", ScheduledCompletionDate = "2026-11-04" },
                    Circuits = new List<TelecomCircuit>
                    {
                        new() { CircuitId = 9014, StoreId = 908, Role = "Primary WAN", CarrierName = "Comcast Business", ServiceType = "Commercial Fiber DIA", OrderStatus = "FOC Confirmed", BandwidthDownMbps = 300, BandwidthUpMbps = 300, FocDate = "2026-10-30", DemarcExtended = false }
                    },
                    WeeklyCompletionPct = 35, WeeklyStatus = "Scheduled / Pending",
                    WeeklySummaryNote = "Site survey and initial circuit install scheduled for Week 42–43."
                }
            };

            var workItems = new List<CalendarWorkItem>
            {
                // WEEK 37 (Sep 13 - Sep 19, 2026)
                new() { WorkItemId = "SBX-3701", ActivityDate = new DateTime(2026, 9, 14), FiscalYear = 2026, FiscalWeek = 37, ActivityType = "Site Survey", Title = "Full Infrastructure & Demarc Site Survey", StoreNumber = "9102", StoreName = "Seattle Cascade", BannerCode = "UP", Status = "Completed", AssignedTo = "Claire Tremblay (PA)", TimeWindow = "09:00 - 12:00", Notes = "Verified 16.5ft deck and Sub-Basement B2 fiber handoff." },
                new() { WorkItemId = "SBX-3702", ActivityDate = new DateTime(2026, 9, 16), FiscalYear = 2026, FiscalWeek = 37, ActivityType = "Power Pole Install", Title = "Install 2x Wiremold 25DTP-4 Cash Wrap Poles", StoreNumber = "9101", StoreName = "Austin Flagship", BannerCode = "CR", Status = "Completed", AssignedTo = "Marcus Vance (PM)", TimeWindow = "22:00 - 02:00", Notes = "Night shift install completed and inspected." },
                new() { WorkItemId = "SBX-3703", ActivityDate = new DateTime(2026, 9, 18), FiscalYear = 2026, FiscalWeek = 37, ActivityType = "Site Survey", Title = "Harbourfront Telco Vault & Ceiling Survey", StoreNumber = "9103", StoreName = "Harbourfront East", BannerCode = "BN", Status = "Completed", AssignedTo = "Terry O'Connor (PS1)", TimeWindow = "08:00 - 11:30", Notes = "Approved MDF rack location in backroom." },

                // WEEK 38 (Sep 20 - Sep 26, 2026)
                new() { WorkItemId = "SBX-3801", ActivityDate = new DateTime(2026, 9, 21), FiscalYear = 2026, FiscalWeek = 38, ActivityType = "Low-Voltage Cabling", Title = "Cat6 CMP 26-Drop Pull & Fluke Certification", StoreNumber = "9101", StoreName = "Austin Flagship", BannerCode = "CR", Status = "Completed", AssignedTo = "Carlos Mendez", TimeWindow = "07:00 - 15:00", Notes = "All 26 drops passed Fluke DSX-8000 certification." },
                new() { WorkItemId = "SBX-3802", ActivityDate = new DateTime(2026, 9, 22), FiscalYear = 2026, FiscalWeek = 38, ActivityType = "Site Survey", Title = "Denver Mile High Conduit & Rack Verification", StoreNumber = "9104", StoreName = "Denver Mile High", BannerCode = "R21", Status = "Completed", AssignedTo = "Danielle Levy (PS2)", TimeWindow = "10:00 - 13:00", Notes = "Rear conduit clear." },
                new() { WorkItemId = "SBX-3803", ActivityDate = new DateTime(2026, 9, 24), FiscalYear = 2026, FiscalWeek = 38, ActivityType = "Power Pole Install", Title = "3x Wiremold 25DTP-4 Open Plenum Beam Mount", StoreNumber = "9102", StoreName = "Seattle Cascade", BannerCode = "UP", Status = "Completed", AssignedTo = "Claire Tremblay (PA)", TimeWindow = "06:00 - 10:00", Notes = "Passed electrical inspection." },
                new() { WorkItemId = "SBX-3804", ActivityDate = new DateTime(2026, 9, 26), FiscalYear = 2026, FiscalWeek = 38, ActivityType = "TTU (Test & Turn-Up)", Title = "AT&T 300M Fiber DIA & Verizon 5G Failover TTU", StoreNumber = "9101", StoreName = "Austin Flagship", BannerCode = "CR", Status = "Completed", AssignedTo = "Marcus Vance (PM)", TimeWindow = "09:00 - 11:30", Notes = "Both WAN circuits active, BGP/SD-WAN tunnel up." },

                // WEEK 39 (Sep 27 - Oct 3, 2026)
                new() { WorkItemId = "SBX-3901", ActivityDate = new DateTime(2026, 9, 29), FiscalYear = 2026, FiscalWeek = 39, ActivityType = "Site Survey", Title = "Bayside Miami Exterior Demarc & IDF Survey", StoreNumber = "9105", StoreName = "Bayside Miami", BannerCode = "M", Status = "Completed", AssignedTo = "Sarah Jenkins (PS4)", TimeWindow = "09:00 - 12:00", Notes = "Exterior terminal ready for fiber entry." },
                new() { WorkItemId = "SBX-3902", ActivityDate = new DateTime(2026, 9, 30), FiscalYear = 2026, FiscalWeek = 39, ActivityType = "Low-Voltage Cabling", Title = "32-Drop Cat6 Plenum Pull & Patch Panel Term", StoreNumber = "9102", StoreName = "Seattle Cascade", BannerCode = "UP", Status = "Completed", AssignedTo = "Nadia K.", TimeWindow = "06:00 - 14:00", Notes = "Fluke test report uploaded." },
                new() { WorkItemId = "SBX-3903", ActivityDate = new DateTime(2026, 10, 2), FiscalYear = 2026, FiscalWeek = 39, ActivityType = "Circuit Install", Title = "Lumen 500M Fiber Demarc Extension to Suite 204", StoreNumber = "9102", StoreName = "Seattle Cascade", BannerCode = "UP", Status = "Completed", AssignedTo = "Claire Tremblay (PA)", TimeWindow = "08:00 - 12:00", Notes = "Fiber spliced into MDF patch tray." },
                new() { WorkItemId = "SBX-3904", ActivityDate = new DateTime(2026, 10, 3), FiscalYear = 2026, FiscalWeek = 39, ActivityType = "POS Staging & Cutover", Title = "5-Lane POS Register Commissioning & Payment QA", StoreNumber = "9101", StoreName = "Austin Flagship", BannerCode = "CR", Status = "Completed", AssignedTo = "Marcus Vance (PM)", TimeWindow = "10:00 - 15:00", Notes = "All 5 registers online for Oct 5 Grand Opening." },

                // WEEK 40 (Oct 4 - Oct 10, 2026)
                new() { WorkItemId = "SBX-4001", ActivityDate = new DateTime(2026, 10, 4), FiscalYear = 2026, FiscalWeek = 40, ActivityType = "TTU (Test & Turn-Up)", Title = "Lumen 500M Primary WAN & T-Mobile 5G TTU", StoreNumber = "9102", StoreName = "Seattle Cascade", BannerCode = "UP", Status = "Completed", AssignedTo = "Claire Tremblay (PA)", TimeWindow = "09:00 - 11:00", Notes = "Zero packet loss; SD-WAN failover verified." },
                new() { WorkItemId = "SBX-4002", ActivityDate = new DateTime(2026, 10, 6), FiscalYear = 2026, FiscalWeek = 40, ActivityType = "Site Survey", Title = "Montreal Quartier Riser & Bulkhead Survey", StoreNumber = "9106", StoreName = "Quartier Montreal", BannerCode = "UP", Status = "Completed", AssignedTo = "Hector Ruiz (PS3)", TimeWindow = "08:30 - 12:00", Notes = "Identified riser conduit blockage requiring landlord escort." },
                new() { WorkItemId = "SBX-4003", ActivityDate = new DateTime(2026, 10, 8), FiscalYear = 2026, FiscalWeek = 40, ActivityType = "Power Pole Install", Title = "2x Wiremold 25DTP-4 Cash Wrap Drop Poles", StoreNumber = "9103", StoreName = "Harbourfront East", BannerCode = "BN", Status = "Completed", AssignedTo = "Terry O'Connor (PS1)", TimeWindow = "22:00 - 01:30", Notes = "Union electrician signed off." },
                new() { WorkItemId = "SBX-4004", ActivityDate = new DateTime(2026, 10, 10), FiscalYear = 2026, FiscalWeek = 40, ActivityType = "POS Staging & Cutover", Title = "6-Lane POS & Wi-Fi AP Cutover", StoreNumber = "9102", StoreName = "Seattle Cascade", BannerCode = "UP", Status = "Completed", AssignedTo = "Claire Tremblay (PA)", TimeWindow = "09:00 - 14:00", Notes = "Store #9102 certified for Grand Opening." },

                // WEEK 41 (Oct 11 - Oct 17, 2026)
                new() { WorkItemId = "SBX-4101", ActivityDate = new DateTime(2026, 10, 12), FiscalYear = 2026, FiscalWeek = 41, ActivityType = "Power Pole Install", Title = "Install 2x Anodized Wiremold 25DTP-4 Poles", StoreNumber = "9104", StoreName = "Denver Mile High", BannerCode = "R21", Status = "Completed", AssignedTo = "Danielle Levy (PS2)", TimeWindow = "07:00 - 10:00", Notes = "4 cash wrap circuits energized." },
                new() { WorkItemId = "SBX-4102", ActivityDate = new DateTime(2026, 10, 14), FiscalYear = 2026, FiscalWeek = 41, ActivityType = "Low-Voltage Cabling", Title = "28-Drop Cat6 Certification & Rack Dress", StoreNumber = "9103", StoreName = "Harbourfront East", BannerCode = "BN", Status = "Completed", AssignedTo = "Liam Foster", TimeWindow = "06:00 - 13:00", Notes = "Fluke test passed 100%." },
                new() { WorkItemId = "SBX-4103", ActivityDate = new DateTime(2026, 10, 15), FiscalYear = 2026, FiscalWeek = 41, ActivityType = "Power Pole Install", Title = "2x Wiremold Drop Poles — NorthStar Annex", StoreNumber = "9107", StoreName = "NorthStar Annex", BannerCode = "CR", Status = "Completed", AssignedTo = "Marcus Vance (PM)", TimeWindow = "22:00 - 01:00", Notes = "Completed during night window." },
                new() { WorkItemId = "SBX-4104", ActivityDate = new DateTime(2026, 10, 16), FiscalYear = 2026, FiscalWeek = 41, ActivityType = "TTU (Test & Turn-Up)", Title = "Bell Canada 250M Fiber DIA & Rogers 5G TTU", StoreNumber = "9103", StoreName = "Harbourfront East", BannerCode = "BN", Status = "Completed", AssignedTo = "Terry O'Connor (PS1)", TimeWindow = "09:30 - 12:00", Notes = "Primary & failover circuits turned up." },
                new() { WorkItemId = "SBX-4105", ActivityDate = new DateTime(2026, 10, 17), FiscalYear = 2026, FiscalWeek = 41, ActivityType = "Low-Voltage Cabling", Title = "24-Drop Cat6 Pull & Fluke Certification", StoreNumber = "9104", StoreName = "Denver Mile High", BannerCode = "R21", Status = "Completed", AssignedTo = "Brett Collins", TimeWindow = "07:30 - 15:00", Notes = "Certified all POS and AP drops." },

                // WEEK 42 (Oct 18 - Oct 24, 2026 — CURRENT ACTIVE WEEK)
                new() { WorkItemId = "SBX-4201", ActivityDate = new DateTime(2026, 10, 18), FiscalYear = 2026, FiscalWeek = 42, ActivityType = "Power Pole Install", Title = "2x Wiremold 25DTP-4 Cash Wrap Poles Install", StoreNumber = "9105", StoreName = "Bayside Miami", BannerCode = "M", Status = "Completed", AssignedTo = "Sarah Jenkins (PS4)", TimeWindow = "07:00 - 10:30", Notes = "Installed & verified before Sunday store traffic." },
                new() { WorkItemId = "SBX-4202", ActivityDate = new DateTime(2026, 10, 18), FiscalYear = 2026, FiscalWeek = 42, ActivityType = "TTU (Test & Turn-Up)", Title = "AT&T 5G Failover Cradlepoint Turn-Up", StoreNumber = "9104", StoreName = "Denver Mile High", BannerCode = "R21", Status = "Completed", AssignedTo = "Danielle Levy (PS2)", TimeWindow = "13:00 - 14:30", Notes = "LTE/5G signal -68 dBm verified." },
                new() { WorkItemId = "SBX-4203", ActivityDate = new DateTime(2026, 10, 19), FiscalYear = 2026, FiscalWeek = 42, ActivityType = "TTU (Test & Turn-Up)", Title = "Comcast 200M Fiber DIA Primary Circuit TTU", StoreNumber = "9104", StoreName = "Denver Mile High", BannerCode = "R21", Status = "Completed", AssignedTo = "Danielle Levy (PS2)", TimeWindow = "09:00 - 11:30", Notes = "Primary WAN live on Meraki MX." },
                new() { WorkItemId = "SBX-4204", ActivityDate = new DateTime(2026, 10, 20), FiscalYear = 2026, FiscalWeek = 42, ActivityType = "TTU (Test & Turn-Up)", Title = "Spectrum 200M Fiber DIA & AT&T 5G Dual TTU", StoreNumber = "9107", StoreName = "NorthStar Annex", BannerCode = "CR", Status = "Completed", AssignedTo = "Marcus Vance (PM)", TimeWindow = "08:30 - 11:00", Notes = "Both circuits tested & turned up." },
                new() { WorkItemId = "SBX-4205", ActivityDate = new DateTime(2026, 10, 20), FiscalYear = 2026, FiscalWeek = 42, ActivityType = "Power Pole Install", Title = "3x Wiremold Poles Night Shift Attempt", StoreNumber = "9106", StoreName = "Quartier Montreal", BannerCode = "UP", Status = "Blocked / Delayed", AssignedTo = "Hector Ruiz (PS3)", TimeWindow = "22:00 - 02:00", Notes = "Blocked by mall security pending COI endorsement update." },
                new() { WorkItemId = "SBX-4206", ActivityDate = new DateTime(2026, 10, 21), FiscalYear = 2026, FiscalWeek = 42, ActivityType = "Circuit Install", Title = "AT&T 300M Fiber Demarc Extension & ONT Mount", StoreNumber = "9105", StoreName = "Bayside Miami", BannerCode = "M", Status = "In Progress", AssignedTo = "Sarah Jenkins (PS4)", TimeWindow = "08:00 - 12:00", Notes = "AT&T tech pulling fiber from rear terminal to MDF." },
                new() { WorkItemId = "SBX-4207", ActivityDate = new DateTime(2026, 10, 21), FiscalYear = 2026, FiscalWeek = 42, ActivityType = "Site Survey", Title = "Pacific Promenade Suite 415 Field Survey", StoreNumber = "9108", StoreName = "Pacific Promenade", BannerCode = "BN", Status = "In Progress", AssignedTo = "Claire Tremblay (PA)", TimeWindow = "10:00 - 13:00", Notes = "Measuring drop ceiling height and B2 conduit path." },
                new() { WorkItemId = "SBX-4208", ActivityDate = new DateTime(2026, 10, 22), FiscalYear = 2026, FiscalWeek = 42, ActivityType = "TTU (Test & Turn-Up)", Title = "AT&T 300M Fiber DIA Test & Turn-Up", StoreNumber = "9105", StoreName = "Bayside Miami", BannerCode = "M", Status = "Scheduled", AssignedTo = "Sarah Jenkins (PS4)", TimeWindow = "09:00 - 11:30", Notes = "Scheduled immediately following Oct 21 ONT install." },
                new() { WorkItemId = "SBX-4209", ActivityDate = new DateTime(2026, 10, 23), FiscalYear = 2026, FiscalWeek = 42, ActivityType = "Low-Voltage Cabling", Title = "24-Drop Cat6 Termination & Fluke Certification", StoreNumber = "9105", StoreName = "Bayside Miami", BannerCode = "M", Status = "Scheduled", AssignedTo = "Diego Morales", TimeWindow = "08:00 - 15:00", Notes = "Final patch panel dress and Fluke test." },
                new() { WorkItemId = "SBX-4210", ActivityDate = new DateTime(2026, 10, 24), FiscalYear = 2026, FiscalWeek = 42, ActivityType = "POS Staging & Cutover", Title = "Harbourfront East Soft-Open POS Final Cutover", StoreNumber = "9103", StoreName = "Harbourfront East", BannerCode = "BN", Status = "Scheduled", AssignedTo = "Terry O'Connor (PS1)", TimeWindow = "07:00 - 11:00", Notes = "Final register check before doors open." },

                // WEEK 43 (Oct 25 - Oct 31, 2026)
                new() { WorkItemId = "SBX-4301", ActivityDate = new DateTime(2026, 10, 26), FiscalYear = 2026, FiscalWeek = 43, ActivityType = "Circuit Install", Title = "Videotron 250M Fiber Riser Pull (Rescheduled)", StoreNumber = "9106", StoreName = "Quartier Montreal", BannerCode = "UP", Status = "Scheduled", AssignedTo = "Hector Ruiz (PS3)", TimeWindow = "06:30 - 10:30", Notes = "Pending COI clearance from Mont-Royal Immobilier." },
                new() { WorkItemId = "SBX-4302", ActivityDate = new DateTime(2026, 10, 28), FiscalYear = 2026, FiscalWeek = 43, ActivityType = "Power Pole Install", Title = "2x Wiremold 25DTP-4 Drop Pole Install", StoreNumber = "9108", StoreName = "Pacific Promenade", BannerCode = "BN", Status = "Scheduled", AssignedTo = "Claire Tremblay (PA)", TimeWindow = "06:00 - 09:00", Notes = "Cascade Spark electrical crew booked." },
                new() { WorkItemId = "SBX-4303", ActivityDate = new DateTime(2026, 10, 30), FiscalYear = 2026, FiscalWeek = 43, ActivityType = "Circuit Install", Title = "Comcast 300M Fiber Demarc Extension", StoreNumber = "9108", StoreName = "Pacific Promenade", BannerCode = "BN", Status = "Scheduled", AssignedTo = "Claire Tremblay (PA)", TimeWindow = "08:00 - 12:00", Notes = "FOC confirmed for Oct 30." }
            };

            return new DummyDatabaseState
            {
                ProjectName = "Project Horizon — Sandbox Rollout DB",
                LastUpdatedUtc = DateTime.UtcNow,
                Malls = malls,
                Stores = stores,
                WorkItems = workItems
            };
        }
    }
}
