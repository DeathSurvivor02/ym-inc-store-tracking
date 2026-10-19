using System.Collections.Generic;
using YMInc.StoreDeploymentApp.Models;

namespace YMInc.StoreDeploymentApp.Services
{
    public static class SeedDataService
    {
        public static readonly Dictionary<string, MallProperty> Malls = new()
        {
            ["MALL-YORK"] = new MallProperty
            {
                MallId = 1,
                MallCode = "MALL-YORK",
                PropertyName = "Yorkdale Shopping Centre",
                LandlordCompany = "Oxford Properties",
                PropertyType = "Enclosed Regional Mall",
                City = "Toronto",
                StateProvince = "ON",
                CountryCode = "CA",
                CoiRequired = true,
                MinGeneralLiabilityCad = 10000000m,
                DeliveryWindow = "06:00 - 09:00",
                NightWorkOnly = false,
                BadgingFeePerTech = 25m
            },
            ["MALL-EATN"] = new MallProperty
            {
                MallId = 2,
                MallCode = "MALL-EATN",
                PropertyName = "CF Toronto Eaton Centre",
                LandlordCompany = "Cadillac Fairview",
                PropertyType = "Enclosed Regional Mall",
                City = "Toronto",
                StateProvince = "ON",
                CountryCode = "CA",
                CoiRequired = true,
                MinGeneralLiabilityCad = 10000000m,
                DeliveryWindow = "06:00 - 09:00",
                NightWorkOnly = true,
                BadgingFeePerTech = 25m
            },
            ["MALL-GSP"] = new MallProperty
            {
                MallId = 8,
                MallCode = "MALL-GSP",
                PropertyName = "Westfield Garden State Plaza",
                LandlordCompany = "Unibail-Rodamco-Westfield",
                PropertyType = "Enclosed Regional Mall",
                City = "Paramus",
                StateProvince = "NJ",
                CountryCode = "US",
                CoiRequired = true,
                MinGeneralLiabilityCad = 10000000m,
                DeliveryWindow = "06:00 - 09:00",
                NightWorkOnly = true,
                BadgingFeePerTech = 25m
            },
            ["MALL-SAW"] = new MallProperty
            {
                MallId = 12,
                MallCode = "MALL-SAW",
                PropertyName = "Sawgrass Mills",
                LandlordCompany = "Simon Property Group",
                PropertyType = "Outlet Centre",
                City = "Sunrise",
                StateProvince = "FL",
                CountryCode = "US",
                CoiRequired = true,
                MinGeneralLiabilityCad = 10000000m,
                DeliveryWindow = "07:00 - 10:00",
                NightWorkOnly = false,
                BadgingFeePerTech = 20m
            },
            ["MALL-WBGE"] = new MallProperty
            {
                MallId = 15,
                MallCode = "MALL-WBGE",
                PropertyName = "Woodbridge Center",
                LandlordCompany = "Jones Lang LaSalle",
                PropertyType = "Enclosed Regional Mall",
                City = "Woodbridge",
                StateProvince = "NJ",
                CountryCode = "US",
                CoiRequired = true,
                MinGeneralLiabilityCad = 5000000m,
                DeliveryWindow = "07:00 - 10:00",
                NightWorkOnly = false,
                BadgingFeePerTech = 20m
            },
            ["MALL-METRO"] = new MallProperty
            {
                MallId = 5,
                MallCode = "MALL-METRO",
                PropertyName = "Metropolis at Metrotown",
                LandlordCompany = "Ivanhoé Cambridge",
                PropertyType = "Enclosed Regional Mall",
                City = "Burnaby",
                StateProvince = "BC",
                CountryCode = "CA",
                CoiRequired = true,
                MinGeneralLiabilityCad = 5000000m,
                DeliveryWindow = "06:00 - 09:30",
                NightWorkOnly = false,
                BadgingFeePerTech = 25m
            }
        };

