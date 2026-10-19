using System;
using System.Collections.Generic;
using System.Linq;
using YMInc.StoreDeploymentApp.Models;

namespace YMInc.StoreDeploymentApp.Services
{
    public static class DummyDataGenerator
    {
        public static DummyDatabaseState Generate200StoreFleet()
        {
            var malls = new List<MallProperty>();
            var stores = new List<StoreDeployment>();
            var workItems = new List<CalendarWorkItem>();

            int mallIdCounter = 501;
            int storeIdCounter = 901;
            int circuitIdCounter = 9001;
            int workItemSeq = 100;

            // Helper to register a mall
            MallProperty AddMall(
                string code, string name, string landlord, string propType,
                string address, string city, string state, string postal, string country,
                bool nightWork, string deliveryWindow, decimal badgingFee,
                string cablingVendor, string leadTech, string assignedMember,
                string specialRules, string circuitsReq)
            {
                var mall = new MallProperty
                {
                    MallId = mallIdCounter++,
                    MallCode = code,
                    PropertyName = name,
                    LandlordCompany = landlord,
                    PropertyType = propType,
                    Address = address,
                    City = city,
                    StateProvince = state,
                    PostalCode = postal,
                    CountryCode = country,
                    CoiRequired = true,
                    MinGeneralLiabilityCad = country == "CA" ? 10000000m : 5000000m,
                    NightWorkOnly = nightWork,
                    DeliveryWindow = deliveryWindow,
                    BadgingFeePerTech = badgingFee,
                    CablingVendor = cablingVendor,
                    LeadTechnicianName = leadTech,
                    MallContacted = true,
                    AssignedTeamMember = assignedMember,
                    ManagementContactName = $"{assignedMember.Split(' ')[0]}'s Landlord Rep",
                    ManagementPhone = country == "US" ? "(555) 234-5678" : "(416) 555-0199",
                    ManagementEmail = $"ops@{code.ToLowerInvariant()}.example.com",
                    SecurityPhone = country == "US" ? "(555) 234-9999" : "(416) 555-0100",
                    SpecialRules = specialRules,
                    RequirementsCircuits = circuitsReq,
                    IntermediateSteps = "COI filed and active for fiscal rollout."
                };
                malls.Add(mall);
                return mall;
            }

            // Helper to build a store
            void AddStore(
                string storeNum, string bannerCode, string bannerName,
                string storeName, string suite, MallProperty mall,
                string projectType, string opStatus, string health,
                int grossSqft, int salesSqft, int posCount, string posStatus,
                int completionPct, string weeklyStatus, string weeklyNote,
                int polesReq, int polesInst, string poleStatus,
                int drops, bool fluke, string cablingStatus,
                string primaryCarrier, string primaryType, int primarySpeed, string primaryStatus,
                string secCarrier, string secType, int secSpeed, string secStatus,
                int? activeTaskWeek = null, string? activeTaskType = null, string? activeTaskTitle = null, string? activeTaskStatus = null)
            {
                int storeId = storeIdCounter++;
                string country = mall.CountryCode;
                string region = country == "CA"
                    ? (mall.StateProvince == "ON" ? "Central Ontario" : (mall.StateProvince == "QC" ? "Quebec Metro" : "Western Canada"))
                    : (mall.StateProvince is "TX" or "FL" or "GA" or "NC" or "TN" ? "US South" :
                      (mall.StateProvince is "CA" or "WA" or "OR" or "AZ" or "CO" or "NV" ? "US West" :
                      (mall.StateProvince is "NY" or "PA" or "NJ" or "MA" or "VA" or "MD" ? "US Northeast" : "US Midwest")));

                string district = $"{region} — {mall.City} Metro";

                var circuits = new List<TelecomCircuit>
                {
                    new()
                    {
                        CircuitId = circuitIdCounter++,
                        StoreId = storeId,
                        Role = "Primary WAN",
                        CarrierName = primaryCarrier,
                        ServiceType = primaryType,
                        OrderStatus = primaryStatus,
                        BandwidthDownMbps = primarySpeed,
                        BandwidthUpMbps = primarySpeed,
                        FocDate = "2026-10-18",
                        TurnupDate = primaryStatus.Contains("Turned Up") ? "2026-10-19" : null,
                        DemarcExtended = true
                    },
                    new()
                    {
                        CircuitId = circuitIdCounter++,
                        StoreId = storeId,
                        Role = "Secondary Failover WAN",
                        CarrierName = secCarrier,
                        ServiceType = secType,
                        OrderStatus = secStatus,
                        BandwidthDownMbps = secSpeed,
                        BandwidthUpMbps = secSpeed / 3,
                        TurnupDate = secStatus.Contains("Turned Up") ? "2026-10-18" : null,
                        DemarcExtended = true
                    }
                };

                var store = new StoreDeployment
                {
                    StoreId = storeId,
                    StoreNumber = storeNum,
                    BannerCode = bannerCode,
                    BannerName = bannerName,
                    StoreName = $"{storeName} (Sandbox)",
                    ProjectType = projectType,
                    OperationalStatus = opStatus,
                    OverallHealth = health,
                    CountryCode = country,
                    Region = region,
                    District = district,
                    Address = $"{mall.Address} {suite}".Trim(),
                    City = mall.City,
                    StateProvince = mall.StateProvince,
                    PostalCode = mall.PostalCode,
                    GrossSqft = grossSqft,
                    SalesSqft = salesSqft,
                    PosTerminalCount = posCount,
                    PosStagingStatus = posStatus,
                    TargetOpenDate = "2026-11-15",
                    OpeningDate = opStatus.Contains("Open") ? "2026-10-01" : null,
                    PhoneNumber = country == "US" ? $"(555) 7{storeNum.Substring(Math.Max(0, storeNum.Length - 3))}-1000" : $"(416) 555-{storeNum.Substring(Math.Max(0, storeNum.Length - 4))}",
                    Mall = mall,
                    Survey = new SiteSurvey
                    {
                        StoreId = storeId,
                        SurveyVendor = country == "US" ? "Apex Field Techs" : "Ontario Field Ops",
                        SurveyorName = "Jordan Hayes",
                        ScheduledDate = "2026-09-25",
                        ActualDate = "2026-09-26",
                        Status = "Completed & Approved",
                        CeilingType = "Drop Ceiling (2x4 Acoustic)",
                        CeilingHeightFeet = 14.5,
                        DemarcLocation = "Building Telco MPOE",
                        DemarcConduitAvailable = true,
                        MdfRackLocation = "Back Office IDF",
                        StructureNotes = "Pathways and rack cleared."
                    },
                    PowerPoles = new PowerPole
                    {
                        StoreId = storeId,
                        Status = poleStatus,
                        TotalRequired = polesReq,
                        InstalledCount = polesInst,
                        FinishColor = "Anodized Aluminum",
                        CeilingMountType = "T-Bar Grid Clamp",
                        ElectricalContractor = country == "US" ? "Apex Commercial Electric" : "Maple Leaf Power",
                        CashWrapFeedDrops = polesReq * 2,
                        ScheduledInstallDate = "2026-10-14",
                        ActualInstallDate = polesInst == polesReq ? "2026-10-15" : null
                    },
                    Cabling = new CablingOperation
                    {
                        StoreId = storeId,
                        Vendor = mall.CablingVendor,
                        LeadTechName = mall.LeadTechnicianName,
                        Status = cablingStatus,
                        TotalDataDrops = drops,
                        PosDrops = posCount,
                        ApDrops = 4,
                        FlukeCertified = fluke,
                        ScheduledStartDate = "2026-10-12",
                        ScheduledCompletionDate = "2026-10-16",
                        ActualCompletionDate = fluke ? "2026-10-16" : null
                    },
                    Circuits = circuits,
                    WeeklyCompletionPct = completionPct,
                    WeeklyStatus = weeklyStatus,
                    WeeklySummaryNote = weeklyNote
                };

                stores.Add(store);

                // Add calendar work item if specified
                if (activeTaskWeek.HasValue && !string.IsNullOrEmpty(activeTaskType))
                {
                    var (ws, _) = DummyProjectService.GetDateRangeForFiscalWeek(activeTaskWeek.Value);
                    DateTime taskDate = ws.AddDays(Math.Abs(storeId % 6));

                    workItems.Add(new CalendarWorkItem
                    {
                        WorkItemId = $"SBX-{activeTaskWeek.Value}{workItemSeq++}",
                        ActivityDate = taskDate,
                        FiscalYear = 2026,
                        FiscalWeek = activeTaskWeek.Value,
                        ActivityType = activeTaskType,
                        Title = activeTaskTitle ?? $"{activeTaskType} — {store.StoreName}",
                        StoreNumber = store.StoreNumber,
                        StoreName = store.StoreName,
                        BannerCode = store.BannerCode,
                        Status = activeTaskStatus ?? "Scheduled",
                        AssignedTo = mall.AssignedTeamMember,
                        TimeWindow = "08:00 - 12:00",
                        Notes = $"Rollout execution task for Store #{store.StoreNumber} ({store.BannerName})."
                    });
                }
            }

            // =========================================================================
            // PART 1: PREMIER CO-LOCATED MALLS (Multiple YM Inc branches in the same mall)
            // e.g. Rue21 and Charlotte Russe sharing the same mall property
            // =========================================================================

            // 1. King of Prussia Mall (PA) — CR + R21
            var mKop = AddMall("MALL-PA01", "King of Prussia Mall (Sandbox)", "Simon Property Group", "Enclosed Super-Regional Mall",
                "160 N Gulph Rd", "King of Prussia", "PA", "19406", "US", true, "06:00 - 09:00", 25m,
                "Mid-Atlantic Telecom", "Patrick Sullivan", "Marcus Vance (PM)",
                "Night work only after 21:30. Union electrical mandatory.", "Sub-basement Demarc B1.");
            AddStore("9001", "CR", "Charlotte Russe", "King of Prussia Russe", "Ste 120", mKop, "New Store Opening", "Grand Open", "Complete", 7200, 6000, 5, "Commissioned", 100, "Completed & Certified", "Opening completed Week 40.", 2, 2, "Fully Installed & Certified", 26, true, "Fluke Certified & Closed", "Comcast Business", "Commercial Fiber DIA", 300, "Tested & Turned Up (Active)", "Verizon 5G", "Enterprise 5G Wireless", 100, "Tested & Turned Up (Active)");
            AddStore("9002", "R21", "rue21", "King of Prussia rue21", "Ste 215", mKop, "New Store Opening", "IT Staging & Telecom", "On Track", 6500, 5300, 4, "Commissioned", 95, "Completed & Certified", "Dual-WAN certified in Week 42.", 2, 2, "Fully Installed & Certified", 24, true, "Fluke Certified & Closed", "Comcast Business", "Commercial Fiber DIA", 300, "Tested & Turned Up (Active)", "AT&T 5G", "Enterprise 5G Wireless", 100, "Tested & Turned Up (Active)", 42, "TTU (Test & Turn-Up)", "Comcast Fiber DIA & AT&T 5G Dual TTU", "Completed");

            // 2. Mall of America (MN) — CR + R21 + UP
            var mMoa = AddMall("MALL-MN01", "Mall of America (Sandbox)", "Triple Five Group", "Enclosed Mega-Mall",
                "60 E Broadway", "Bloomington", "MN", "55425", "US", true, "05:00 - 08:30", 30m,
                "NorthStar Low-Volt", "Erik Lindqvist", "Danielle Levy (PS2)",
                "Strict badging. Riser room key requires 24h notice.", "Riser tray West Tower 3.");
            AddStore("9003", "CR", "Charlotte Russe", "Mall of America Russe", "Ste W310", mMoa, "Active Fleet", "Grand Open", "Complete", 8400, 7100, 6, "Commissioned", 100, "Completed & Certified", "Fleet operational.", 3, 3, "Fully Installed & Certified", 32, true, "Fluke Certified & Closed", "CenturyLink Fiber", "Commercial Fiber DIA", 500, "Tested & Turned Up (Active)", "Verizon 5G", "Enterprise 5G Wireless", 150, "Tested & Turned Up (Active)");
            AddStore("9004", "R21", "rue21", "Mall of America rue21", "Ste S240", mMoa, "Active Fleet", "Grand Open", "Complete", 7600, 6400, 5, "Commissioned", 100, "Completed & Certified", "Fleet operational.", 2, 2, "Fully Installed & Certified", 28, true, "Fluke Certified & Closed", "CenturyLink Fiber", "Commercial Fiber DIA", 300, "Tested & Turned Up (Active)", "T-Mobile 5G", "Enterprise 5G Wireless", 100, "Tested & Turned Up (Active)");
            AddStore("9005", "UP", "Urban Planet", "Mall of America Urban Planet", "Ste E112", mMoa, "Major Remodel", "Low-Voltage & Power Poles", "On Track", 14500, 12200, 8, "Hardware Staged", 80, "In Progress / On Schedule", "Poles installed; cabling underway Week 42.", 4, 4, "Fully Installed & Certified", 44, false, "Pathway & J-Hooks Installed", "CenturyLink Fiber", "Commercial Fiber DIA", 500, "FOC Confirmed", "Verizon 5G", "Enterprise 5G Wireless", 150, "Tested & Turned Up (Active)", 42, "Low-Voltage Cabling", "Cat6 44-Drop Pull & Fluke Testing", "In Progress");

            // 3. Sawgrass Mills (FL) — CR + R21
            var mSawgrass = AddMall("MALL-FL01", "Sawgrass Mills (Sandbox)", "Simon Property Group", "Enclosed Outlet Mall",
                "12801 W Sunrise Blvd", "Sunrise", "FL", "33323", "US", false, "06:00 - 10:00", 20m,
                "Sunbelt Data Systems", "Diego Morales", "Sarah Jenkins (PS4)",
                "Loading dock gate 4 only. No scaffolding during holiday peak.", "Exterior conduit B-14.");
            AddStore("9006", "CR", "Charlotte Russe", "Sawgrass Mills Russe", "Ste 402", mSawgrass, "New Store Opening", "IT Staging & Telecom", "On Track", 7900, 6600, 5, "Commissioned", 90, "Completed & Certified", "POS QA verified.", 2, 2, "Fully Installed & Certified", 26, true, "Fluke Certified & Closed", "AT&T Fiber", "Commercial Fiber DIA", 300, "Tested & Turned Up (Active)", "Verizon 5G", "Enterprise 5G Wireless", 100, "Tested & Turned Up (Active)");
            AddStore("9007", "R21", "rue21", "Sawgrass Mills rue21", "Ste 810", mSawgrass, "New Store Opening", "Low-Voltage & Power Poles", "On Track", 7100, 5900, 4, "Hardware Staged", 75, "In Progress / On Schedule", "Cabling termination Week 42.", 2, 2, "Fully Installed & Certified", 24, false, "Pathway & J-Hooks Installed", "AT&T Fiber", "Commercial Fiber DIA", 300, "FOC Confirmed", "T-Mobile 5G", "Enterprise 5G Wireless", 100, "Tested & Turned Up (Active)", 42, "Circuit Install", "AT&T Fiber Demarc Extension & ONT Mount", "In Progress");

            // 4. The Houston Galleria (TX) — CR + R21
            var mGalleriaTx = AddMall("MALL-TX01", "The Houston Galleria (Sandbox)", "Simon Property Group", "Enclosed Super-Regional Mall",
                "5085 Westheimer Rd", "Houston", "TX", "77056", "US", true, "05:30 - 08:30", 25m,
                "LoneStar Network Techs", "Carlos Mendez", "Marcus Vance (PM)",
                "Night work after 22:00. Scissor lift requires floor protection mats.", "IDF-3 North Galleria.");
            AddStore("9008", "CR", "Charlotte Russe", "Houston Galleria Russe", "Ste 2100", mGalleriaTx, "Active Fleet", "Grand Open", "Complete", 8100, 6800, 5, "Commissioned", 100, "Completed & Certified", "Active.", 2, 2, "Fully Installed & Certified", 28, true, "Fluke Certified & Closed", "AT&T Fiber", "Commercial Fiber DIA", 300, "Tested & Turned Up (Active)", "Verizon 5G", "Enterprise 5G Wireless", 100, "Tested & Turned Up (Active)");
            AddStore("9009", "R21", "rue21", "Houston Galleria rue21", "Ste 3300", mGalleriaTx, "Active Fleet", "Grand Open", "Complete", 7200, 6000, 4, "Commissioned", 100, "Completed & Certified", "Active.", 2, 2, "Fully Installed & Certified", 24, true, "Fluke Certified & Closed", "AT&T Fiber", "Commercial Fiber DIA", 300, "Tested & Turned Up (Active)", "AT&T 5G", "Enterprise 5G Wireless", 100, "Tested & Turned Up (Active)");

            // 5. Woodfield Mall (IL) — CR + R21
            var mWoodfield = AddMall("MALL-IL01", "Woodfield Mall (Sandbox)", "Simon Property Group", "Enclosed Super-Regional Mall",
                "5 Woodfield Mall", "Schaumburg", "IL", "60173", "US", true, "06:00 - 09:00", 20m,
                "Midwest Structured Net", "Dave Kowalski", "Claire Tremblay (PA)",
                "Security escort mandatory for all electrical room entries.", "Central Demarc Shaft 2.");
            AddStore("9010", "CR", "Charlotte Russe", "Woodfield Mall Russe", "Ste E105", mWoodfield, "Active Fleet", "Grand Open", "Complete", 8500, 7100, 6, "Commissioned", 100, "Completed & Certified", "Operational.", 3, 3, "Fully Installed & Certified", 30, true, "Fluke Certified & Closed", "Comcast Business", "Commercial Fiber DIA", 300, "Tested & Turned Up (Active)", "Verizon 5G", "Enterprise 5G Wireless", 100, "Tested & Turned Up (Active)");
            AddStore("9011", "R21", "rue21", "Woodfield Mall rue21", "Ste J210", mWoodfield, "Major Remodel", "Site Survey", "On Track", 7400, 6100, 4, "Pending", 40, "Scheduled / Pending", "Survey approved; awaiting permit.", 2, 0, "Delivered On Site", 26, false, "Vendor Assigned", "Comcast Business", "Commercial Fiber DIA", 300, "Order Placed", "T-Mobile 5G", "Enterprise 5G Wireless", 100, "Order Placed", 42, "Site Survey", "MDF Conduit & Pathway Verification Survey", "Completed");

            // 6. Aventura Mall (FL) — M + CR
            var mAventura = AddMall("MALL-FL02", "Aventura Mall (Sandbox)", "Turnberry Associates", "Enclosed Luxury Mall",
                "19501 Biscayne Blvd", "Aventura", "FL", "33180", "US", true, "05:00 - 08:30", 35m,
                "Sunbelt Data Systems", "Diego Morales", "Sarah Jenkins (PS4)",
                "Strict finish guidelines. All conduit concealed.", "South Telco Core B.");
            AddStore("9012", "M", "Mandee", "Aventura Mall Mandee", "Ste 140", mAventura, "Active Fleet", "Grand Open", "Complete", 6900, 5700, 4, "Commissioned", 100, "Completed & Certified", "Active fleet store.", 2, 2, "Fully Installed & Certified", 24, true, "Fluke Certified & Closed", "AT&T Fiber", "Commercial Fiber DIA", 300, "Tested & Turned Up (Active)", "Verizon 5G", "Enterprise 5G Wireless", 100, "Tested & Turned Up (Active)");
            AddStore("9013", "CR", "Charlotte Russe", "Aventura Mall Russe", "Ste 210", mAventura, "New Store Opening", "IT Staging & Telecom", "On Track", 7800, 6500, 5, "Commissioned", 95, "Completed & Certified", "POS QA verified in Week 42.", 2, 2, "Fully Installed & Certified", 28, true, "Fluke Certified & Closed", "AT&T Fiber", "Commercial Fiber DIA", 300, "Tested & Turned Up (Active)", "AT&T 5G", "Enterprise 5G Wireless", 100, "Tested & Turned Up (Active)", 42, "POS Staging & Cutover", "POS Registers Commissioning & Peripheral QA", "Completed");

            // 7. South Coast Plaza (CA) — CR + R21
            var mSouthCoast = AddMall("MALL-CA01", "South Coast Plaza (Sandbox)", "Segerstrom & Sons", "Enclosed Luxury Center",
                "3333 Bristol St", "Costa Mesa", "CA", "92626", "US", true, "05:30 - 08:30", 30m,
                "Pacific Shore Cabling", "Brandon Lee", "Terry O'Connor (PS1)",
                "White gloves on finish fixtures. High acoustic ceiling.", "Level 2 IDF West.");
            AddStore("9014", "CR", "Charlotte Russe", "South Coast Plaza Russe", "Ste 310", mSouthCoast, "Active Fleet", "Grand Open", "Complete", 8300, 6900, 5, "Commissioned", 100, "Completed & Certified", "Active.", 2, 2, "Fully Installed & Certified", 28, true, "Fluke Certified & Closed", "Spectrum Enterprise", "Commercial Fiber DIA", 300, "Tested & Turned Up (Active)", "Verizon 5G", "Enterprise 5G Wireless", 100, "Tested & Turned Up (Active)");
            AddStore("9015", "R21", "rue21", "South Coast Plaza rue21", "Ste 125", mSouthCoast, "Active Fleet", "Grand Open", "Complete", 7100, 5900, 4, "Commissioned", 100, "Completed & Certified", "Active.", 2, 2, "Fully Installed & Certified", 24, true, "Fluke Certified & Closed", "Spectrum Enterprise", "Commercial Fiber DIA", 300, "Tested & Turned Up (Active)", "T-Mobile 5G", "Enterprise 5G Wireless", 100, "Tested & Turned Up (Active)");

            // 8. Ontario Mills (CA) — CR + R21 + UP
            var mOntarioMills = AddMall("MALL-CA02", "Ontario Mills (Sandbox)", "Simon Property Group", "Enclosed Outlet Mall",
                "1 Mills Circle", "Ontario", "CA", "91764", "US", false, "06:00 - 10:00", 20m,
                "SoCal Cableworks", "Hector Ramirez", "Terry O'Connor (PS1)",
                "Loading bay 7. Fast-track landlord permits.", "Demarc Node G.");
            AddStore("9016", "CR", "Charlotte Russe", "Ontario Mills Russe", "Ste 502", mOntarioMills, "Active Fleet", "Grand Open", "Complete", 9100, 7700, 6, "Commissioned", 100, "Completed & Certified", "Active.", 3, 3, "Fully Installed & Certified", 32, true, "Fluke Certified & Closed", "Frontier Fiber", "Commercial Fiber DIA", 500, "Tested & Turned Up (Active)", "Verizon 5G", "Enterprise 5G Wireless", 100, "Tested & Turned Up (Active)");
            AddStore("9017", "R21", "rue21", "Ontario Mills rue21", "Ste 714", mOntarioMills, "Active Fleet", "Grand Open", "Complete", 7800, 6500, 5, "Commissioned", 100, "Completed & Certified", "Active.", 2, 2, "Fully Installed & Certified", 26, true, "Fluke Certified & Closed", "Frontier Fiber", "Commercial Fiber DIA", 300, "Tested & Turned Up (Active)", "AT&T 5G", "Enterprise 5G Wireless", 100, "Tested & Turned Up (Active)");
            AddStore("9018", "UP", "Urban Planet", "Ontario Mills Urban Planet", "Ste 320", mOntarioMills, "New Store Opening", "Low-Voltage & Power Poles", "Critical Blocker", 15200, 12800, 8, "Pending", 48, "Blocked / In Jeopardy", "Landlord riser conduit obstructed.", 4, 1, "Blocked / On Hold", 48, false, "Blocked", "Frontier Fiber", "Commercial Fiber DIA", 500, "Carrier Delay / Jeopardy", "Verizon 5G", "Enterprise 5G Wireless", 150, "Order Placed", 42, "Power Pole Install", "3x Wiremold Poles Night Shift Installation", "Blocked / Delayed");

            // 9. Roosevelt Field (NY) — CR + M
            var mRoosevelt = AddMall("MALL-NY01", "Roosevelt Field (Sandbox)", "Simon Property Group", "Enclosed Super-Regional Mall",
                "630 Old Country Rd", "Garden City", "NY", "11530", "US", true, "05:00 - 08:30", 30m,
                "Empire State Cable", "Anthony Ricci", "Claire Tremblay (PA)",
                "Nassau County electrical permits required. No open flames.", "Basement Demarc Core 4.");
            AddStore("9019", "CR", "Charlotte Russe", "Roosevelt Field Russe", "Ste 108", mRoosevelt, "Active Fleet", "Grand Open", "Complete", 8600, 7200, 6, "Commissioned", 100, "Completed & Certified", "Active.", 3, 3, "Fully Installed & Certified", 30, true, "Fluke Certified & Closed", "Verizon Fios", "Commercial Fiber DIA", 500, "Tested & Turned Up (Active)", "AT&T 5G", "Enterprise 5G Wireless", 100, "Tested & Turned Up (Active)");
            AddStore("9020", "M", "Mandee", "Roosevelt Field Mandee", "Ste 245", mRoosevelt, "Major Remodel", "IT Staging & Telecom", "On Track", 7200, 5900, 4, "Commissioned", 90, "Completed & Certified", "Cutover scheduled Week 42.", 2, 2, "Fully Installed & Certified", 24, true, "Fluke Certified & Closed", "Verizon Fios", "Commercial Fiber DIA", 300, "Tested & Turned Up (Active)", "T-Mobile 5G", "Enterprise 5G Wireless", 100, "Tested & Turned Up (Active)", 42, "POS Staging & Cutover", "Mandee POS Hardware Cutover & Verification", "Scheduled");

            // 10. Destiny USA (NY) — CR + R21
            var mDestiny = AddMall("MALL-NY02", "Destiny USA (Sandbox)", "Pyramid Management Group", "Enclosed Super-Regional Mall",
                "9090 Destiny USA Dr", "Syracuse", "NY", "13204", "US", false, "06:00 - 10:00", 20m,
                "Upstate Net Solutions", "Gary Miller", "Claire Tremblay (PA)",
                "Core drilling requires radar scan. 24h notice to security.", "3rd Floor Telco Riser.");
            AddStore("9021", "CR", "Charlotte Russe", "Destiny USA Russe", "Ste 204", mDestiny, "Active Fleet", "Grand Open", "Complete", 8000, 6700, 5, "Commissioned", 100, "Completed & Certified", "Active.", 2, 2, "Fully Installed & Certified", 26, true, "Fluke Certified & Closed", "Spectrum Enterprise", "Commercial Fiber DIA", 300, "Tested & Turned Up (Active)", "Verizon 5G", "Enterprise 5G Wireless", 100, "Tested & Turned Up (Active)");
            AddStore("9022", "R21", "rue21", "Destiny USA rue21", "Ste 408", mDestiny, "New Store Opening", "Low-Voltage & Power Poles", "On Track", 7300, 6100, 4, "Hardware Staged", 70, "In Progress / On Schedule", "Cabling pull Week 42.", 2, 2, "Fully Installed & Certified", 24, false, "Pathway & J-Hooks Installed", "Spectrum Enterprise", "Commercial Fiber DIA", 300, "FOC Confirmed", "AT&T 5G", "Enterprise 5G Wireless", 100, "Tested & Turned Up (Active)", 42, "Low-Voltage Cabling", "Cat6 24-Drop Pull & Termination", "In Progress");

            // 11. Grapevine Mills (TX) — CR + R21
            var mGrapevine = AddMall("MALL-TX02", "Grapevine Mills (Sandbox)", "Simon Property Group", "Enclosed Outlet Mall",
                "3000 Grapevine Mills Pkwy", "Grapevine", "TX", "76051", "US", false, "06:00 - 09:30", 20m,
                "Velocity Low-Voltage", "Carlos Mendez", "Marcus Vance (PM)",
                "Loading Dock 3. Standard Simon rules.", "Exterior Telco Demarc.");
            AddStore("9023", "CR", "Charlotte Russe", "Grapevine Mills Russe", "Ste 310", mGrapevine, "Active Fleet", "Grand Open", "Complete", 8700, 7300, 6, "Commissioned", 100, "Completed & Certified", "Active.", 3, 3, "Fully Installed & Certified", 30, true, "Fluke Certified & Closed", "AT&T Fiber", "Commercial Fiber DIA", 300, "Tested & Turned Up (Active)", "Verizon 5G", "Enterprise 5G Wireless", 100, "Tested & Turned Up (Active)");
            AddStore("9024", "R21", "rue21", "Grapevine Mills rue21", "Ste 622", mGrapevine, "Active Fleet", "Grand Open", "Complete", 7600, 6300, 5, "Commissioned", 100, "Completed & Certified", "Active.", 2, 2, "Fully Installed & Certified", 26, true, "Fluke Certified & Closed", "AT&T Fiber", "Commercial Fiber DIA", 300, "Tested & Turned Up (Active)", "T-Mobile 5G", "Enterprise 5G Wireless", 100, "Tested & Turned Up (Active)");

            // 12. The Mall at Millenia (FL) — CR + R21
            var mMillenia = AddMall("MALL-FL03", "The Mall at Millenia (Sandbox)", "The Forbes Company", "Enclosed Luxury Mall",
                "4200 Conroy Rd", "Orlando", "FL", "32839", "US", true, "05:30 - 08:30", 30m,
                "Sunbelt Data Systems", "Diego Morales", "Sarah Jenkins (PS4)",
                "Strict landlord aesthetics. High-end drop tiles.", "IDF South Concourse.");
            AddStore("9025", "CR", "Charlotte Russe", "Millenia Orlando Russe", "Ste 114", mMillenia, "Active Fleet", "Grand Open", "Complete", 8100, 6800, 5, "Commissioned", 100, "Completed & Certified", "Active.", 2, 2, "Fully Installed & Certified", 28, true, "Fluke Certified & Closed", "AT&T Fiber", "Commercial Fiber DIA", 300, "Tested & Turned Up (Active)", "Verizon 5G", "Enterprise 5G Wireless", 100, "Tested & Turned Up (Active)");
            AddStore("9026", "R21", "rue21", "Millenia Orlando rue21", "Ste 220", mMillenia, "Active Fleet", "Grand Open", "Complete", 7200, 6000, 4, "Commissioned", 100, "Completed & Certified", "Active.", 2, 2, "Fully Installed & Certified", 24, true, "Fluke Certified & Closed", "AT&T Fiber", "Commercial Fiber DIA", 300, "Tested & Turned Up (Active)", "AT&T 5G", "Enterprise 5G Wireless", 100, "Tested & Turned Up (Active)");

            // 13. Arundel Mills (MD) — CR + R21
            var mArundel = AddMall("MALL-MD01", "Arundel Mills (Sandbox)", "Simon Property Group", "Enclosed Outlet Mall",
                "7000 Arundel Mills Cir", "Hanover", "MD", "21076", "US", false, "06:00 - 09:30", 20m,
                "Chesapeake Cabling", "Jason Myers", "Claire Tremblay (PA)",
                "Loading dock 2. Standard Anne Arundel county inspections.", "Exterior MPOE Cabinet.");
            AddStore("9027", "CR", "Charlotte Russe", "Arundel Mills Russe", "Ste 412", mArundel, "Active Fleet", "Grand Open", "Complete", 8200, 6900, 5, "Commissioned", 100, "Completed & Certified", "Active.", 2, 2, "Fully Installed & Certified", 28, true, "Fluke Certified & Closed", "Comcast Business", "Commercial Fiber DIA", 300, "Tested & Turned Up (Active)", "Verizon 5G", "Enterprise 5G Wireless", 100, "Tested & Turned Up (Active)");
            AddStore("9028", "R21", "rue21", "Arundel Mills rue21", "Ste 814", mArundel, "Active Fleet", "Grand Open", "Complete", 7400, 6100, 4, "Commissioned", 100, "Completed & Certified", "Active.", 2, 2, "Fully Installed & Certified", 24, true, "Fluke Certified & Closed", "Comcast Business", "Commercial Fiber DIA", 300, "Tested & Turned Up (Active)", "AT&T 5G", "Enterprise 5G Wireless", 100, "Tested & Turned Up (Active)");

            // 14. Polaris Fashion Place (OH) — CR + R21
            var mPolaris = AddMall("MALL-OH01", "Polaris Fashion Place (Sandbox)", "Washington Prime Group", "Enclosed Regional Mall",
                "1500 Polaris Pkwy", "Columbus", "OH", "43240", "US", true, "06:00 - 09:00", 20m,
                "Buckeye Telecom Systems", "Trevor Vance", "Danielle Levy (PS2)",
                "Night work after 21:00. Union cabling only.", "Basement Demarc 1.");
            AddStore("9029", "CR", "Charlotte Russe", "Polaris Columbus Russe", "Ste 118", mPolaris, "Active Fleet", "Grand Open", "Complete", 7900, 6600, 5, "Commissioned", 100, "Completed & Certified", "Active.", 2, 2, "Fully Installed & Certified", 26, true, "Fluke Certified & Closed", "Spectrum Enterprise", "Commercial Fiber DIA", 300, "Tested & Turned Up (Active)", "Verizon 5G", "Enterprise 5G Wireless", 100, "Tested & Turned Up (Active)");
            AddStore("9030", "R21", "rue21", "Polaris Columbus rue21", "Ste 234", mPolaris, "New Store Opening", "IT Staging & Telecom", "On Track", 7100, 5900, 4, "Commissioned", 92, "Completed & Certified", "TTU signed off Oct 19.", 2, 2, "Fully Installed & Certified", 24, true, "Fluke Certified & Closed", "Spectrum Enterprise", "Commercial Fiber DIA", 300, "Tested & Turned Up (Active)", "AT&T 5G", "Enterprise 5G Wireless", 100, "Tested & Turned Up (Active)", 42, "TTU (Test & Turn-Up)", "Spectrum Fiber 300M DIA Test & Turn-Up", "Completed");

            // 15. Kenwood Towne Centre (OH) — CR + R21
            var mKenwood = AddMall("MALL-OH02", "Kenwood Towne Centre (Sandbox)", "Brookfield Properties", "Enclosed Regional Mall",
                "7875 Montgomery Rd", "Cincinnati", "OH", "45236", "US", true, "05:30 - 08:30", 25m,
                "Buckeye Telecom Systems", "Trevor Vance", "Danielle Levy (PS2)",
                "Hamilton County permits required. No drilling during trading hours.", "Core Telco Shaft.");
            AddStore("9031", "CR", "Charlotte Russe", "Kenwood Cincinnati Russe", "Ste 240", mKenwood, "Active Fleet", "Grand Open", "Complete", 8300, 6900, 5, "Commissioned", 100, "Completed & Certified", "Active.", 2, 2, "Fully Installed & Certified", 28, true, "Fluke Certified & Closed", "Altafiber Enterprise", "Commercial Fiber DIA", 400, "Tested & Turned Up (Active)", "Verizon 5G", "Enterprise 5G Wireless", 100, "Tested & Turned Up (Active)");
            AddStore("9032", "R21", "rue21", "Kenwood Cincinnati rue21", "Ste 110", mKenwood, "Active Fleet", "Grand Open", "Complete", 7200, 6000, 4, "Commissioned", 100, "Completed & Certified", "Active.", 2, 2, "Fully Installed & Certified", 24, true, "Fluke Certified & Closed", "Altafiber Enterprise", "Commercial Fiber DIA", 300, "Tested & Turned Up (Active)", "T-Mobile 5G", "Enterprise 5G Wireless", 100, "Tested & Turned Up (Active)");

            // 16. Great Lakes Crossing (MI) — CR + R21 + UP
            var mGreatLakes = AddMall("MALL-MI01", "Great Lakes Crossing (Sandbox)", "Taubman Centers", "Enclosed Outlet Mega-Mall",
                "4000 Baldwin Rd", "Auburn Hills", "MI", "48326", "US", false, "06:00 - 09:30", 20m,
                "MotorCity Structured Nets", "Kurt Wagner", "Danielle Levy (PS2)",
                "Loading Bay 5. Fast turnaround required.", "District 4 Telco Node.");
            AddStore("9033", "CR", "Charlotte Russe", "Great Lakes Crossing Russe", "Ste 314", mGreatLakes, "Active Fleet", "Grand Open", "Complete", 8800, 7400, 6, "Commissioned", 100, "Completed & Certified", "Active.", 3, 3, "Fully Installed & Certified", 30, true, "Fluke Certified & Closed", "Comcast Business", "Commercial Fiber DIA", 300, "Tested & Turned Up (Active)", "Verizon 5G", "Enterprise 5G Wireless", 100, "Tested & Turned Up (Active)");
            AddStore("9034", "R21", "rue21", "Great Lakes Crossing rue21", "Ste 512", mGreatLakes, "Active Fleet", "Grand Open", "Complete", 7900, 6600, 5, "Commissioned", 100, "Completed & Certified", "Active.", 2, 2, "Fully Installed & Certified", 26, true, "Fluke Certified & Closed", "Comcast Business", "Commercial Fiber DIA", 300, "Tested & Turned Up (Active)", "AT&T 5G", "Enterprise 5G Wireless", 100, "Tested & Turned Up (Active)");
            AddStore("9035", "UP", "Urban Planet", "Great Lakes Crossing Urban Planet", "Ste 800", mGreatLakes, "Major Remodel", "IT Staging & Telecom", "On Track", 16000, 13500, 8, "Commissioned", 94, "Completed & Certified", "Dual TTU completed Week 42.", 4, 4, "Fully Installed & Certified", 48, true, "Fluke Certified & Closed", "Comcast Business", "Commercial Fiber DIA", 500, "Tested & Turned Up (Active)", "Verizon 5G", "Enterprise 5G Wireless", 150, "Tested & Turned Up (Active)", 42, "TTU (Test & Turn-Up)", "Comcast 500M DIA & Verizon 5G Enterprise TTU", "Completed");

            // 17. Potomac Mills (VA) — CR + R21
            var mPotomac = AddMall("MALL-VA01", "Potomac Mills (Sandbox)", "Simon Property Group", "Enclosed Outlet Mall",
                "2700 Potomac Mills Cir", "Woodbridge", "VA", "22192", "US", false, "06:00 - 09:30", 20m,
                "Capital Structured Cabling", "Marcus Ward", "Claire Tremblay (PA)",
                "Security clearance required for Prince William County.", "MPOE Room B.");
            AddStore("9036", "CR", "Charlotte Russe", "Potomac Mills Russe", "Ste 210", mPotomac, "Active Fleet", "Grand Open", "Complete", 8500, 7100, 6, "Commissioned", 100, "Completed & Certified", "Active.", 3, 3, "Fully Installed & Certified", 30, true, "Fluke Certified & Closed", "Verizon Fios", "Commercial Fiber DIA", 400, "Tested & Turned Up (Active)", "AT&T 5G", "Enterprise 5G Wireless", 100, "Tested & Turned Up (Active)");
            AddStore("9037", "R21", "rue21", "Potomac Mills rue21", "Ste 420", mPotomac, "Active Fleet", "Grand Open", "Complete", 7500, 6200, 4, "Commissioned", 100, "Completed & Certified", "Active.", 2, 2, "Fully Installed & Certified", 24, true, "Fluke Certified & Closed", "Verizon Fios", "Commercial Fiber DIA", 300, "Tested & Turned Up (Active)", "Verizon 5G", "Enterprise 5G Wireless", 100, "Tested & Turned Up (Active)");

            // 18. Gurnee Mills (IL) — CR + R21
            var mGurnee = AddMall("MALL-IL02", "Gurnee Mills (Sandbox)", "Simon Property Group", "Enclosed Super-Regional Outlet",
                "6170 W Grand Ave", "Gurnee", "IL", "60031", "US", false, "06:00 - 10:00", 20m,
                "Midwest Structured Net", "Dave Kowalski", "Danielle Levy (PS2)",
                "Loading Dock 6. Standard Simon operating agreement.", "Wing E Demarc.");
            AddStore("9038", "CR", "Charlotte Russe", "Gurnee Mills Russe", "Ste 114", mGurnee, "Active Fleet", "Grand Open", "Complete", 8100, 6800, 5, "Commissioned", 100, "Completed & Certified", "Active.", 2, 2, "Fully Installed & Certified", 28, true, "Fluke Certified & Closed", "Comcast Business", "Commercial Fiber DIA", 300, "Tested & Turned Up (Active)", "Verizon 5G", "Enterprise 5G Wireless", 100, "Tested & Turned Up (Active)");
            AddStore("9039", "R21", "rue21", "Gurnee Mills rue21", "Ste 318", mGurnee, "Active Fleet", "Grand Open", "Complete", 7300, 6100, 4, "Commissioned", 100, "Completed & Certified", "Active.", 2, 2, "Fully Installed & Certified", 24, true, "Fluke Certified & Closed", "Comcast Business", "Commercial Fiber DIA", 300, "Tested & Turned Up (Active)", "AT&T 5G", "Enterprise 5G Wireless", 100, "Tested & Turned Up (Active)");

            // 19. Dolphin Mall (FL) — CR + R21
            var mDolphin = AddMall("MALL-FL04", "Dolphin Mall (Sandbox)", "Taubman Centers", "Enclosed Regional Mall",
                "11401 NW 12th St", "Miami", "FL", "33172", "US", false, "06:30 - 09:30", 25m,
                "Sunbelt Data Systems", "Diego Morales", "Sarah Jenkins (PS4)",
                "High tourist foot traffic. Delivery before 09:30 strict.", "Ramblas Demarc 2.");
            AddStore("9040", "CR", "Charlotte Russe", "Dolphin Mall Russe", "Ste 204", mDolphin, "Active Fleet", "Grand Open", "Complete", 8700, 7300, 6, "Commissioned", 100, "Completed & Certified", "Active.", 3, 3, "Fully Installed & Certified", 30, true, "Fluke Certified & Closed", "AT&T Fiber", "Commercial Fiber DIA", 400, "Tested & Turned Up (Active)", "Verizon 5G", "Enterprise 5G Wireless", 100, "Tested & Turned Up (Active)");
            AddStore("9041", "R21", "rue21", "Dolphin Mall rue21", "Ste 410", mDolphin, "Active Fleet", "Grand Open", "Complete", 7600, 6400, 5, "Commissioned", 100, "Completed & Certified", "Active.", 2, 2, "Fully Installed & Certified", 26, true, "Fluke Certified & Closed", "AT&T Fiber", "Commercial Fiber DIA", 300, "Tested & Turned Up (Active)", "T-Mobile 5G", "Enterprise 5G Wireless", 100, "Tested & Turned Up (Active)");

            // 20. Concord Mills (NC) — CR + R21
            var mConcord = AddMall("MALL-NC01", "Concord Mills (Sandbox)", "Simon Property Group", "Enclosed Outlet Mall",
                "8111 Concord Mills Blvd", "Concord", "NC", "28027", "US", false, "06:00 - 09:30", 20m,
                "Carolina Network Solutions", "Derrick Vance", "Marcus Vance (PM)",
                "Entry 4 loading area. Standard Simon rules.", "South Telco IDF.");
            AddStore("9042", "CR", "Charlotte Russe", "Concord Mills Russe", "Ste 312", mConcord, "Active Fleet", "Grand Open", "Complete", 8200, 6900, 5, "Commissioned", 100, "Completed & Certified", "Active.", 2, 2, "Fully Installed & Certified", 28, true, "Fluke Certified & Closed", "Spectrum Enterprise", "Commercial Fiber DIA", 300, "Tested & Turned Up (Active)", "Verizon 5G", "Enterprise 5G Wireless", 100, "Tested & Turned Up (Active)");
            AddStore("9043", "R21", "rue21", "Concord Mills rue21", "Ste 518", mConcord, "Active Fleet", "Grand Open", "Complete", 7300, 6100, 4, "Commissioned", 100, "Completed & Certified", "Active.", 2, 2, "Fully Installed & Certified", 24, true, "Fluke Certified & Closed", "Spectrum Enterprise", "Commercial Fiber DIA", 300, "Tested & Turned Up (Active)", "AT&T 5G", "Enterprise 5G Wireless", 100, "Tested & Turned Up (Active)");

            // 21. Opry Mills (TN) — CR + R21
            var mOpry = AddMall("MALL-TN01", "Opry Mills (Sandbox)", "Simon Property Group", "Enclosed Super-Regional Mall",
                "433 Opry Mills Dr", "Nashville", "TN", "37214", "US", false, "06:00 - 09:30", 20m,
                "Volunteer Low-Volt", "Braden Cole", "Marcus Vance (PM)",
                "Flood-wall egress clearance strictly enforced.", "Delta Telco Core.");
            AddStore("9044", "CR", "Charlotte Russe", "Opry Mills Russe", "Ste 110", mOpry, "Active Fleet", "Grand Open", "Complete", 8400, 7000, 6, "Commissioned", 100, "Completed & Certified", "Active.", 3, 3, "Fully Installed & Certified", 30, true, "Fluke Certified & Closed", "Comcast Business", "Commercial Fiber DIA", 300, "Tested & Turned Up (Active)", "Verizon 5G", "Enterprise 5G Wireless", 100, "Tested & Turned Up (Active)");
            AddStore("9045", "R21", "rue21", "Opry Mills rue21", "Ste 322", mOpry, "Active Fleet", "Grand Open", "Complete", 7500, 6200, 5, "Commissioned", 100, "Completed & Certified", "Active.", 2, 2, "Fully Installed & Certified", 26, true, "Fluke Certified & Closed", "Comcast Business", "Commercial Fiber DIA", 300, "Tested & Turned Up (Active)", "AT&T 5G", "Enterprise 5G Wireless", 100, "Tested & Turned Up (Active)");

            // 22. Castleton Square (IN) — CR + R21
            var mCastleton = AddMall("MALL-IN01", "Castleton Square (Sandbox)", "Simon Property Group", "Enclosed Regional Mall",
                "6020 E 82nd St", "Indianapolis", "IN", "46250", "US", true, "06:00 - 09:00", 20m,
                "Hoosier Network Techs", "Ethan Wright", "Danielle Levy (PS2)",
                "Marion County building inspections required for cabling.", "Central Demarc Shaft.");
            AddStore("9046", "CR", "Charlotte Russe", "Castleton Square Russe", "Ste 104", mCastleton, "Active Fleet", "Grand Open", "Complete", 8000, 6700, 5, "Commissioned", 100, "Completed & Certified", "Active.", 2, 2, "Fully Installed & Certified", 26, true, "Fluke Certified & Closed", "AT&T Fiber", "Commercial Fiber DIA", 300, "Tested & Turned Up (Active)", "Verizon 5G", "Enterprise 5G Wireless", 100, "Tested & Turned Up (Active)");
            AddStore("9047", "R21", "rue21", "Castleton Square rue21", "Ste 216", mCastleton, "Active Fleet", "Grand Open", "Complete", 7100, 5900, 4, "Commissioned", 100, "Completed & Certified", "Active.", 2, 2, "Fully Installed & Certified", 24, true, "Fluke Certified & Closed", "AT&T Fiber", "Commercial Fiber DIA", 300, "Tested & Turned Up (Active)", "T-Mobile 5G", "Enterprise 5G Wireless", 100, "Tested & Turned Up (Active)");

            // 23. Cherry Creek Shopping Center (CO) — CR + R21
            var mCherryCreek = AddMall("MALL-CO01", "Cherry Creek Shopping Center (Sandbox)", "Taubman Centers", "Enclosed Luxury Center",
                "3000 E 1st Ave", "Denver", "CO", "80206", "US", true, "05:30 - 08:30", 30m,
                "FrontRange Structured Nets", "Brett Collins", "Danielle Levy (PS2)",
                "Strict noise ordinance before 08:00. High-end tenant guidelines.", "Level B IDF.");
            AddStore("9048", "CR", "Charlotte Russe", "Cherry Creek Russe", "Ste 212", mCherryCreek, "Active Fleet", "Grand Open", "Complete", 8100, 6800, 5, "Commissioned", 100, "Completed & Certified", "Active.", 2, 2, "Fully Installed & Certified", 28, true, "Fluke Certified & Closed", "Lumen Fiber", "Commercial Fiber DIA", 400, "Tested & Turned Up (Active)", "Verizon 5G", "Enterprise 5G Wireless", 100, "Tested & Turned Up (Active)");
            AddStore("9049", "R21", "rue21", "Cherry Creek rue21", "Ste 118", mCherryCreek, "Active Fleet", "Grand Open", "Complete", 7200, 6000, 4, "Commissioned", 100, "Completed & Certified", "Active.", 2, 2, "Fully Installed & Certified", 24, true, "Fluke Certified & Closed", "Lumen Fiber", "Commercial Fiber DIA", 300, "Tested & Turned Up (Active)", "AT&T 5G", "Enterprise 5G Wireless", 100, "Tested & Turned Up (Active)");

            // 24. Scottsdale Fashion Square (AZ) — CR + R21
            var mScottsdale = AddMall("MALL-AZ01", "Scottsdale Fashion Square (Sandbox)", "Macerich", "Enclosed Luxury Regional Mall",
                "7014 E Camelback Rd", "Scottsdale", "AZ", "85251", "US", true, "05:00 - 08:00", 35m,
                "Desert Data Networks", "Ray Martinez", "Terry O'Connor (PS1)",
                "High aesthetic standards. All cables concealed inside drop ceiling.", "East Wing MPOE.");
            AddStore("9050", "CR", "Charlotte Russe", "Scottsdale Fashion Russe", "Ste 1020", mScottsdale, "Active Fleet", "Grand Open", "Complete", 8500, 7100, 6, "Commissioned", 100, "Completed & Certified", "Active.", 3, 3, "Fully Installed & Certified", 30, true, "Fluke Certified & Closed", "Cox Business", "Commercial Fiber DIA", 500, "Tested & Turned Up (Active)", "Verizon 5G", "Enterprise 5G Wireless", 100, "Tested & Turned Up (Active)");
            AddStore("9051", "R21", "rue21", "Scottsdale Fashion rue21", "Ste 2045", mScottsdale, "Active Fleet", "Grand Open", "Complete", 7400, 6200, 4, "Commissioned", 100, "Completed & Certified", "Active.", 2, 2, "Fully Installed & Certified", 24, true, "Fluke Certified & Closed", "Cox Business", "Commercial Fiber DIA", 300, "Tested & Turned Up (Active)", "AT&T 5G", "Enterprise 5G Wireless", 100, "Tested & Turned Up (Active)");

            // 25. Burlington Mall (MA) — CR + M
            var mBurlington = AddMall("MALL-MA01", "Burlington Mall (Sandbox)", "Simon Property Group", "Enclosed Regional Mall",
                "75 Middlesex Turnpike", "Burlington", "MA", "01803", "US", true, "06:00 - 09:00", 25m,
                "Bay State Telecom", "Colin O'Shea", "Claire Tremblay (PA)",
                "Massachusetts licensed journeyman electrician mandatory on site.", "Basement Demarc 3.");
            AddStore("9052", "CR", "Charlotte Russe", "Burlington Mall Russe", "Ste 110", mBurlington, "Active Fleet", "Grand Open", "Complete", 7900, 6600, 5, "Commissioned", 100, "Completed & Certified", "Active.", 2, 2, "Fully Installed & Certified", 26, true, "Fluke Certified & Closed", "Verizon Fios", "Commercial Fiber DIA", 400, "Tested & Turned Up (Active)", "Verizon 5G", "Enterprise 5G Wireless", 100, "Tested & Turned Up (Active)");
            AddStore("9053", "M", "Mandee", "Burlington Mall Mandee", "Ste 225", mBurlington, "Active Fleet", "Grand Open", "Complete", 7100, 5900, 4, "Commissioned", 100, "Completed & Certified", "Active.", 2, 2, "Fully Installed & Certified", 24, true, "Fluke Certified & Closed", "Verizon Fios", "Commercial Fiber DIA", 300, "Tested & Turned Up (Active)", "T-Mobile 5G", "Enterprise 5G Wireless", 100, "Tested & Turned Up (Active)");

            // 26. Westfield Garden State Plaza (NJ) — CR + M
            var mGardenState = AddMall("MALL-NJ01", "Garden State Plaza (Sandbox)", "Unibail-Rodamco-Westfield", "Enclosed Super-Regional Mall",
                "1 Garden State Plaza Blvd", "Paramus", "NJ", "07652", "US", true, "05:00 - 08:30", 35m,
                "Garden State Structured", "Lou Moretti", "Claire Tremblay (PA)",
                "Paramus blue laws restrict Sunday operations. Union labor mandatory.", "MPOE Vault Paramus.");
            AddStore("9054", "CR", "Charlotte Russe", "Garden State Plaza Russe", "Ste 1030", mGardenState, "Active Fleet", "Grand Open", "Complete", 8900, 7500, 6, "Commissioned", 100, "Completed & Certified", "Active.", 3, 3, "Fully Installed & Certified", 32, true, "Fluke Certified & Closed", "Verizon Fios", "Commercial Fiber DIA", 500, "Tested & Turned Up (Active)", "AT&T 5G", "Enterprise 5G Wireless", 100, "Tested & Turned Up (Active)");
            AddStore("9055", "M", "Mandee", "Garden State Plaza Mandee", "Ste 2110", mGardenState, "Active Fleet", "Grand Open", "Complete", 7500, 6200, 5, "Commissioned", 100, "Completed & Certified", "Active.", 2, 2, "Fully Installed & Certified", 26, true, "Fluke Certified & Closed", "Verizon Fios", "Commercial Fiber DIA", 300, "Tested & Turned Up (Active)", "Verizon 5G", "Enterprise 5G Wireless", 100, "Tested & Turned Up (Active)");

            // 27. Lenox Square (GA) — CR + R21
            var mLenox = AddMall("MALL-GA01", "Lenox Square (Sandbox)", "Simon Property Group", "Enclosed Super-Regional Mall",
                "3393 Peachtree Rd NE", "Atlanta", "GA", "30326", "US", true, "05:30 - 08:30", 25m,
                "Peachtree Low-Voltage", "Darius Bell", "Marcus Vance (PM)",
                "Buckhead luxury corridor standards. Fire suppression tie-in sign-off.", "IDF North Wing.");
            AddStore("9056", "CR", "Charlotte Russe", "Lenox Square Russe", "Ste 3014", mLenox, "Active Fleet", "Grand Open", "Complete", 8200, 6900, 5, "Commissioned", 100, "Completed & Certified", "Active.", 2, 2, "Fully Installed & Certified", 28, true, "Fluke Certified & Closed", "AT&T Fiber", "Commercial Fiber DIA", 400, "Tested & Turned Up (Active)", "Verizon 5G", "Enterprise 5G Wireless", 100, "Tested & Turned Up (Active)");
            AddStore("9057", "R21", "rue21", "Lenox Square rue21", "Ste 1088", mLenox, "Active Fleet", "Grand Open", "Complete", 7300, 6100, 4, "Commissioned", 100, "Completed & Certified", "Active.", 2, 2, "Fully Installed & Certified", 24, true, "Fluke Certified & Closed", "AT&T Fiber", "Commercial Fiber DIA", 300, "Tested & Turned Up (Active)", "T-Mobile 5G", "Enterprise 5G Wireless", 100, "Tested & Turned Up (Active)");

            // 28. Mall of Georgia (GA) — CR + R21
            var mMallOfGa = AddMall("MALL-GA02", "Mall of Georgia (Sandbox)", "Simon Property Group", "Enclosed Super-Regional Mall",
                "3333 Buford Dr", "Buford", "GA", "30519", "US", false, "06:00 - 09:30", 20m,
                "Peachtree Low-Voltage", "Darius Bell", "Marcus Vance (PM)",
                "Village concourse loading access. Standard Simon procedures.", "Demarc West Village.");
            AddStore("9058", "CR", "Charlotte Russe", "Mall of Georgia Russe", "Ste 110", mMallOfGa, "Active Fleet", "Grand Open", "Complete", 8600, 7200, 6, "Commissioned", 100, "Completed & Certified", "Active.", 3, 3, "Fully Installed & Certified", 30, true, "Fluke Certified & Closed", "Spectrum Enterprise", "Commercial Fiber DIA", 300, "Tested & Turned Up (Active)", "Verizon 5G", "Enterprise 5G Wireless", 100, "Tested & Turned Up (Active)");
            AddStore("9059", "R21", "rue21", "Mall of Georgia rue21", "Ste 225", mMallOfGa, "Active Fleet", "Grand Open", "Complete", 7700, 6400, 5, "Commissioned", 100, "Completed & Certified", "Active.", 2, 2, "Fully Installed & Certified", 26, true, "Fluke Certified & Closed", "Spectrum Enterprise", "Commercial Fiber DIA", 300, "Tested & Turned Up (Active)", "AT&T 5G", "Enterprise 5G Wireless", 100, "Tested & Turned Up (Active)");

            // 29. Stonebriar Centre (TX) — CR + R21
            var mStonebriar = AddMall("MALL-TX03", "Stonebriar Centre (Sandbox)", "Brookfield Properties", "Enclosed Regional Mall",
                "2601 Preston Rd", "Frisco", "TX", "75034", "US", true, "06:00 - 09:00", 20m,
                "LoneStar Network Techs", "Carlos Mendez", "Marcus Vance (PM)",
                "Frisco fire marshal conduit inspection required prior to ceiling close.", "Basement Telco 1.");
            AddStore("9060", "CR", "Charlotte Russe", "Stonebriar Centre Russe", "Ste 124", mStonebriar, "Active Fleet", "Grand Open", "Complete", 8100, 6800, 5, "Commissioned", 100, "Completed & Certified", "Active.", 2, 2, "Fully Installed & Certified", 28, true, "Fluke Certified & Closed", "AT&T Fiber", "Commercial Fiber DIA", 300, "Tested & Turned Up (Active)", "Verizon 5G", "Enterprise 5G Wireless", 100, "Tested & Turned Up (Active)");
            AddStore("9061", "R21", "rue21", "Stonebriar Centre rue21", "Ste 230", mStonebriar, "Active Fleet", "Grand Open", "Complete", 7200, 6000, 4, "Commissioned", 100, "Completed & Certified", "Active.", 2, 2, "Fully Installed & Certified", 24, true, "Fluke Certified & Closed", "AT&T Fiber", "Commercial Fiber DIA", 300, "Tested & Turned Up (Active)", "T-Mobile 5G", "Enterprise 5G Wireless", 100, "Tested & Turned Up (Active)");

            // 30. Twelve Oaks Mall (MI) — CR + R21
            var mTwelveOaks = AddMall("MALL-MI02", "Twelve Oaks Mall (Sandbox)", "Taubman Centers", "Enclosed Regional Mall",
                "27500 Novi Rd", "Novi", "MI", "48377", "US", true, "05:30 - 08:30", 25m,
                "MotorCity Structured Nets", "Kurt Wagner", "Danielle Levy (PS2)",
                "Loading bay 2. Security clearance for core telecom risers.", "Riser Shaft B.");
            AddStore("9062", "CR", "Charlotte Russe", "Twelve Oaks Russe", "Ste 140", mTwelveOaks, "Active Fleet", "Grand Open", "Complete", 8000, 6700, 5, "Commissioned", 100, "Completed & Certified", "Active.", 2, 2, "Fully Installed & Certified", 26, true, "Fluke Certified & Closed", "Comcast Business", "Commercial Fiber DIA", 300, "Tested & Turned Up (Active)", "Verizon 5G", "Enterprise 5G Wireless", 100, "Tested & Turned Up (Active)");
            AddStore("9063", "R21", "rue21", "Twelve Oaks rue21", "Ste 210", mTwelveOaks, "Active Fleet", "Grand Open", "Complete", 7300, 6100, 4, "Commissioned", 100, "Completed & Certified", "Active.", 2, 2, "Fully Installed & Certified", 24, true, "Fluke Certified & Closed", "Comcast Business", "Commercial Fiber DIA", 300, "Tested & Turned Up (Active)", "AT&T 5G", "Enterprise 5G Wireless", 100, "Tested & Turned Up (Active)");

            // =========================================================================
            // PART 2: CANADIAN CO-LOCATED MALLS (Multiple YM Inc banners in Canada)
            // e.g. Bluenotes and Urban Planet / Sirens in the same mall
            // =========================================================================

            // 31. Toronto Eaton Centre (ON) — BN + UP
            var mEaton = AddMall("MALL-ON01", "CF Toronto Eaton Centre (Sandbox)", "Cadillac Fairview", "Enclosed Urban Super-Mall",
                "220 Yonge St", "Toronto", "ON", "M5B 2H1", "CA", true, "06:00 - 09:00", 35m,
                "Maple Leaf Telecom", "Liam Foster", "Terry O'Connor (PS1)",
                "Union labor only. Strict downtown Toronto dock booking system.", "Bell Telco Vault Level B2.");
            AddStore("9064", "BN", "Bluenotes", "Toronto Eaton Bluenotes", "Unit 2-108", mEaton, "Active Fleet", "Grand Open", "Complete", 8500, 7100, 6, "Commissioned", 100, "Completed & Certified", "Active flagship.", 3, 3, "Fully Installed & Certified", 30, true, "Fluke Certified & Closed", "Bell Canada", "Commercial Fiber DIA", 500, "Tested & Turned Up (Active)", "Rogers Business", "Enterprise 5G Wireless", 150, "Tested & Turned Up (Active)");
            AddStore("9065", "UP", "Urban Planet", "Toronto Eaton Urban Planet", "Unit 1-140", mEaton, "Active Fleet", "Grand Open", "Complete", 18500, 15500, 10, "Commissioned", 100, "Completed & Certified", "Active mega-store.", 5, 5, "Fully Installed & Certified", 56, true, "Fluke Certified & Closed", "Bell Canada", "Commercial Fiber DIA", 500, "Tested & Turned Up (Active)", "Rogers Business", "Enterprise 5G Wireless", 150, "Tested & Turned Up (Active)");

            // 32. Yorkdale Shopping Centre (ON) — BN + SF
            var mYorkdale = AddMall("MALL-ON02", "Yorkdale Shopping Centre (Sandbox)", "Oxford Properties", "Enclosed Luxury Super-Mall",
                "3401 Dufferin St", "Toronto", "ON", "M6A 2T9", "CA", true, "05:30 - 08:30", 35m,
                "Maple Leaf Telecom", "Liam Foster", "Terry O'Connor (PS1)",
                "Highest CAD sales/sqft mall in Canada. Strict tenant finish rules.", "Oxford Telco Hub B.");
            AddStore("9066", "BN", "Bluenotes", "Yorkdale Bluenotes", "Wing B Ste 112", mYorkdale, "Active Fleet", "Grand Open", "Complete", 8200, 6900, 5, "Commissioned", 100, "Completed & Certified", "Active.", 2, 2, "Fully Installed & Certified", 28, true, "Fluke Certified & Closed", "Bell Canada", "Commercial Fiber DIA", 500, "Tested & Turned Up (Active)", "Rogers Business", "Enterprise 5G Wireless", 100, "Tested & Turned Up (Active)");
            AddStore("9067", "SF", "Sirens", "Yorkdale Sirens", "Wing C Ste 204", mYorkdale, "Active Fleet", "Grand Open", "Complete", 6800, 5600, 4, "Commissioned", 100, "Completed & Certified", "Active.", 2, 2, "Fully Installed & Certified", 24, true, "Fluke Certified & Closed", "Bell Canada", "Commercial Fiber DIA", 300, "Tested & Turned Up (Active)", "Rogers Business", "Enterprise 5G Wireless", 100, "Tested & Turned Up (Active)");

            // 33. West Edmonton Mall (AB) — UP + BN + SF
            var mWem = AddMall("MALL-AB01", "West Edmonton Mall (Sandbox)", "Triple Five Group", "Enclosed Mega-Mall & Entertainment",
                "8882 170 St NW", "Edmonton", "AB", "T5T 4J2", "CA", false, "06:00 - 10:00", 25m,
                "Prairie Tech Solutions", "Duncan Ross", "Hector Ruiz (PS3)",
                "Phase 3 loading dock. 10km of corridors. Security cart required.", "Phase 3 IDF Core.");
            AddStore("9068", "UP", "Urban Planet", "West Edmonton Urban Planet", "Phase 3 Ste 2100", mWem, "Active Fleet", "Grand Open", "Complete", 19200, 16100, 10, "Commissioned", 100, "Completed & Certified", "Active flagship.", 5, 5, "Fully Installed & Certified", 60, true, "Fluke Certified & Closed", "Telus Business", "Commercial Fiber DIA", 500, "Tested & Turned Up (Active)", "Shaw Business", "Commercial Fiber DIA", 300, "Tested & Turned Up (Active)");
            AddStore("9069", "BN", "Bluenotes", "West Edmonton Bluenotes", "Phase 1 Ste 1140", mWem, "Active Fleet", "Grand Open", "Complete", 8900, 7500, 6, "Commissioned", 100, "Completed & Certified", "Active.", 3, 3, "Fully Installed & Certified", 30, true, "Fluke Certified & Closed", "Telus Business", "Commercial Fiber DIA", 300, "Tested & Turned Up (Active)", "Rogers 5G", "Enterprise 5G Wireless", 100, "Tested & Turned Up (Active)");
            AddStore("9070", "SF", "Sirens", "West Edmonton Sirens", "Phase 2 Ste 1620", mWem, "Active Fleet", "Grand Open", "Complete", 7200, 6000, 4, "Commissioned", 100, "Completed & Certified", "Active.", 2, 2, "Fully Installed & Certified", 24, true, "Fluke Certified & Closed", "Telus Business", "Commercial Fiber DIA", 300, "Tested & Turned Up (Active)", "Rogers 5G", "Enterprise 5G Wireless", 100, "Tested & Turned Up (Active)");

            // 34. Metropolis at Metrotown (BC) — BN + UP
            var mMetrotown = AddMall("MALL-BC01", "Metropolis at Metrotown (Sandbox)", "Ivanhoé Cambridge", "Enclosed Super-Regional Mall",
                "4700 Kingsway", "Burnaby", "BC", "V5H 4M5", "CA", true, "05:30 - 08:30", 30m,
                "Pacific Rim Cabling", "Gordon Wong", "Hector Ruiz (PS3)",
                "Seismic tie-wire standards required for all ceiling drop poles.", "Central MPOE Vault.");
            AddStore("9071", "BN", "Bluenotes", "Metrotown Bluenotes", "Atrium Ste 118", mMetrotown, "Active Fleet", "Grand Open", "Complete", 8600, 7200, 6, "Commissioned", 100, "Completed & Certified", "Active.", 3, 3, "Fully Installed & Certified", 30, true, "Fluke Certified & Closed", "Telus Business", "Commercial Fiber DIA", 400, "Tested & Turned Up (Active)", "Rogers 5G", "Enterprise 5G Wireless", 100, "Tested & Turned Up (Active)");
            AddStore("9072", "UP", "Urban Planet", "Metrotown Urban Planet", "Lower Level Ste 45", mMetrotown, "Active Fleet", "Grand Open", "Complete", 17000, 14200, 9, "Commissioned", 100, "Completed & Certified", "Active.", 4, 4, "Fully Installed & Certified", 52, true, "Fluke Certified & Closed", "Telus Business", "Commercial Fiber DIA", 500, "Tested & Turned Up (Active)", "Rogers 5G", "Enterprise 5G Wireless", 150, "Tested & Turned Up (Active)");

            // 35. Square One Shopping Centre (ON) — BN + UP
            var mSquareOne = AddMall("MALL-ON03", "Square One Shopping Centre (Sandbox)", "Oxford Properties", "Enclosed Super-Regional Mall",
                "100 City Centre Dr", "Mississauga", "ON", "L5B 2C9", "CA", true, "05:30 - 08:30", 30m,
                "Maple Leaf Telecom", "Liam Foster", "Terry O'Connor (PS1)",
                "Peel Region safety certs required for ladder/scissor lift.", "IDF North Concourse.");
            AddStore("9073", "BN", "Bluenotes", "Square One Bluenotes", "Ste 2-210", mSquareOne, "Active Fleet", "Grand Open", "Complete", 8800, 7400, 6, "Commissioned", 100, "Completed & Certified", "Active.", 3, 3, "Fully Installed & Certified", 30, true, "Fluke Certified & Closed", "Bell Canada", "Commercial Fiber DIA", 400, "Tested & Turned Up (Active)", "Rogers Business", "Enterprise 5G Wireless", 100, "Tested & Turned Up (Active)");
            AddStore("9074", "UP", "Urban Planet", "Square One Urban Planet", "Ste 1-180", mSquareOne, "Active Fleet", "Grand Open", "Complete", 16800, 14100, 9, "Commissioned", 100, "Completed & Certified", "Active.", 4, 4, "Fully Installed & Certified", 50, true, "Fluke Certified & Closed", "Bell Canada", "Commercial Fiber DIA", 500, "Tested & Turned Up (Active)", "Rogers Business", "Enterprise 5G Wireless", 150, "Tested & Turned Up (Active)");

            // =========================================================================
            // PART 3: REMAINING 126 STORES (Mainly US Premier Markets + Canada)
            // Bringing the fleet up to exactly 200 Stores
            // =========================================================================

            var additionalPremierLocations = new[]
            {
                // US Multi & Major Metro Premier Hubs (TX, CA, FL, NY, OH, PA, NC, GA, VA, MI, IL, TN, MO, IN, WA, CO, AZ, NV, MD, WI, UT, etc.)
                ("MALL-TX04", "The Galleria Dallas", "Galleria Mall Investors", "Dallas", "TX", "75240", "US", "R21", "rue21", "Galleria Dallas rue21", "Ste 214", "AT&T Fiber", "Verizon 5G"),
                ("MALL-TX05", "Barton Creek Square", "Simon Property Group", "Austin", "TX", "78746", "US", "CR", "Charlotte Russe", "Barton Creek Russe", "Ste 110", "AT&T Fiber", "Verizon 5G"),
                ("MALL-TX06", "The Shops at La Cantera", "Brookfield Properties", "San Antonio", "TX", "78256", "US", "CR", "Charlotte Russe", "La Cantera Russe", "Ste 204", "Spectrum Enterprise", "AT&T 5G"),
                ("MALL-TX07", "North Star Mall", "Brookfield Properties", "San Antonio", "TX", "78216", "US", "R21", "rue21", "North Star Mall rue21", "Ste 312", "Spectrum Enterprise", "AT&T 5G"),
                ("MALL-TX08", "The Woodlands Mall", "Brookfield Properties", "The Woodlands", "TX", "77380", "US", "CR", "Charlotte Russe", "Woodlands Mall Russe", "Ste 140", "Comcast Business", "Verizon 5G"),
                ("MALL-TX09", "Memorial City Mall", "MetroNational", "Houston", "TX", "77024", "US", "R21", "rue21", "Memorial City rue21", "Ste 210", "Comcast Business", "Verizon 5G"),
                ("MALL-TX10", "Baybrook Mall", "Brookfield Properties", "Friendswood", "TX", "77546", "US", "CR", "Charlotte Russe", "Baybrook Mall Russe", "Ste 118", "AT&T Fiber", "T-Mobile 5G"),
                ("MALL-TX11", "Cielo Vista Mall", "Simon Property Group", "El Paso", "TX", "79925", "US", "CR", "Charlotte Russe", "Cielo Vista Russe", "Ste 302", "Spectrum Enterprise", "Verizon 5G"),
                ("MALL-TX12", "Cielo Vista Mall", "Simon Property Group", "El Paso", "TX", "79925", "US", "R21", "rue21", "Cielo Vista rue21", "Ste 514", "Spectrum Enterprise", "AT&T 5G"),
                ("MALL-TX13", "Town East Mall", "Brookfield Properties", "Mesquite", "TX", "75150", "US", "R21", "rue21", "Town East rue21", "Ste 112", "AT&T Fiber", "Verizon 5G"),
                
                ("MALL-CA03", "Brea Mall", "Simon Property Group", "Brea", "CA", "92821", "US", "CR", "Charlotte Russe", "Brea Mall Russe", "Ste 204", "Spectrum Enterprise", "Verizon 5G"),
                ("MALL-CA04", "Brea Mall", "Simon Property Group", "Brea", "CA", "92821", "US", "R21", "rue21", "Brea Mall rue21", "Ste 412", "Spectrum Enterprise", "AT&T 5G"),
                ("MALL-CA05", "Glendale Galleria", "Brookfield Properties", "Glendale", "CA", "91210", "US", "CR", "Charlotte Russe", "Glendale Galleria Russe", "Ste 114", "Spectrum Enterprise", "Verizon 5G"),
                ("MALL-CA06", "Glendale Galleria", "Brookfield Properties", "Glendale", "CA", "91210", "US", "R21", "rue21", "Glendale Galleria rue21", "Ste 322", "Spectrum Enterprise", "AT&T 5G"),
                ("MALL-CA07", "Del Amo Fashion Center", "Simon Property Group", "Torrance", "CA", "90503", "US", "CR", "Charlotte Russe", "Del Amo Russe", "Ste 210", "Frontier Fiber", "Verizon 5G"),
                ("MALL-CA08", "Del Amo Fashion Center", "Simon Property Group", "Torrance", "CA", "90503", "US", "R21", "rue21", "Del Amo rue21", "Ste 418", "Frontier Fiber", "AT&T 5G"),
                ("MALL-CA09", "Sunvalley Shopping Center", "Taubman Centers", "Concord", "CA", "94520", "US", "CR", "Charlotte Russe", "Sunvalley Russe", "Ste 108", "Comcast Business", "Verizon 5G"),
                ("MALL-CA10", "Westfield Valley Fair", "Unibail-Rodamco-Westfield", "San Jose", "CA", "95128", "US", "CR", "Charlotte Russe", "Valley Fair Russe", "Ste 2012", "AT&T Fiber", "Verizon 5G"),
                ("MALL-CA11", "Westfield Valley Fair", "Unibail-Rodamco-Westfield", "San Jose", "CA", "95128", "US", "R21", "rue21", "Valley Fair rue21", "Ste 1045", "AT&T Fiber", "T-Mobile 5G"),
                ("MALL-CA12", "Arden Fair", "Fulcrum Property", "Sacramento", "CA", "95815", "US", "CR", "Charlotte Russe", "Arden Fair Russe", "Ste 118", "Comcast Business", "Verizon 5G"),
                ("MALL-CA13", "Westfield Galleria at Roseville", "Unibail-Rodamco", "Roseville", "CA", "95678", "US", "R21", "rue21", "Roseville Galleria rue21", "Ste 220", "Comcast Business", "AT&T 5G"),
                ("MALL-CA14", "Fashion Valley", "Simon Property Group", "San Diego", "CA", "92108", "US", "CR", "Charlotte Russe", "Fashion Valley Russe", "Ste 114", "Cox Business", "Verizon 5G"),
                ("MALL-CA15", "Fashion Valley", "Simon Property Group", "San Diego", "CA", "92108", "US", "R21", "rue21", "Fashion Valley rue21", "Ste 308", "Cox Business", "AT&T 5G"),

                ("MALL-FL05", "International Plaza", "Taubman Centers", "Tampa", "FL", "33607", "US", "CR", "Charlotte Russe", "Tampa Int Plaza Russe", "Ste 124", "Spectrum Enterprise", "Verizon 5G"),
                ("MALL-FL06", "International Plaza", "Taubman Centers", "Tampa", "FL", "33607", "US", "R21", "rue21", "Tampa Int Plaza rue21", "Ste 218", "Spectrum Enterprise", "AT&T 5G"),
                ("MALL-FL07", "The Florida Mall", "Simon Property Group", "Orlando", "FL", "32809", "US", "CR", "Charlotte Russe", "Florida Mall Russe", "Ste 102", "AT&T Fiber", "Verizon 5G"),
                ("MALL-FL08", "The Florida Mall", "Simon Property Group", "Orlando", "FL", "32809", "US", "R21", "rue21", "Florida Mall rue21", "Ste 314", "AT&T Fiber", "T-Mobile 5G"),
                ("MALL-FL09", "St. Johns Town Center", "Simon Property Group", "Jacksonville", "FL", "32246", "US", "CR", "Charlotte Russe", "St Johns Russe", "Ste 110", "Comcast Business", "Verizon 5G"),
                ("MALL-FL10", "Town Center at Boca Raton", "Simon Property Group", "Boca Raton", "FL", "33431", "US", "M", "Mandee", "Boca Raton Mandee", "Ste 210", "AT&T Fiber", "Verizon 5G"),
                ("MALL-FL11", "Brandon Town Center", "Brookfield Properties", "Brandon", "FL", "33511", "US", "R21", "rue21", "Brandon rue21", "Ste 104", "Spectrum Enterprise", "AT&T 5G"),

                ("MALL-NY03", "Crossgates Mall", "Pyramid Management Group", "Albany", "NY", "12203", "US", "CR", "Charlotte Russe", "Crossgates Russe", "Ste 112", "Spectrum Enterprise", "Verizon 5G"),
                ("MALL-NY04", "Crossgates Mall", "Pyramid Management Group", "Albany", "NY", "12203", "US", "R21", "rue21", "Crossgates rue21", "Ste 220", "Spectrum Enterprise", "AT&T 5G"),
                ("MALL-NY05", "Walden Galleria", "Pyramid Management Group", "Buffalo", "NY", "14225", "US", "CR", "Charlotte Russe", "Walden Galleria Russe", "Ste 118", "Spectrum Enterprise", "Verizon 5G"),
                ("MALL-NY06", "Walden Galleria", "Pyramid Management Group", "Buffalo", "NY", "14225", "US", "R21", "rue21", "Walden Galleria rue21", "Ste 304", "Spectrum Enterprise", "AT&T 5G"),
                ("MALL-NY07", "Palisades Center", "Pyramid Management Group", "West Nyack", "NY", "10994", "US", "CR", "Charlotte Russe", "Palisades Center Russe", "Ste 210", "Verizon Fios", "AT&T 5G"),
                ("MALL-NY08", "Palisades Center", "Pyramid Management Group", "West Nyack", "NY", "10994", "US", "M", "Mandee", "Palisades Center Mandee", "Ste 314", "Verizon Fios", "Verizon 5G"),
                ("MALL-NY09", "Queens Center", "Macerich", "Elmhurst", "NY", "11373", "US", "M", "Mandee", "Queens Center Mandee", "Ste 104", "Verizon Fios", "Verizon 5G"),
                ("MALL-NY10", "Kings Plaza", "Macerich", "Brooklyn", "NY", "11234", "US", "CR", "Charlotte Russe", "Kings Plaza Russe", "Ste 112", "Verizon Fios", "AT&T 5G"),

                ("MALL-NJ02", "The Mills at Jersey Gardens", "Simon Property Group", "Elizabeth", "NJ", "07201", "US", "CR", "Charlotte Russe", "Jersey Gardens Russe", "Ste 104", "Verizon Fios", "Verizon 5G"),
                ("MALL-NJ03", "The Mills at Jersey Gardens", "Simon Property Group", "Elizabeth", "NJ", "07201", "US", "R21", "rue21", "Jersey Gardens rue21", "Ste 218", "Verizon Fios", "AT&T 5G"),
                ("MALL-NJ04", "The Mills at Jersey Gardens", "Simon Property Group", "Elizabeth", "NJ", "07201", "US", "M", "Mandee", "Jersey Gardens Mandee", "Ste 310", "Verizon Fios", "T-Mobile 5G"),
                ("MALL-NJ05", "Willowbrook Mall NJ", "Brookfield Properties", "Wayne", "NJ", "07470", "US", "CR", "Charlotte Russe", "Willowbrook NJ Russe", "Ste 124", "Verizon Fios", "Verizon 5G"),
                ("MALL-NJ06", "Cherry Hill Mall", "PREIT", "Cherry Hill", "NJ", "08002", "US", "CR", "Charlotte Russe", "Cherry Hill Russe", "Ste 110", "Comcast Business", "Verizon 5G"),
                ("MALL-NJ07", "Cherry Hill Mall", "PREIT", "Cherry Hill", "NJ", "08002", "US", "R21", "rue21", "Cherry Hill rue21", "Ste 214", "Comcast Business", "AT&T 5G"),

                ("MALL-PA02", "Philadelphia Mills", "Simon Property Group", "Philadelphia", "PA", "19154", "US", "CR", "Charlotte Russe", "Philadelphia Mills Russe", "Ste 204", "Comcast Business", "Verizon 5G"),
                ("MALL-PA03", "Philadelphia Mills", "Simon Property Group", "Philadelphia", "PA", "19154", "US", "R21", "rue21", "Philadelphia Mills rue21", "Ste 410", "Comcast Business", "AT&T 5G"),
                ("MALL-PA04", "Ross Park Mall", "Simon Property Group", "Pittsburgh", "PA", "15237", "US", "CR", "Charlotte Russe", "Ross Park Russe", "Ste 114", "Verizon Fios", "Verizon 5G"),
                ("MALL-PA05", "Park City Center", "Brookfield Properties", "Lancaster", "PA", "17601", "US", "R21", "rue21", "Park City rue21", "Ste 108", "Comcast Business", "AT&T 5G"),

                ("MALL-OH03", "Easton Town Center", "Steiner + Associates", "Columbus", "OH", "43219", "US", "CR", "Charlotte Russe", "Easton Town Russe", "Ste 210", "Spectrum Enterprise", "Verizon 5G"),
                ("MALL-OH04", "SouthPark Mall Strongsville", "Kohan Retail", "Strongsville", "OH", "44136", "US", "R21", "rue21", "SouthPark OH rue21", "Ste 112", "Spectrum Enterprise", "AT&T 5G"),
                ("MALL-OH05", "Dayton Mall", "Washington Prime Group", "Dayton", "OH", "45459", "US", "CR", "Charlotte Russe", "Dayton Mall Russe", "Ste 104", "Spectrum Enterprise", "Verizon 5G"),

                ("MALL-IL03", "Oakbrook Center", "Brookfield Properties", "Oak Brook", "IL", "60523", "US", "CR", "Charlotte Russe", "Oakbrook Center Russe", "Ste 118", "Comcast Business", "Verizon 5G"),
                ("MALL-IL04", "Orland Square", "Simon Property Group", "Orland Park", "IL", "60462", "US", "CR", "Charlotte Russe", "Orland Square Russe", "Ste 104", "Comcast Business", "Verizon 5G"),
                ("MALL-IL05", "Orland Square", "Simon Property Group", "Orland Park", "IL", "60462", "US", "R21", "rue21", "Orland Square rue21", "Ste 214", "Comcast Business", "AT&T 5G"),
                ("MALL-IL06", "Fox Valley Mall", "Centennial Real Estate", "Aurora", "IL", "60504", "US", "R21", "rue21", "Fox Valley rue21", "Ste 110", "Comcast Business", "Verizon 5G"),

                ("MALL-NC02", "SouthPark Charlotte", "Simon Property Group", "Charlotte", "NC", "28211", "US", "CR", "Charlotte Russe", "SouthPark Charlotte Russe", "Ste 112", "Spectrum Enterprise", "Verizon 5G"),
                ("MALL-NC03", "Crabtree Valley Mall", "Pacific Retail", "Raleigh", "NC", "27612", "US", "CR", "Charlotte Russe", "Crabtree Valley Russe", "Ste 210", "Spectrum Enterprise", "Verizon 5G"),
                ("MALL-NC04", "Crabtree Valley Mall", "Pacific Retail", "Raleigh", "NC", "27612", "US", "R21", "rue21", "Crabtree Valley rue21", "Ste 104", "Spectrum Enterprise", "AT&T 5G"),
                ("MALL-NC05", "Hanes Mall", "CBL Properties", "Winston-Salem", "NC", "27103", "US", "R21", "rue21", "Hanes Mall rue21", "Ste 114", "Spectrum Enterprise", "Verizon 5G"),

                ("MALL-GA03", "Perimeter Mall", "Brookfield Properties", "Atlanta", "GA", "30346", "US", "CR", "Charlotte Russe", "Perimeter Mall Russe", "Ste 108", "AT&T Fiber", "Verizon 5G"),
                ("MALL-GA04", "Perimeter Mall", "Brookfield Properties", "Atlanta", "GA", "30346", "US", "R21", "rue21", "Perimeter Mall rue21", "Ste 216", "AT&T Fiber", "T-Mobile 5G"),
                ("MALL-GA05", "Cumberland Mall", "Brookfield Properties", "Atlanta", "GA", "30339", "US", "CR", "Charlotte Russe", "Cumberland Mall Russe", "Ste 114", "Comcast Business", "Verizon 5G"),
                ("MALL-GA06", "Town Center at Cobb", "Kohan Retail", "Kennesaw", "GA", "30144", "US", "R21", "rue21", "Town Center Cobb rue21", "Ste 102", "Comcast Business", "AT&T 5G"),

                ("MALL-VA02", "Tysons Corner Center", "Macerich", "Tysons", "VA", "22102", "US", "CR", "Charlotte Russe", "Tysons Corner Russe", "Ste 1120", "Verizon Fios", "Verizon 5G"),
                ("MALL-VA03", "Fair Oaks Mall", "Taubman Centers", "Fairfax", "VA", "22033", "US", "CR", "Charlotte Russe", "Fair Oaks Russe", "Ste 104", "Verizon Fios", "Verizon 5G"),
                ("MALL-VA04", "Lynnhaven Mall", "Brookfield Properties", "Virginia Beach", "VA", "23452", "US", "R21", "rue21", "Lynnhaven rue21", "Ste 210", "Cox Business", "Verizon 5G"),

                ("MALL-CO02", "Colorado Mills", "Simon Property Group", "Lakewood", "CO", "80401", "US", "CR", "Charlotte Russe", "Colorado Mills Russe", "Ste 114", "Comcast Business", "Verizon 5G"),
                ("MALL-CO03", "Colorado Mills", "Simon Property Group", "Lakewood", "CO", "80401", "US", "R21", "rue21", "Colorado Mills rue21", "Ste 220", "Comcast Business", "AT&T 5G"),
                ("MALL-CO04", "Park Meadows", "Brookfield Properties", "Lone Tree", "CO", "80124", "US", "CR", "Charlotte Russe", "Park Meadows Russe", "Ste 108", "CenturyLink Fiber", "Verizon 5G"),

                ("MALL-AZ02", "Chandler Fashion Center", "Macerich", "Chandler", "AZ", "85226", "US", "CR", "Charlotte Russe", "Chandler Fashion Russe", "Ste 112", "Cox Business", "Verizon 5G"),
                ("MALL-AZ03", "Chandler Fashion Center", "Macerich", "Chandler", "AZ", "85226", "US", "R21", "rue21", "Chandler Fashion rue21", "Ste 216", "Cox Business", "AT&T 5G"),
                ("MALL-AZ04", "Arrowhead Towne Center", "Macerich", "Glendale", "AZ", "85308", "US", "R21", "rue21", "Arrowhead rue21", "Ste 104", "Cox Business", "Verizon 5G"),

                ("MALL-WA01", "Alderwood Mall", "Brookfield Properties", "Lynnwood", "WA", "98036", "US", "CR", "Charlotte Russe", "Alderwood Russe", "Ste 110", "Comcast Business", "Verizon 5G"),
                ("MALL-WA02", "Alderwood Mall", "Brookfield Properties", "Lynnwood", "WA", "98036", "US", "R21", "rue21", "Alderwood rue21", "Ste 218", "Comcast Business", "AT&T 5G"),
                ("MALL-WA03", "Southcenter Mall", "Westfield/URW", "Tukwila", "WA", "98188", "US", "CR", "Charlotte Russe", "Southcenter Russe", "Ste 1040", "CenturyLink Fiber", "Verizon 5G"),

                ("MALL-OR01", "Washington Square OR", "Macerich", "Tigard", "OR", "97223", "US", "CR", "Charlotte Russe", "Washington Sq OR Russe", "Ste 108", "Comcast Business", "Verizon 5G"),
                ("MALL-OR02", "Washington Square OR", "Macerich", "Tigard", "OR", "97223", "US", "R21", "rue21", "Washington Sq OR rue21", "Ste 214", "Comcast Business", "AT&T 5G"),

                ("MALL-NV01", "Fashion Show Las Vegas", "Brookfield Properties", "Las Vegas", "NV", "89109", "US", "CR", "Charlotte Russe", "Fashion Show LV Russe", "Ste 1120", "Cox Business", "Verizon 5G"),
                ("MALL-NV02", "Las Vegas South Premium", "Simon Property Group", "Las Vegas", "NV", "89123", "US", "R21", "rue21", "LV South Premium rue21", "Ste 104", "Cox Business", "AT&T 5G"),

                ("MALL-MO01", "Saint Louis Galleria", "Brookfield Properties", "St. Louis", "MO", "63117", "US", "CR", "Charlotte Russe", "St Louis Galleria Russe", "Ste 114", "Spectrum Enterprise", "Verizon 5G"),
                ("MALL-MO02", "West County Center", "CBL Properties", "Des Peres", "MO", "63131", "US", "R21", "rue21", "West County rue21", "Ste 210", "Spectrum Enterprise", "AT&T 5G"),

                ("MALL-TN02", "West Town Mall", "Simon Property Group", "Knoxville", "TN", "37919", "US", "CR", "Charlotte Russe", "West Town Russe", "Ste 108", "Comcast Business", "Verizon 5G"),
                ("MALL-TN03", "Wolfchase Galleria", "Simon Property Group", "Memphis", "TN", "38133", "US", "R21", "rue21", "Wolfchase rue21", "Ste 112", "Comcast Business", "AT&T 5G"),

                ("MALL-WI01", "Mayfair Mall", "Brookfield Properties", "Wauwatosa", "WI", "53226", "US", "CR", "Charlotte Russe", "Mayfair Mall Russe", "Ste 110", "Spectrum Enterprise", "Verizon 5G"),
                ("MALL-WI02", "Southridge Mall", "Simon Property Group", "Greendale", "WI", "53129", "US", "R21", "rue21", "Southridge rue21", "Ste 214", "Spectrum Enterprise", "AT&T 5G"),

                ("MALL-UT01", "Fashion Place", "Brookfield Properties", "Murray", "UT", "84107", "US", "CR", "Charlotte Russe", "Fashion Place Russe", "Ste 108", "CenturyLink Fiber", "Verizon 5G"),
                ("MALL-UT02", "Station Park", "CenterCal Properties", "Farmington", "UT", "84025", "US", "R21", "rue21", "Station Park rue21", "Ste 114", "Comcast Business", "AT&T 5G"),

                ("MALL-KY01", "Oxmoor Center", "Brookfield Properties", "Louisville", "KY", "40222", "US", "CR", "Charlotte Russe", "Oxmoor Center Russe", "Ste 112", "Spectrum Enterprise", "Verizon 5G"),
                ("MALL-KY02", "Fayette Mall", "CBL Properties", "Lexington", "KY", "40503", "US", "R21", "rue21", "Fayette Mall rue21", "Ste 210", "Spectrum Enterprise", "AT&T 5G"),

                ("MALL-OK01", "Penn Square Mall", "Simon Property Group", "Oklahoma City", "OK", "73118", "US", "CR", "Charlotte Russe", "Penn Square Russe", "Ste 104", "Cox Business", "Verizon 5G"),
                ("MALL-OK02", "Woodland Hills Mall", "Simon Property Group", "Tulsa", "OK", "74133", "US", "R21", "rue21", "Woodland Hills rue21", "Ste 212", "Cox Business", "AT&T 5G"),

                ("MALL-SC01", "Haywood Mall", "Simon Property Group", "Greenville", "SC", "29607", "US", "CR", "Charlotte Russe", "Haywood Mall Russe", "Ste 110", "Spectrum Enterprise", "Verizon 5G"),
                ("MALL-SC02", "Columbiana Centre", "Brookfield Properties", "Columbia", "SC", "29212", "US", "R21", "rue21", "Columbiana rue21", "Ste 214", "Spectrum Enterprise", "AT&T 5G"),

                ("MALL-CT01", "Westfarms", "Taubman Centers", "Farmington", "CT", "06032", "US", "M", "Mandee", "Westfarms Mandee", "Ste 108", "Comcast Business", "Verizon 5G"),
                ("MALL-CT02", "Danbury Fair Mall", "Macerich", "Danbury", "CT", "06810", "US", "CR", "Charlotte Russe", "Danbury Fair Russe", "Ste 210", "Comcast Business", "Verizon 5G"),

                ("MALL-DE01", "Christiana Mall", "Brookfield Properties", "Newark", "DE", "19702", "US", "CR", "Charlotte Russe", "Christiana Mall Russe", "Ste 112", "Comcast Business", "Verizon 5G"),
                ("MALL-DE02", "Christiana Mall", "Brookfield Properties", "Newark", "DE", "19702", "US", "R21", "rue21", "Christiana Mall rue21", "Ste 216", "Comcast Business", "AT&T 5G"),

                ("MALL-PR01", "Plaza Las Americas", "Empresas Fonalledas", "San Juan", "PR", "00918", "US", "CR", "Charlotte Russe", "Plaza Las Americas Russe", "Ste 110", "Liberty Business PR", "Claro 5G"),
                ("MALL-PR02", "Plaza Las Americas", "Empresas Fonalledas", "San Juan", "PR", "00918", "US", "R21", "rue21", "Plaza Las Americas rue21", "Ste 214", "Liberty Business PR", "T-Mobile 5G"),

                // Canadian Regional Hubs (ON, QC, AB, BC, MB, NS)
                ("MALL-ON04", "CF Lime Ridge", "Cadillac Fairview", "Hamilton", "ON", "L9A 4X1", "CA", "BN", "Bluenotes", "Lime Ridge Bluenotes", "Ste 110", "Bell Canada", "Rogers 5G"),
                ("MALL-ON05", "CF Lime Ridge", "Cadillac Fairview", "Hamilton", "ON", "L9A 4X1", "CA", "UP", "Urban Planet", "Lime Ridge Urban Planet", "Ste 214", "Bell Canada", "Rogers 5G"),
                ("MALL-ON06", "CF Fairview Mall", "Cadillac Fairview", "Toronto", "ON", "M2J 5A7", "CA", "BN", "Bluenotes", "Fairview Mall Bluenotes", "Ste 104", "Bell Canada", "Rogers 5G"),
                ("MALL-ON07", "CF Fairview Mall", "Cadillac Fairview", "Toronto", "ON", "M2J 5A7", "CA", "UP", "Urban Planet", "Fairview Mall Urban Planet", "Ste 218", "Bell Canada", "Rogers 5G"),
                ("MALL-ON08", "Scarborough Town Centre", "Oxford Properties", "Toronto", "ON", "M1P 4P5", "CA", "BN", "Bluenotes", "Scarborough Bluenotes", "Ste 112", "Bell Canada", "Rogers 5G"),
                ("MALL-ON09", "Scarborough Town Centre", "Oxford Properties", "Toronto", "ON", "M1P 4P5", "CA", "UP", "Urban Planet", "Scarborough Urban Planet", "Ste 220", "Bell Canada", "Rogers 5G"),
                ("MALL-ON10", "Conestoga Mall", "Ivanhoé Cambridge", "Waterloo", "ON", "N2L 5B6", "CA", "BN", "Bluenotes", "Conestoga Bluenotes", "Ste 108", "Rogers Business", "Bell 5G"),
                ("MALL-ON11", "CF Rideau Centre", "Cadillac Fairview", "Ottawa", "ON", "K1N 9J7", "CA", "BN", "Bluenotes", "Rideau Centre Bluenotes", "Ste 114", "Bell Canada", "Rogers 5G"),
                ("MALL-ON12", "CF Rideau Centre", "Cadillac Fairview", "Ottawa", "ON", "K1N 9J7", "CA", "UP", "Urban Planet", "Rideau Centre Urban Planet", "Ste 304", "Bell Canada", "Rogers 5G"),

                ("MALL-QC01", "CF Carrefour Laval", "Cadillac Fairview", "Laval", "QC", "H7T 1C7", "CA", "UP", "Urban Planet", "Carrefour Laval Urban Planet", "Ste 110", "Videotron Business", "Bell 5G"),
                ("MALL-QC02", "CF Carrefour Laval", "Cadillac Fairview", "Laval", "QC", "H7T 1C7", "CA", "BN", "Bluenotes", "Carrefour Laval Bluenotes", "Ste 214", "Videotron Business", "Rogers 5G"),
                ("MALL-QC03", "CF Promenades St-Bruno", "Cadillac Fairview", "Saint-Bruno", "QC", "J3V 5K4", "CA", "UP", "Urban Planet", "Promenades St-Bruno Urban Planet", "Ste 112", "Bell Canada", "Videotron 5G"),
                ("MALL-QC04", "CF Galeries d'Anjou", "Cadillac Fairview", "Montreal", "QC", "H1M 1W9", "CA", "SF", "Sirens", "Galeries d'Anjou Sirens", "Ste 104", "Videotron Business", "Bell 5G"),
                ("MALL-QC05", "Place Ste-Foy", "Ivanhoé Cambridge", "Quebec City", "QC", "G1V 1W2", "CA", "UP", "Urban Planet", "Place Ste-Foy Urban Planet", "Ste 210", "Bell Canada", "Telus 5G"),

                ("MALL-AB02", "CF Chinook Centre", "Cadillac Fairview", "Calgary", "AB", "T2H 0K8", "CA", "BN", "Bluenotes", "Chinook Centre Bluenotes", "Ste 114", "Telus Business", "Rogers 5G"),
                ("MALL-AB03", "CF Chinook Centre", "Cadillac Fairview", "Calgary", "AB", "T2H 0K8", "CA", "UP", "Urban Planet", "Chinook Centre Urban Planet", "Ste 220", "Telus Business", "Rogers 5G"),
                ("MALL-AB04", "Southgate Centre", "Ivanhoé Cambridge", "Edmonton", "AB", "T6H 4M6", "CA", "BN", "Bluenotes", "Southgate Bluenotes", "Ste 108", "Telus Business", "Shaw 5G"),
                ("MALL-AB05", "CF Market Mall", "Cadillac Fairview", "Calgary", "AB", "T3A 0V7", "CA", "UP", "Urban Planet", "Market Mall Urban Planet", "Ste 110", "Telus Business", "Rogers 5G"),

                ("MALL-BC02", "CF Pacific Centre", "Cadillac Fairview", "Vancouver", "BC", "V7Y 1G5", "CA", "BN", "Bluenotes", "Pacific Centre Bluenotes", "Ste 112", "Telus Business", "Rogers 5G"),
                ("MALL-BC03", "CF Richmond Centre", "Cadillac Fairview", "Richmond", "BC", "V6Y 1S8", "CA", "BN", "Bluenotes", "Richmond Centre Bluenotes", "Ste 104", "Telus Business", "Rogers 5G"),
                ("MALL-BC04", "Guildford Town Centre", "Ivanhoé Cambridge", "Surrey", "BC", "V3R 7C1", "CA", "UP", "Urban Planet", "Guildford Urban Planet", "Ste 210", "Telus Business", "Rogers 5G"),

                ("MALL-MB01", "CF Polo Park", "Cadillac Fairview", "Winnipeg", "MB", "R3G 0W4", "CA", "BN", "Bluenotes", "Polo Park Bluenotes", "Ste 114", "Bell MTS", "Rogers 5G"),
                ("MALL-MB02", "CF Polo Park", "Cadillac Fairview", "Winnipeg", "MB", "R3G 0W4", "CA", "UP", "Urban Planet", "Polo Park Urban Planet", "Ste 220", "Bell MTS", "Rogers 5G"),

                ("MALL-NS01", "Halifax Shopping Centre", "Cushman & Wakefield", "Halifax", "NS", "B3L 2H8", "CA", "BN", "Bluenotes", "Halifax Shopping Bluenotes", "Ste 108", "Bell Aliant", "Rogers 5G"),
                ("MALL-NS02", "Mic Mac Mall", "Rank Inc", "Dartmouth", "NS", "B3A 4K7", "CA", "UP", "Urban Planet", "Mic Mac Mall Urban Planet", "Ste 210", "Bell Aliant", "Rogers 5G")
            };

            int nextNum = 9075;
            foreach (var item in additionalPremierLocations)
            {
                if (nextNum > 9200) break;
                string sNum = nextNum.ToString();
                nextNum++;

                // Check if mall already created in Part 3
                var existingMall = malls.FirstOrDefault(m => m.MallCode.Equals(item.Item1, StringComparison.OrdinalIgnoreCase));
                if (existingMall == null)
                {
                    existingMall = AddMall(
                        item.Item1,
                        $"{item.Item2} (Sandbox)",
                        item.Item3,
                        "Enclosed Regional Mall",
                        $"100 {item.Item2} Way",
                        item.Item4,
                        item.Item5,
                        item.Item6,
                        item.Item7,
                        true,
                        "06:00 - 09:00",
                        20m,
                        item.Item7 == "US" ? "National Low-Voltage Alliance" : "Maple Leaf Telecom",
                        item.Item7 == "US" ? "Tyler Bennett" : "Liam Foster",
                        item.Item7 == "US" ? "Marcus Vance (PM)" : "Terry O'Connor (PS1)",
                        "Standard mall landlord tenant handbook.",
                        "Building main telco shaft.");
                }

                // Add store
                bool isCompleted = (nextNum % 4 != 0);
                int grossSq = item.Item8 == "UP" ? 15500 : 7800;
                int salesSq = (int)(grossSq * 0.83);
                int pos = item.Item8 == "UP" ? 8 : (item.Item8 == "M" ? 4 : 5);

                if (isCompleted)
                {
                    AddStore(sNum, item.Item8, item.Item9, item.Item10, item.Item11, existingMall,
                        "Active Fleet", "Grand Open", "Complete", grossSq, salesSq, pos, "Commissioned",
                        100, "Completed & Certified", "Active fleet store verified.",
                        pos > 6 ? 3 : 2, pos > 6 ? 3 : 2, "Fully Installed & Certified",
                        pos * 4 + 8, true, "Fluke Certified & Closed",
                        item.Item12, "Commercial Fiber DIA", 300, "Tested & Turned Up (Active)",
                        item.Item13, "Enterprise 5G Wireless", 100, "Tested & Turned Up (Active)");
                }
                else if (nextNum % 10 == 0)
                {
                    // Blocked store for realistic variety
                    AddStore(sNum, item.Item8, item.Item9, item.Item10, item.Item11, existingMall,
                        "New Store Opening", "Low-Voltage & Power Poles", "Critical Blocker", grossSq, salesSq, pos, "Pending",
                        45, "Blocked / In Jeopardy", "Landlord COI revision required by mall operations.",
                        2, 1, "Blocked / On Hold",
                        pos * 4 + 8, false, "Blocked",
                        item.Item12, "Commercial Fiber DIA", 300, "Carrier Delay / Jeopardy",
                        item.Item13, "Enterprise 5G Wireless", 100, "Order Placed",
                        42, "Power Pole Install", "Wiremold Drop Pole Night Shift Installation Attempt", "Blocked / Delayed");
                }
                else
                {
                    // In-progress store in Week 42
                    AddStore(sNum, item.Item8, item.Item9, item.Item10, item.Item11, existingMall,
                        "New Store Opening", "IT Staging & Telecom", "On Track", grossSq, salesSq, pos, "Hardware Staged",
                        85, "In Progress / On Schedule", "Telecom TTU scheduled for active week.",
                        2, 2, "Fully Installed & Certified",
                        pos * 4 + 8, true, "Fluke Certified & Closed",
                        item.Item12, "Commercial Fiber DIA", 300, "FOC Confirmed",
                        item.Item13, "Enterprise 5G Wireless", 100, "Tested & Turned Up (Active)",
                        42, "TTU (Test & Turn-Up)", $"Primary DIA Fiber Circuit Turn-Up ({item.Item12})", "In Progress");
                }
            }

            // Fill remaining up to exactly 200 stores if any gap exists
            while (stores.Count < 200)
            {
                int currNum = 9001 + stores.Count;
                string sNum = currNum.ToString();
                var m = malls[stores.Count % malls.Count];

                AddStore(sNum, "CR", "Charlotte Russe", $"Retail Annex {currNum}", $"Suite {currNum % 500}", m,
                    "Active Fleet", "Grand Open", "Complete", 7600, 6300, 5, "Commissioned",
                    100, "Completed & Certified", "Active fleet store verified.",
                    2, 2, "Fully Installed & Certified", 24, true, "Fluke Certified & Closed",
                    "AT&T Fiber", "Commercial Fiber DIA", 300, "Tested & Turned Up (Active)",
                    "Verizon 5G", "Enterprise 5G Wireless", 100, "Tested & Turned Up (Active)");
            }

            return new DummyDatabaseState
            {
                ProjectName = "Project Horizon — Sandbox Rollout DB (200 Stores)",
                LastUpdatedUtc = DateTime.UtcNow,
                Malls = malls,
                Stores = stores,
                WorkItems = workItems
            };
        }
    }
}