        public static List<StoreDeployment> GetStores()
        {
            return new List<StoreDeployment>
            {
                new StoreDeployment
                {
                    StoreId = 2,
                    StoreNumber = "6142",
                    BannerCode = "BN",
                    BannerName = "Bluenotes",
                    StoreName = "Bluenotes Flagship",
                    ProjectType = "Major Remodel",
                    OperationalStatus = "Low-Voltage & Power Poles",
                    OverallHealth = "At Risk",
                    CountryCode = "CA",
                    Region = "Greater Toronto Area & Central Ontario",
                    District = "Toronto North & York Region",
                    Address = "3401 Dufferin St CRU-218B",
                    City = "Toronto",
                    StateProvince = "ON",
                    PostalCode = "M6A 2T9",
                    GrossSqft = 8500,
                    SalesSqft = 7200,
                    PosTerminalCount = 6,
                    PosStagingStatus = "Hardware Staged",
                    TargetOpenDate = "2026-11-27",
                    OpeningDate = "2002-11-20",
                    Mall = Malls["MALL-YORK"],
                    Survey = new SiteSurvey
                    {
                        SurveyVendor = "CompuCom Field Services",
                        SurveyorName = "Dave Miller",
                        ScheduledDate = "2026-09-15",
                        ActualDate = "2026-09-15",
                        Status = "Completed & Approved",
                        CeilingType = "Drop Ceiling (2x4 Acoustic Tiles)",
                        CeilingHeightFeet = 14.5,
                        DemarcLocation = "Mall Common Demarc Room B1-14",
                        DemarcConduitAvailable = true,
                        MdfRackLocation = "Back Office Cash Wrap IDF",
                        MdfRackSpaceAdequate = true,
                        DedicatedCircuitsCount = 2
                    },
                    PowerPoles = new PowerPole
                    {
                        Status = "Delivered On Site",
                        TotalRequired = 2,
                        InstalledCount = 0,
                        Manufacturer = "Wiremold / Legrand",
                        ModelNumber = "25DTP-4",
                        FinishColor = "Matte Black",
                        CeilingMountType = "T-Bar Grid Clamp with Structural Tie-Wire",
                        ElectricalContractor = "Guild Electric Ltd.",
                        CashWrapFeedDrops = 4,
                        ScheduledInstallDate = "2026-11-02",
                        BlockerReason = "Awaiting landlord night work clearance for ceiling tie-wire anchoring"
                    },
                    Cabling = new CablingOperation
                    {
                        Vendor = "Black Box Network Services",
                        LeadTechName = "Terry O'Connor",
                        Status = "Pathway & J-Hooks Installed",
                        CableGrade = "Cat6 Plenum (CMP)",
                        TotalDataDrops = 28,
                        PosDrops = 6,
                        ApDrops = 4,
                        BackOfficeDrops = 4,
                        CctvDrops = 12,
                        FlukeCertified = false,
                        ScheduledStartDate = "2026-11-04",
                        ScheduledCompletionDate = "2026-11-10"
                    },
                    Circuits = new List<TelecomCircuit>
                    {
                        new TelecomCircuit
                        {
                            CircuitId = 101,
                            Role = "Primary WAN",
                            CarrierName = "Bell Canada",
                            ServiceType = "Commercial Fiber DIA",
                            OrderStatus = "FOC Confirmed",
                            BandwidthDownMbps = 100,
                            BandwidthUpMbps = 100,
                            FocDate = "2026-11-08",
                            DemarcExtended = true
                        },
                        new TelecomCircuit
                        {
                            CircuitId = 102,
                            Role = "Secondary Failover WAN",
                            CarrierName = "Telus",
                            ServiceType = "Enterprise 5G / LTE Wireless",
                            OrderStatus = "Tested & Turned Up (Active)",
                            BandwidthDownMbps = 50,
                            BandwidthUpMbps = 20,
                            TurnupDate = "2026-10-25",
                            DemarcExtended = true
                        }
                    },
                    WeeklyCompletionPct = 62,
                    WeeklyStatus = "In Progress / On Schedule",
                    WeeklySummaryNote = "Pathways and J-hooks completed. Wiremold poles on site, scheduling tie-wire install."
                },
                new StoreDeployment
                {
                    StoreId = 11,
                    StoreNumber = "6004",
                    BannerCode = "CR",
                    BannerName = "Charlotte Russe",
                    StoreName = "Charlotte Russe",
                    ProjectType = "Major Remodel",
                    OperationalStatus = "Low-Voltage & Power Poles",
                    OverallHealth = "At Risk",
                    CountryCode = "US",
                    Region = "US Southeast & Florida",
                    District = "South Florida (Miami/Ft Lauderdale)",
                    Address = "12801 W Sunrise Blvd Spc 408",
                    City = "Sunrise",
                    StateProvince = "FL",
                    PostalCode = "33323",
                    GrossSqft = 8800,
                    SalesSqft = 7400,
                    PosTerminalCount = 6,
                    PosStagingStatus = "Configured",
                    TargetOpenDate = "2026-11-30",
                    OpeningDate = "2018-04-19",
                    Mall = Malls["MALL-SAW"],
                    Survey = new SiteSurvey
                    {
                        SurveyVendor = "National Retail Services US",
                        SurveyorName = "Marcus Brody",
                        ScheduledDate = "2026-10-01",
                        ActualDate = "2026-10-02",
                        Status = "Completed & Approved",
                        CeilingType = "Drop Ceiling (2x2 Acoustic Tiles)",
                        CeilingHeightFeet = 15.0,
                        DemarcLocation = "Sawgrass Mall Demarc Riser C",
                        DemarcConduitAvailable = true,
                        MdfRackLocation = "MDF Back Room",
                        MdfRackSpaceAdequate = true,
                        DedicatedCircuitsCount = 2
                    },
                    PowerPoles = new PowerPole
                    {
                        Status = "Delivered On Site",
                        TotalRequired = 3,
                        InstalledCount = 0,
                        Manufacturer = "Wiremold / Legrand",
                        ModelNumber = "25DTP-4",
                        FinishColor = "Anodized Aluminum",
                        CeilingMountType = "T-Bar Grid Clamp with Structural Tie-Wire",
                        ElectricalContractor = "Florida Power Techs",
                        CashWrapFeedDrops = 4,
                        ScheduledInstallDate = "2026-11-05"
                    },
                    Cabling = new CablingOperation
                    {
                        Vendor = "Granite Telecommunications",
                        LeadTechName = "Hector Ruiz",
                        Status = "Cable Pull In Progress",
                        CableGrade = "Cat6 Plenum (CMP)",
                        TotalDataDrops = 32,
                        PosDrops = 6,
                        ApDrops = 6,
                        BackOfficeDrops = 4,
                        CctvDrops = 14,
                        FlukeCertified = false,
                        ScheduledStartDate = "2026-11-06",
                        ScheduledCompletionDate = "2026-11-14"
                    },
                    Circuits = new List<TelecomCircuit>
                    {
                        new TelecomCircuit
                        {
                            CircuitId = 201,
                            Role = "Primary WAN",
                            CarrierName = "Comcast Business",
                            ServiceType = "Commercial Fiber DIA",
                            OrderStatus = "FOC Confirmed",
                            BandwidthDownMbps = 200,
                            BandwidthUpMbps = 200,
                            FocDate = "2026-11-12",
                            DemarcExtended = true
                        },
                        new TelecomCircuit
                        {
                            CircuitId = 202,
                            Role = "Secondary Failover WAN",
                            CarrierName = "AT&T Commercial",
                            ServiceType = "Enterprise 5G / LTE Wireless",
                            OrderStatus = "Tested & Turned Up (Active)",
                            BandwidthDownMbps = 100,
                            BandwidthUpMbps = 25,
                            TurnupDate = "2026-10-28",
                            DemarcExtended = true
                        }
                    },
                    WeeklyCompletionPct = 58,
                    WeeklyStatus = "In Progress / On Schedule",
                    WeeklySummaryNote = "Granite pulling Cat6 plenum drops. Secondary LTE active and verified."
                },
                new StoreDeployment
                {
                    StoreId = 14,
                    StoreNumber = "6012",
                    BannerCode = "R21",
                    BannerName = "rue21",
                    StoreName = "rue21",
                    ProjectType = "New Store Opening",
                    OperationalStatus = "IT Staging & Telecom",
                    OverallHealth = "On Track",
                    CountryCode = "US",
                    Region = "US Northeast & Mid-Atlantic",
                    District = "New York Metro & Northern NJ",
                    Address = "250 Woodbridge Ctr Spc 120",
                    City = "Woodbridge",
                    StateProvince = "NJ",
                    PostalCode = "07095",
                    GrossSqft = 6800,
                    SalesSqft = 5600,
                    PosTerminalCount = 4,
                    PosStagingStatus = "Shipped",
                    TargetOpenDate = "2026-11-15",
                    OpeningDate = "2026-11-15",
                    Mall = Malls["MALL-WBGE"],
                    Survey = new SiteSurvey
                    {
                        SurveyVendor = "Tri-State Telecom Surveyors",
                        SurveyorName = "Anthony Greco",
                        ScheduledDate = "2026-09-20",
                        ActualDate = "2026-09-21",
                        Status = "Completed & Approved",
                        CeilingType = "Drop Ceiling (2x4 Acoustic Tiles)",
                        CeilingHeightFeet = 13.5,
                        DemarcLocation = "Basement Telco Room 02",
                        DemarcConduitAvailable = true,
                        MdfRackLocation = "Cash Wrap Storage Closet",
                        MdfRackSpaceAdequate = true,
                        DedicatedCircuitsCount = 2
                    },
                    PowerPoles = new PowerPole
                    {
                        Status = "Passed Electrical Inspection",
                        TotalRequired = 2,
                        InstalledCount = 2,
                        Manufacturer = "Wiremold / Legrand",
                        ModelNumber = "25DTP-4",
                        FinishColor = "Matte Black",
                        CeilingMountType = "T-Bar Grid Clamp with Structural Tie-Wire",
                        ElectricalContractor = "Tri-County Electric",
                        CashWrapFeedDrops = 4,
                        ActualInstallDate = "2026-10-18"
                    },
                    Cabling = new CablingOperation
                    {
                        Vendor = "Direct Line Cabling",
                        LeadTechName = "David Vance",
                        Status = "Completed & Certified",
                        CableGrade = "Cat6 Plenum (CMP)",
                        TotalDataDrops = 24,
                        PosDrops = 4,
                        ApDrops = 4,
                        BackOfficeDrops = 4,
                        CctvDrops = 10,
                        FlukeCertified = true,
                        ScheduledStartDate = "2026-10-22",
                        ScheduledCompletionDate = "2026-10-26",
                        ActualCompletionDate = "2026-10-25"
                    },
                    Circuits = new List<TelecomCircuit>
                    {
                        new TelecomCircuit
                        {
                            CircuitId = 301,
                            Role = "Primary WAN",
                            CarrierName = "Comcast Business",
                            ServiceType = "Commercial Fiber DIA",
                            OrderStatus = "Tested & Turned Up (Active)",
                            BandwidthDownMbps = 200,
                            BandwidthUpMbps = 200,
                            TurnupDate = "2026-10-24",
                            DemarcExtended = true
                        },
                        new TelecomCircuit
                        {
                            CircuitId = 302,
                            Role = "Secondary Failover WAN",
                            CarrierName = "AT&T Commercial",
                            ServiceType = "Enterprise 5G / LTE Wireless",
                            OrderStatus = "Tested & Turned Up (Active)",
                            BandwidthDownMbps = 100,
                            BandwidthUpMbps = 25,
                            TurnupDate = "2026-10-24",
                            DemarcExtended = true
                        }
                    },
                    WeeklyCompletionPct = 92,
                    WeeklyStatus = "Completed & Certified",
                    WeeklySummaryNote = "Fluke cabling certified. Both Fiber and LTE turned up. POS hardware en route."
                },
                new StoreDeployment
                {
                    StoreId = 8,
                    StoreNumber = "6105",
                    BannerCode = "UP",
                    BannerName = "Urban Planet",
                    StoreName = "Urban Planet",
                    ProjectType = "New Store Opening",
                    OperationalStatus = "Site Survey",
                    OverallHealth = "On Track",
                    CountryCode = "CA",
                    Region = "Western Canada (AB, BC, SK, MB)",
                    District = "Greater Vancouver & Fraser",
                    Address = "4700 Kingsway Unit M12",
                    City = "Burnaby",
                    StateProvince = "BC",
                    PostalCode = "V5H 4M5",
                    GrossSqft = 19500,
                    SalesSqft = 16500,
                    PosTerminalCount = 8,
                    PosStagingStatus = "Pending",
                    TargetOpenDate = "2026-12-15",
                    OpeningDate = "2026-12-15",
                    Mall = Malls["MALL-METRO"],
                    Survey = new SiteSurvey
                    {
                        SurveyVendor = "West Coast Surveys",
                        SurveyorName = "Ken Tanaka",
                        ScheduledDate = "2026-10-15",
                        ActualDate = "2026-10-16",
                        Status = "Completed & Approved",
                        CeilingType = "Open Plenum / Exposed Deck",
                        CeilingHeightFeet = 18.0,
                        DemarcLocation = "Demarc Rm Lower Level Corridor D",
                        DemarcConduitAvailable = true,
                        MdfRackLocation = "Electrical Room 102",
                        MdfRackSpaceAdequate = true,
                        DedicatedCircuitsCount = 3
                    },
                    PowerPoles = new PowerPole
                    {
                        Status = "Equipment Ordered",
                        TotalRequired = 4,
                        InstalledCount = 0,
                        Manufacturer = "Wiremold / Legrand",
                        ModelNumber = "25DTP-4",
                        FinishColor = "Industrial Grey",
                        CeilingMountType = "Structural Beam Clamp (Open Plenum)",
                        CashWrapFeedDrops = 4,
                        ScheduledInstallDate = "2026-11-18"
                    },
                    Cabling = new CablingOperation
                    {
                        Vendor = "Pacific Data Systems",
                        Status = "Vendor Assigned",
                        CableGrade = "Cat6A Plenum (CMP)",
                        TotalDataDrops = 46,
                        PosDrops = 8,
                        ApDrops = 8,
                        BackOfficeDrops = 6,
                        CctvDrops = 20,
                        FlukeCertified = false,
                        ScheduledStartDate = "2026-11-20",
                        ScheduledCompletionDate = "2026-11-28"
                    },
                    Circuits = new List<TelecomCircuit>
                    {
                        new TelecomCircuit
                        {
                            CircuitId = 401,
                            Role = "Primary WAN",
                            CarrierName = "Telus",
                            ServiceType = "Commercial Fiber DIA",
                            OrderStatus = "Site Survey / Engineering",
                            BandwidthDownMbps = 250,
                            BandwidthUpMbps = 250,
                            FocDate = "2026-11-25",
                            DemarcExtended = false
                        }
                    },
                    WeeklyCompletionPct = 28,
                    WeeklyStatus = "Scheduled / Pending",
                    WeeklySummaryNote = "Site survey completed and approved. Open plenum mounts ordered for 18ft deck."
                },
                new StoreDeployment
                {
                    StoreId = 16,
                    StoreNumber = "6020",
                    BannerCode = "M",
                    BannerName = "Mandee",
                    StoreName = "Mandee",
                    ProjectType = "Minor Refresh",
                    OperationalStatus = "GC Construction",
                    OverallHealth = "Critical Blocker",
                    CountryCode = "US",
                    Region = "US Northeast & Mid-Atlantic",
                    District = "New York Metro & Northern NJ",
                    Address = "90-15 Queens Blvd Spc 115",
                    City = "Elmhurst",
                    StateProvince = "NY",
                    PostalCode = "11373",
                    GrossSqft = 6500,
                    SalesSqft = 5400,
                    PosTerminalCount = 4,
                    PosStagingStatus = "Pending",
                    TargetOpenDate = "2026-11-20",
                    OpeningDate = "2017-06-05",
                    Circuits = new List<TelecomCircuit>
                    {
                        new TelecomCircuit
                        {
                            CircuitId = 701,
                            Role = "Primary WAN",
                            CarrierName = "Charter Spectrum",
                            ServiceType = "Business Broadband Cable",
                            OrderStatus = "Carrier Delay / Jeopardy",
                            BandwidthDownMbps = 200,
                            BandwidthUpMbps = 20,
                            JeopardyReason = "Mall riser access denied due to missing security badging credential",
                            DemarcExtended = false
                        }
                    },
                    WeeklyCompletionPct = 35,
                    WeeklyStatus = "Blocked / In Jeopardy",
                    WeeklySummaryNote = "Carrier dispatch rejected by Queens Center mall security (badging required)."
                }
            };
        }

        public static WeeklyProgressReport GetWeeklyReport(int fiscalWeek)
        {
            if (fiscalWeek == 42)
            {
                return new WeeklyProgressReport
                {
                    FiscalYear = 2026,
                    FiscalWeek = 42,
                    WeekLabel = "Fiscal 2026 — Week 42",
                    DateRange = "Oct 18 – Oct 24, 2026",
                    TotalActiveProjects = 24,
                    CompletedThisWeek = 17,
                    InProgress = 5,
                    ScheduledPending = 1,
                    BlockedInJeopardy = 1,
                    OverallProgressPct = 84,
                    Milestones = new List<MilestoneProgress>
                    {
                        new() { Name = "Site Surveys", Completed = 24, Total = 24 },
                        new() { Name = "Wiremold Power Poles", Completed = 21, Total = 24 },
                        new() { Name = "Cat6 Cabling Certification", Completed = 20, Total = 24 },
                        new() { Name = "Dual-WAN Telecom Turn-up", Completed = 19, Total = 24 },
                        new() { Name = "POS Commissioning & QA", Completed = 16, Total = 24 }
                    },
                    Slices = new List<DonutSlice>
                    {
                        new() { Name = "Completed & Certified", Count = 17, Percentage = 71, ColorHex = "#10b981" },
                        new() { Name = "In Progress / On Schedule", Count = 5, Percentage = 21, ColorHex = "#0ea5e9" },
                        new() { Name = "Scheduled / Pending", Count = 1, Percentage = 4, ColorHex = "#f59e0b" },
                        new() { Name = "Blocked / In Jeopardy", Count = 1, Percentage = 4, ColorHex = "#f43f5e" }
                    }
                };
            }

            return new WeeklyProgressReport
            {
                FiscalYear = 2026,
                FiscalWeek = 41,
                WeekLabel = "Fiscal 2026 — Week 41",
                DateRange = "Oct 11 – Oct 17, 2026",
                TotalActiveProjects = 24,
                CompletedThisWeek = 13,
                InProgress = 7,
                ScheduledPending = 3,
                BlockedInJeopardy = 1,
                OverallProgressPct = 76,
                Milestones = new List<MilestoneProgress>
                {
                    new() { Name = "Site Surveys", Completed = 23, Total = 24 },
                    new() { Name = "Wiremold Power Poles", Completed = 18, Total = 24 },
                    new() { Name = "Cat6 Cabling Certification", Completed = 16, Total = 24 },
                    new() { Name = "Dual-WAN Telecom Turn-up", Completed = 14, Total = 24 },
                    new() { Name = "POS Commissioning & QA", Completed = 12, Total = 24 }
                },
                Slices = new List<DonutSlice>
                {
                    new() { Name = "Completed & Certified", Count = 13, Percentage = 54, ColorHex = "#10b981" },
                    new() { Name = "In Progress / On Schedule", Count = 7, Percentage = 29, ColorHex = "#0ea5e9" },
                    new() { Name = "Scheduled / Pending", Count = 3, Percentage = 13, ColorHex = "#f59e0b" },
                    new() { Name = "Blocked / In Jeopardy", Count = 1, Percentage = 4, ColorHex = "#f43f5e" }
                }
            };
        }
    }
}
