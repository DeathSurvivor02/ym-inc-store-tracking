using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Npgsql;
using YMInc.StoreDeploymentApp.Models;

namespace YMInc.StoreDeploymentApp.Services
{
    public class DatabaseService
    {
        public string ConnectionString { get; set; } = 
            "Host=localhost;Port=5432;Database=ym_store_tracking;Username=ym_admin;Password=ym_dev_secret_2026;Timeout=5;CommandTimeout=10;";

        public bool IsConnected { get; private set; }
        public string StatusMessage { get; private set; } = "Initializing...";

        /// <summary>Details of the most recent failed save, or null if the last save succeeded.</summary>
        public string? LastError { get; private set; }

        public DatabaseService()
        {
            InitializeConnectionString();
        }

        private void InitializeConnectionString()
        {
            try
            {
                string? envUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
                if (string.IsNullOrWhiteSpace(envUrl))
                {
                    string? currentDir = AppDomain.CurrentDomain.BaseDirectory;
                    for (int i = 0; i < 6 && currentDir != null; i++)
                    {
                        string envFile = Path.Combine(currentDir, ".env");
                        if (File.Exists(envFile))
                        {
                            foreach (var line in File.ReadAllLines(envFile))
                            {
                                var trimmed = line.Trim();
                                if (trimmed.StartsWith("DATABASE_URL=") || trimmed.StartsWith("DATABASE_URL_UNPOOLED="))
                                {
                                    var val = trimmed.Substring(trimmed.IndexOf('=') + 1).Trim('"', '\'', ' ');
                                    if (!string.IsNullOrWhiteSpace(val))
                                    {
                                        envUrl = val;
                                        break;
                                    }
                                }
                            }
                            if (!string.IsNullOrWhiteSpace(envUrl)) break;
                        }
                        var parent = Directory.GetParent(currentDir);
                        currentDir = parent?.FullName;
                    }
                }

                if (!string.IsNullOrWhiteSpace(envUrl) && (envUrl.StartsWith("postgresql://") || envUrl.StartsWith("postgres://")))
                {
                    var uri = new Uri(envUrl);
                    var userInfo = uri.UserInfo.Split(':', 2);
                    var builder = new NpgsqlConnectionStringBuilder
                    {
                        Host = uri.Host,
                        Port = uri.Port > 0 ? uri.Port : 5432,
                        Database = uri.AbsolutePath.TrimStart('/'),
                        Username = userInfo.Length > 0 ? Uri.UnescapeDataString(userInfo[0]) : "",
                        Password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "",
                        SslMode = SslMode.Require,
                        Timeout = 10,
                        CommandTimeout = 15

                    };
                    ConnectionString = builder.ConnectionString;
                }
            }
            catch
            {
                // Fall back to default
            }
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                await using var conn = new NpgsqlConnection(ConnectionString);
                await conn.OpenAsync();
                IsConnected = true;
                var builder = new NpgsqlConnectionStringBuilder(ConnectionString);
                StatusMessage = $"Connected: PostgreSQL 16 ({builder.Host}/{builder.Database})";
                return true;
            }
            catch (Exception ex)
            {
                IsConnected = false;
                StatusMessage = $"PostgreSQL Offline ({ex.Message.Split('\n')[0]}). Using Seed Fixtures.";
                return false;
            }
        }

        public async Task<DateTime?> GetLatestStoreUpdateTimestampAsync()
        {
            if (!IsConnected) return null;
            try
            {
                await using var conn = new NpgsqlConnection(ConnectionString);
                await conn.OpenAsync();
                await using var cmd = new NpgsqlCommand("SELECT MAX(updated_at) FROM stores;", conn);
                var result = await cmd.ExecuteScalarAsync();
                if (result != null && result is DateTime dt)
                {
                    return dt;
                }
            }
            catch
            {
                // Non-fatal if offline
            }
            return null;
        }

        public async Task<List<StoreDeployment>> LoadStoresAsync()
        {
            // If PostgreSQL is not reachable, gracefully load seed fixtures
            if (!IsConnected)
            {
                bool connected = await TestConnectionAsync();
                if (!connected)
                {
                    return SeedDataService.GetStores();
                }
            }

            try
            {
                var stores = new List<StoreDeployment>();
                await using var conn = new NpgsqlConnection(ConnectionString);
                await conn.OpenAsync();

                const string sql = @"
                    SELECT 
                        s.store_id, s.store_number, b.banner_code, b.banner_name, s.store_name,
                        s.project_type, s.operational_status, s.overall_health, s.country_code,
                        r.region_name, d.district_name, s.address_line1, s.city, s.state_province,
                        s.postal_code, COALESCE(s.gross_sqft, 0), COALESCE(s.sales_sqft, 0),
                        s.pos_terminal_count, s.pos_staging_status,
                        TO_CHAR(s.target_open_date, 'YYYY-MM-DD') as target_open_date,
                        TO_CHAR(s.opening_date, 'YYYY-MM-DD') as opening_date,
                        s.phone_number,
                        mp.mall_id, mp.mall_code, mp.property_name, mp.landlord_company,
                        sc.full_name as contact_name, sc.email as contact_email, sc.phone_mobile as contact_phone,
                        mp.coi_required, mp.night_work_only, mp.delivery_window, mp.min_general_liability_cad,
                        mp.badging_fee_per_tech, mp.special_cabling_rules, mp.management_contact_name,
                        mp.management_email, mp.management_phone, mp.security_dispatch_phone, mp.address_line1 as mall_address,
                        COALESCE(mp.cabling_vendor, '') as mall_cabling_vendor, COALESCE(mp.lead_technician_name, '') as mall_lead_tech,
                        COALESCE(mp.mall_contacted, false) as mall_contacted, COALESCE(mp.assigned_team_member, '') as assigned_team_member,
                        COALESCE(mp.requirements_circuits, '') as requirements_circuits, COALESCE(mp.intermediate_steps, '') as intermediate_steps
                    FROM stores s
                    JOIN banners b ON s.banner_id = b.banner_id
                    JOIN districts d ON s.district_id = d.district_id
                    JOIN regions r ON d.region_id = r.region_id
                    LEFT JOIN mall_properties mp ON s.mall_id = mp.mall_id
                    LEFT JOIN LATERAL (
                        SELECT full_name, email, phone_mobile 
                        FROM store_contacts 
                        WHERE store_id = s.store_id 
                        ORDER BY (contact_role = 'Landlord Operations Manager') DESC, contact_id ASC 
                        LIMIT 1
                    ) sc ON true
                    ORDER BY s.store_number;";

                await using (var cmd = new NpgsqlCommand(sql, conn))
                await using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        string storeNameVal = reader.GetString(4);
                        if (storeNameVal.Contains(" - "))
                        {
                            storeNameVal = storeNameVal.Substring(0, storeNameVal.IndexOf(" - ")).Trim();
                        }

                        var store = new StoreDeployment
                        {
                            StoreId = reader.GetInt32(0),
                            StoreNumber = reader.GetString(1),
                            BannerCode = reader.GetString(2),
                            BannerName = reader.GetString(3),
                            StoreName = storeNameVal,
                            ProjectType = reader.GetString(5),
                            OperationalStatus = reader.GetString(6),
                            OverallHealth = reader.GetString(7),
                            CountryCode = reader.GetString(8),
                            Region = reader.GetString(9),
                            District = reader.GetString(10),
                            Address = reader.GetString(11),
                            City = reader.GetString(12),
                            StateProvince = reader.GetString(13),
                            PostalCode = reader.GetString(14),
                            GrossSqft = reader.GetInt32(15),
                            SalesSqft = reader.GetInt32(16),
                            PosTerminalCount = reader.GetInt32(17),
                            PosStagingStatus = reader.GetString(18),
                            TargetOpenDate = reader.IsDBNull(19) ? null : reader.GetString(19),
                            OpeningDate = reader.IsDBNull(20) ? null : reader.GetString(20),
                            PhoneNumber = reader.IsDBNull(21) ? null : reader.GetString(21),
                            PrimaryContactName = reader.IsDBNull(26) ? null : reader.GetString(26),
                            PrimaryContactEmail = reader.IsDBNull(27) ? null : reader.GetString(27),
                            PrimaryContactPhone = reader.IsDBNull(28) ? null : reader.GetString(28)
                        };

                        if (!reader.IsDBNull(22))
                        {
                            store.Mall = new MallProperty
                            {
                                MallId = reader.GetInt32(22),
                                MallCode = reader.IsDBNull(23) ? string.Empty : reader.GetString(23),
                                PropertyName = reader.IsDBNull(24) ? string.Empty : reader.GetString(24),
                                LandlordCompany = reader.IsDBNull(25) ? string.Empty : reader.GetString(25),
                                City = store.City,
                                StateProvince = store.StateProvince,
                                CountryCode = store.CountryCode,
                                CoiRequired = !reader.IsDBNull(29) && reader.GetBoolean(29),
                                NightWorkOnly = !reader.IsDBNull(30) && reader.GetBoolean(30),
                                DeliveryWindow = reader.IsDBNull(31) ? "07:00 - 10:00" : reader.GetString(31),
                                MinGeneralLiabilityCad = reader.IsDBNull(32) ? 5000000m : Convert.ToDecimal(reader.GetValue(32)),
                                BadgingFeePerTech = reader.IsDBNull(33) ? 25m : Convert.ToDecimal(reader.GetValue(33)),
                                SpecialRules = reader.IsDBNull(34) ? string.Empty : reader.GetString(34),
                                ManagementContactName = reader.IsDBNull(35) ? string.Empty : reader.GetString(35),
                                ManagementEmail = reader.IsDBNull(36) ? string.Empty : reader.GetString(36),
                                ManagementPhone = reader.IsDBNull(37) ? string.Empty : reader.GetString(37),
                                SecurityPhone = reader.IsDBNull(38) ? string.Empty : reader.GetString(38),
                                Address = reader.IsDBNull(39) ? string.Empty : reader.GetString(39),
                                CablingVendor = reader.IsDBNull(40) ? string.Empty : reader.GetString(40),
                                LeadTechnicianName = reader.IsDBNull(41) ? string.Empty : reader.GetString(41),
                                MallContacted = !reader.IsDBNull(42) && reader.GetBoolean(42),
                                AssignedTeamMember = reader.IsDBNull(43) ? string.Empty : reader.GetString(43),
                                RequirementsCircuits = reader.IsDBNull(44) ? string.Empty : reader.GetString(44),
                                IntermediateSteps = reader.IsDBNull(45) ? string.Empty : reader.GetString(45)
                            };
                        }

                        stores.Add(store);
                    }
                }

                if (stores.Count == 0)
                {
                    IsConnected = false;
                    StatusMessage = "⚠️ Database connected but contains no stores. Showing SEED DATA (read-only).";
                    return SeedDataService.GetStores();
                }

                // Populate technical hardware and circuit specifications for all stores
                await AttachTechnicalSpecsAsync(conn, stores);

                return stores;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DatabaseService.LoadStoresAsync failed: {ex}");
                IsConnected = false;
                StatusMessage = "⚠️ PostgreSQL query failed. Operating in offline seed mode.";
                return SeedDataService.GetStores();
            }
        }

        private async Task AttachTechnicalSpecsAsync(NpgsqlConnection conn, List<StoreDeployment> stores)
        {
            try
            {
                // 1. Site Surveys
                var surveys = new Dictionary<int, SiteSurvey>();
                await using (var cmd = new NpgsqlCommand(@"
                    SELECT store_id, survey_id, COALESCE(survey_vendor, ''), COALESCE(surveyor_name, ''), surveyor_phone, 
                           COALESCE(TO_CHAR(scheduled_date, 'YYYY-MM-DD'), ''), TO_CHAR(actual_survey_date, 'YYYY-MM-DD'), 
                           COALESCE(survey_status, 'Completed & Approved'), COALESCE(ceiling_type, 'Tiled Ceiling'), 
                           COALESCE(ceiling_height_feet, 14.0), COALESCE(demarc_location, 'Mall Demarcation'), 
                           COALESCE(demarc_conduit_available, true), COALESCE(mdf_rack_location, 'Store Backroom Rack'), 
                           COALESCE(mdf_rack_space_adequate, true), COALESCE(dedicated_circuits_count, 2),
                           COALESCE(ceiling_structure_notes, '')
                    FROM site_surveys;", conn))
                {
                    await using var r = await cmd.ExecuteReaderAsync();
                    while (await r.ReadAsync())
                    {
                        int sid = r.GetInt32(0);
                        surveys[sid] = new SiteSurvey
                        {
                            StoreId = sid,
                            SurveyId = r.GetInt32(1),
                            SurveyVendor = r.GetString(2),
                            SurveyorName = r.GetString(3),
                            SurveyorPhone = r.IsDBNull(4) ? null : r.GetString(4),
                            ScheduledDate = r.GetString(5),
                            ActualDate = r.IsDBNull(6) ? null : r.GetString(6),
                            Status = r.GetString(7),
                            CeilingType = r.GetString(8),
                            CeilingHeightFeet = Convert.ToDouble(r.GetValue(9)),
                            DemarcLocation = r.GetString(10),
                            DemarcConduitAvailable = r.GetBoolean(11),
                            MdfRackLocation = r.GetString(12),
                            MdfRackSpaceAdequate = r.GetBoolean(13),
                            DedicatedCircuitsCount = r.GetInt32(14),
                            StructureNotes = r.GetString(15)
                        };
                    }
                }

                // 2. Power Poles
                var poles = new Dictionary<int, PowerPole>();
                await using (var cmd = new NpgsqlCommand(@"
                    SELECT store_id, power_pole_id, COALESCE(pole_requirement_status, 'Passed Electrical Inspection'), 
                           COALESCE(total_poles_required, 2), COALESCE(poles_installed, 2), 
                           COALESCE(pole_manufacturer, 'Wiremold / Legrand'), COALESCE(pole_model_number, '25DTP-4'), 
                           COALESCE(finish_color, 'Anodized Aluminum'), COALESCE(ceiling_mount_type, 'T-Bar Grid Clamp'), 
                           electrical_contractor, COALESCE(cash_wrap_feed_drops, 4), 
                           TO_CHAR(scheduled_install_date, 'YYYY-MM-DD'), TO_CHAR(actual_install_date, 'YYYY-MM-DD'), 
                           blocker_reason
                    FROM power_poles;", conn))
                {
                    await using var r = await cmd.ExecuteReaderAsync();
                    while (await r.ReadAsync())
                    {
                        int sid = r.GetInt32(0);
                        poles[sid] = new PowerPole
                        {
                            StoreId = sid,
                            PowerPoleId = r.GetInt32(1),
                            Status = r.GetString(2),
                            TotalRequired = r.GetInt32(3),
                            InstalledCount = r.GetInt32(4),
                            Manufacturer = r.GetString(5),
                            ModelNumber = r.GetString(6),
                            FinishColor = r.GetString(7),
                            CeilingMountType = r.GetString(8),
                            ElectricalContractor = r.IsDBNull(9) ? null : r.GetString(9),
                            CashWrapFeedDrops = r.GetInt32(10),
                            ScheduledInstallDate = r.IsDBNull(11) ? null : r.GetString(11),
                            ActualInstallDate = r.IsDBNull(12) ? null : r.GetString(12),
                            BlockerReason = r.IsDBNull(13) ? null : r.GetString(13)
                        };
                    }
                }

                // 3. Cabling Operations
                var cabling = new Dictionary<int, CablingOperation>();
                await using (var cmd = new NpgsqlCommand(@"
                    SELECT store_id, cabling_id, COALESCE(cabling_vendor, ''), lead_technician_name, 
                           COALESCE(operational_status, 'Completed & Certified'), COALESCE(cable_grade, 'Cat6 Plenum (CMP)'), 
                           COALESCE(total_data_drops, 24), COALESCE(pos_drops, 4), COALESCE(ap_wireless_drops, 4), 
                           COALESCE(back_office_drops, 4), COALESCE(cctv_camera_drops, 8), 
                           COALESCE(fluke_test_certified, true), 
                           COALESCE(TO_CHAR(scheduled_start_date, 'YYYY-MM-DD'), ''), 
                           COALESCE(TO_CHAR(scheduled_completion_date, 'YYYY-MM-DD'), ''), 
                           TO_CHAR(actual_completion_date, 'YYYY-MM-DD')
                    FROM cabling_operations;", conn))
                {
                    await using var r = await cmd.ExecuteReaderAsync();
                    while (await r.ReadAsync())
                    {
                        int sid = r.GetInt32(0);
                        cabling[sid] = new CablingOperation
                        {
                            StoreId = sid,
                            CablingId = r.GetInt32(1),
                            Vendor = r.GetString(2),
                            LeadTechName = r.IsDBNull(3) ? null : r.GetString(3),
                            Status = r.GetString(4),
                            CableGrade = r.GetString(5),
                            TotalDataDrops = r.GetInt32(6),
                            PosDrops = r.GetInt32(7),
                            ApDrops = r.GetInt32(8),
                            BackOfficeDrops = r.GetInt32(9),
                            CctvDrops = r.GetInt32(10),
                            FlukeCertified = r.GetBoolean(11),
                            ScheduledStartDate = r.GetString(12),
                            ScheduledCompletionDate = r.GetString(13),
                            ActualCompletionDate = r.IsDBNull(14) ? null : r.GetString(14)
                        };
                    }
                }

                // 4. Circuit Details
                var circuits = new Dictionary<int, List<TelecomCircuit>>();
                await using (var cmd = new NpgsqlCommand(@"
                    SELECT store_id, circuit_id, COALESCE(circuit_role, 'Primary WAN'), 
                           COALESCE(carrier_name, ''), COALESCE(service_type, 'Commercial Fiber DIA'), 
                           COALESCE(order_status, 'Tested & Turned Up (Active)'), 
                           COALESCE(bandwidth_down_mbps, 200), COALESCE(bandwidth_up_mbps, 200), 
                           COALESCE(demarc_extended, true), jeopardy_reason, 
                           TO_CHAR(foc_date, 'YYYY-MM-DD'), TO_CHAR(test_turnup_date, 'YYYY-MM-DD')
                    FROM circuit_details
                    ORDER BY circuit_id;", conn))
                {
                    await using var r = await cmd.ExecuteReaderAsync();
                    while (await r.ReadAsync())
                    {
                        int sid = r.GetInt32(0);
                        if (!circuits.ContainsKey(sid)) circuits[sid] = new List<TelecomCircuit>();
                        circuits[sid].Add(new TelecomCircuit
                        {
                            StoreId = sid,
                            CircuitId = r.GetInt32(1),
                            Role = r.GetString(2),
                            CarrierName = r.GetString(3),
                            ServiceType = r.GetString(4),
                            OrderStatus = r.GetString(5),
                            BandwidthDownMbps = r.GetInt32(6),
                            BandwidthUpMbps = r.GetInt32(7),
                            DemarcExtended = r.GetBoolean(8),
                            JeopardyReason = r.IsDBNull(9) ? null : r.GetString(9),
                            FocDate = r.IsDBNull(10) ? null : r.GetString(10),
                            TurnupDate = r.IsDBNull(11) ? null : r.GetString(11)
                        });
                    }
                }

                // Map onto stores
                foreach (var s in stores)
                {
                    if (surveys.TryGetValue(s.StoreId, out var sv)) s.Survey = sv;
                    if (poles.TryGetValue(s.StoreId, out var pp)) s.PowerPoles = pp;
                    if (cabling.TryGetValue(s.StoreId, out var cb)) s.Cabling = cb;
                    if (circuits.TryGetValue(s.StoreId, out var cd)) s.Circuits = cd;

                    if (s.OperationalStatus == "Open" || s.OperationalStatus == "Grand Open")
                    {
                        s.WeeklyStatus = "Completed & Certified";
                        s.WeeklyCompletionPct = 100;
                        s.WeeklySummaryNote = "Store active in fleet. All physical layer hardware and circuits operational.";
                    }
                    else if (s.OverallHealth == "At Risk" || s.OverallHealth == "Critical Blocker")
                    {
                        s.WeeklyStatus = "Blocked / In Jeopardy";
                        s.WeeklyCompletionPct = 45;
                        s.WeeklySummaryNote = s.PowerPoles?.BlockerReason ?? "Site milestone blocked pending landlord or union permit.";
                    }
                    else if (s.OperationalStatus == "IT Staging & Telecom")
                    {
                        s.WeeklyStatus = "In Progress / On Schedule";
                        s.WeeklyCompletionPct = 85;
                        s.WeeklySummaryNote = "Cabling & poles verified. Final WAN circuit turn-up and POS staging.";
                    }
                    else
                    {
                        s.WeeklyStatus = "In Progress / On Schedule";
                        s.WeeklyCompletionPct = 65;
                        s.WeeklySummaryNote = "Wiremold poles delivered, low-voltage cabling pull in progress.";
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error attaching technical specs: {ex.Message}");
            }
        }

        public async Task<List<MallProperty>> LoadMallsAsync()
        {
            if (!IsConnected)
            {
                await TestConnectionAsync();
            }

            try
            {
                var malls = new List<MallProperty>();
                await using var conn = new NpgsqlConnection(ConnectionString);
                await conn.OpenAsync();

                const string sql = @"
                    SELECT 
                        m.mall_id, m.mall_code, m.property_name, m.landlord_company, m.property_type,
                        m.city, m.state_province, m.country_code, m.coi_required,
                        COALESCE(m.min_general_liability_cad, 5000000),
                        COALESCE(m.delivery_window, '07:00 - 10:00'),
                        COALESCE(m.night_work_only, false),
                        COALESCE(m.badging_fee_per_tech, 25),
                        COALESCE(m.special_cabling_rules, ''),
                        COALESCE(m.management_contact_name, ''),
                        COALESCE(m.management_email, ''),
                        COALESCE(m.management_phone, ''),
                        COALESCE(m.security_dispatch_phone, ''),
                        COALESCE(m.address_line1, ''),
                        COALESCE(m.cabling_vendor, ''),
                        COALESCE(m.lead_technician_name, ''),
                        COALESCE(m.mall_contacted, false),
                        COALESCE(m.assigned_team_member, ''),
                        COALESCE(m.requirements_circuits, ''),
                        COALESCE(m.intermediate_steps, '')
                    FROM mall_properties m
                    ORDER BY m.property_name;";

                await using var cmd = new NpgsqlCommand(sql, conn);
                await using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    malls.Add(new MallProperty
                    {
                        MallId = reader.GetInt32(0),
                        MallCode = reader.GetString(1),
                        PropertyName = reader.GetString(2),
                        LandlordCompany = reader.IsDBNull(3) ? "Commercial Landlord" : reader.GetString(3),
                        PropertyType = reader.IsDBNull(4) ? "Enclosed Regional Mall" : reader.GetString(4),
                        City = reader.IsDBNull(5) ? "" : reader.GetString(5),
                        StateProvince = reader.IsDBNull(6) ? "" : reader.GetString(6),
                        CountryCode = reader.IsDBNull(7) ? "US" : reader.GetString(7),
                        CoiRequired = reader.IsDBNull(8) ? true : reader.GetBoolean(8),
                        MinGeneralLiabilityCad = reader.IsDBNull(9) ? 5000000m : Convert.ToDecimal(reader.GetValue(9)),
                        DeliveryWindow = reader.IsDBNull(10) ? "07:00 - 10:00" : reader.GetString(10),
                        NightWorkOnly = reader.IsDBNull(11) ? false : reader.GetBoolean(11),
                        BadgingFeePerTech = reader.IsDBNull(12) ? 25m : Convert.ToDecimal(reader.GetValue(12)),
                        SpecialRules = reader.IsDBNull(13) ? "" : reader.GetString(13),
                        ManagementContactName = reader.IsDBNull(14) ? "" : reader.GetString(14),
                        ManagementEmail = reader.IsDBNull(15) ? "" : reader.GetString(15),
                        ManagementPhone = reader.IsDBNull(16) ? "" : reader.GetString(16),
                        SecurityPhone = reader.IsDBNull(17) ? "" : reader.GetString(17),
                        Address = reader.IsDBNull(18) ? "" : reader.GetString(18),
                        CablingVendor = reader.IsDBNull(19) ? "" : reader.GetString(19),
                        LeadTechnicianName = reader.IsDBNull(20) ? "" : reader.GetString(20),
                        MallContacted = !reader.IsDBNull(21) && reader.GetBoolean(21),
                        AssignedTeamMember = reader.IsDBNull(22) ? "" : reader.GetString(22),
                        RequirementsCircuits = reader.IsDBNull(23) ? "" : reader.GetString(23),
                        IntermediateSteps = reader.IsDBNull(24) ? "" : reader.GetString(24)
                    });
                }

                if (malls.Count == 0)
                {
                    return new List<MallProperty>(SeedDataService.Malls.Values);
                }

                foreach (var m in malls)
                {
                    SeedDataService.Malls[m.MallCode] = m;
                }

                return malls;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadMallsAsync error: {ex}");
                return new List<MallProperty>(SeedDataService.Malls.Values);
            }
        }

        public async Task<bool> SaveMallAsync(MallProperty mall)
        {
            if (!IsConnected)
            {
                LastError = "Not connected to the database (showing seed data). Changes were not saved.";
                return false;
            }

            try
            {
                await using var conn = new NpgsqlConnection(ConnectionString);
                await conn.OpenAsync();
                await using var tx = await conn.BeginTransactionAsync();

                const string sql = @"
                    UPDATE mall_properties
                    SET 
                        property_name = @PropertyName,
                        landlord_company = @LandlordCompany,
                        property_type = @PropertyType,
                        city = @City,
                        state_province = @StateProvince,
                        country_code = @CountryCode,
                        coi_required = @CoiRequired,
                        min_general_liability_cad = @MinLiability,
                        delivery_window = @DeliveryWindow,
                        night_work_only = @NightWorkOnly,
                        badging_fee_per_tech = @BadgingFee,
                        special_cabling_rules = @SpecialRules,
                        management_contact_name = @ContactName,
                        management_email = @ContactEmail,
                        management_phone = @ContactPhone,
                        security_dispatch_phone = @SecurityPhone,
                        address_line1 = @Address,
                        cabling_vendor = @CablingVendor,
                        lead_technician_name = @LeadTechnician,
                        mall_contacted = @MallContacted,
                        has_requirements = @HasRequirements,
                        assigned_team_member = @AssignedTeamMember,
                        requirements_circuits = @ReqCircuits,
                        intermediate_steps = @IntermediateSteps
                    WHERE mall_id = @MallId AND mall_code = @MallCode;";

                await using var cmd = new NpgsqlCommand(sql, conn, tx);
                cmd.Parameters.AddWithValue("MallCode", mall.MallCode);
                cmd.Parameters.AddWithValue("PropertyName", mall.PropertyName);
                cmd.Parameters.AddWithValue("LandlordCompany", mall.LandlordCompany);
                cmd.Parameters.AddWithValue("PropertyType", mall.PropertyType);
                cmd.Parameters.AddWithValue("City", mall.City);
                cmd.Parameters.AddWithValue("StateProvince", mall.StateProvince);
                cmd.Parameters.AddWithValue("CountryCode", mall.CountryCode);
                cmd.Parameters.AddWithValue("CoiRequired", mall.CoiRequired);
                cmd.Parameters.AddWithValue("MinLiability", mall.MinGeneralLiabilityCad);
                cmd.Parameters.AddWithValue("DeliveryWindow", mall.DeliveryWindow);
                cmd.Parameters.AddWithValue("NightWorkOnly", mall.NightWorkOnly);
                cmd.Parameters.AddWithValue("BadgingFee", mall.BadgingFeePerTech);
                cmd.Parameters.AddWithValue("SpecialRules", mall.SpecialRules ?? string.Empty);
                cmd.Parameters.AddWithValue("ContactName", mall.ManagementContactName ?? string.Empty);
                cmd.Parameters.AddWithValue("ContactEmail", mall.ManagementEmail ?? string.Empty);
                cmd.Parameters.AddWithValue("ContactPhone", mall.ManagementPhone ?? string.Empty);
                cmd.Parameters.AddWithValue("SecurityPhone", mall.SecurityPhone ?? string.Empty);
                cmd.Parameters.AddWithValue("Address", mall.Address ?? string.Empty);
                cmd.Parameters.AddWithValue("CablingVendor", mall.CablingVendor ?? string.Empty);
                cmd.Parameters.AddWithValue("LeadTechnician", mall.LeadTechnicianName ?? string.Empty);
                cmd.Parameters.AddWithValue("MallContacted", mall.MallContacted);
                cmd.Parameters.AddWithValue("HasRequirements", mall.HasRequirements);
                cmd.Parameters.AddWithValue("AssignedTeamMember", mall.AssignedTeamMember ?? string.Empty);
                cmd.Parameters.AddWithValue("ReqCircuits", mall.RequirementsCircuits ?? string.Empty);
                cmd.Parameters.AddWithValue("IntermediateSteps", mall.IntermediateSteps ?? string.Empty);
                cmd.Parameters.AddWithValue("MallId", mall.MallId);

                int mallRows = await cmd.ExecuteNonQueryAsync();
                if (mallRows == 0)
                {
                    await tx.RollbackAsync();
                    LastError = $"Mall '{mall.MallCode}' (id {mall.MallId}) does not exist in the database; nothing was saved.";
                    return false;
                }

                const string reqSql = @"
                    UPDATE mall_requirements
                    SET
                        mall_contacted = @MallContacted,
                        assigned_team_member = @AssignedTeamMember,
                        contact_name = @ContactName,
                        email_address = @ContactEmail,
                        phone = @ContactPhone,
                        security_phone = @SecurityPhone,
                        requirements_ttu_cabling = @SpecialRules,
                        requirements_circuits = @ReqCircuits,
                        intermediate_steps = @IntermediateSteps,
                        coi_required = @CoiRequired,
                        night_work_only = @NightWorkOnly,
                        work_hours_restriction = @DeliveryWindow
                    WHERE mall_id = @MallId;";

                await using var reqCmd = new NpgsqlCommand(reqSql, conn, tx);
                reqCmd.Parameters.AddWithValue("MallContacted", mall.MallContacted);
                reqCmd.Parameters.AddWithValue("AssignedTeamMember", mall.AssignedTeamMember ?? string.Empty);
                reqCmd.Parameters.AddWithValue("ContactName", mall.ManagementContactName ?? string.Empty);
                reqCmd.Parameters.AddWithValue("ContactEmail", mall.ManagementEmail ?? string.Empty);
                reqCmd.Parameters.AddWithValue("ContactPhone", mall.ManagementPhone ?? string.Empty);
                reqCmd.Parameters.AddWithValue("SecurityPhone", mall.SecurityPhone ?? string.Empty);
                reqCmd.Parameters.AddWithValue("SpecialRules", mall.SpecialRules ?? string.Empty);
                reqCmd.Parameters.AddWithValue("ReqCircuits", mall.RequirementsCircuits ?? string.Empty);
                reqCmd.Parameters.AddWithValue("IntermediateSteps", mall.IntermediateSteps ?? string.Empty);
                reqCmd.Parameters.AddWithValue("CoiRequired", mall.CoiRequired);
                reqCmd.Parameters.AddWithValue("NightWorkOnly", mall.NightWorkOnly);
                reqCmd.Parameters.AddWithValue("DeliveryWindow", mall.DeliveryWindow);
                reqCmd.Parameters.AddWithValue("MallId", mall.MallId);

                await reqCmd.ExecuteNonQueryAsync();

                await tx.CommitAsync();
                LastError = null;
                return true;
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                System.Diagnostics.Debug.WriteLine($"SaveMallAsync error: {ex}");
                return false;
            }
        }

        public async Task<bool> SaveStoreAsync(StoreDeployment store)
        {
            if (!IsConnected)
            {
                LastError = "Not connected to the database (showing seed data). Changes were not saved.";
                return false;
            }

            try
            {
                await using var conn = new NpgsqlConnection(ConnectionString);
                await conn.OpenAsync();

                const string sql = @"
                    UPDATE stores
                    SET 
                        store_name = CASE 
                            WHEN btrim(split_part(store_name, ' - ', 1)) = @StoreName THEN store_name 
                            ELSE @StoreName END,
                        operational_status = @OperationalStatus,
                        overall_health = @OverallHealth,
                        target_open_date = CASE WHEN @TargetDate = '' THEN NULL ELSE @TargetDate::DATE END
                    WHERE store_id = @StoreId;";

                await using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("StoreName", store.StoreName);
                cmd.Parameters.AddWithValue("OperationalStatus", store.OperationalStatus);
                cmd.Parameters.AddWithValue("OverallHealth", store.OverallHealth);
                cmd.Parameters.AddWithValue("TargetDate", store.TargetOpenDate ?? string.Empty);
                cmd.Parameters.AddWithValue("StoreId", store.StoreId);

                int rows = await cmd.ExecuteNonQueryAsync();
                if (rows == 0)
                {
                    LastError = $"Store #{store.StoreNumber} was not found in the database; nothing was saved.";
                    return false;
                }

                if (store.Survey != null)
                {
                    const string surveySql = @"
                        UPDATE site_surveys
                        SET 
                            ceiling_type = @CeilingType,
                            ceiling_height_feet = @CeilingHeight,
                            survey_status = @SurveyStatus,
                            demarc_location = @DemarcLocation,
                            demarc_conduit_available = @DemarcConduit,
                            mdf_rack_location = @MdfRack,
                            surveyor_name = @SurveyorName,
                            ceiling_structure_notes = @Notes
                        WHERE store_id = @StoreId;";
                    await using var sCmd = new NpgsqlCommand(surveySql, conn);
                    sCmd.Parameters.AddWithValue("CeilingType", store.Survey.CeilingType ?? string.Empty);
                    sCmd.Parameters.AddWithValue("CeilingHeight", Convert.ToDecimal(store.Survey.CeilingHeightFeet));
                    sCmd.Parameters.AddWithValue("SurveyStatus", store.Survey.Status ?? string.Empty);
                    sCmd.Parameters.AddWithValue("DemarcLocation", store.Survey.DemarcLocation ?? string.Empty);
                    sCmd.Parameters.AddWithValue("DemarcConduit", store.Survey.DemarcConduitAvailable);
                    sCmd.Parameters.AddWithValue("MdfRack", store.Survey.MdfRackLocation ?? string.Empty);
                    sCmd.Parameters.AddWithValue("SurveyorName", store.Survey.SurveyorName ?? string.Empty);
                    sCmd.Parameters.AddWithValue("Notes", store.Survey.StructureNotes ?? string.Empty);
                    sCmd.Parameters.AddWithValue("StoreId", store.StoreId);
                    await sCmd.ExecuteNonQueryAsync();
                }

                if (store.PowerPoles != null)
                {
                    const string poleSql = @"
                        UPDATE power_poles
                        SET 
                            pole_requirement_status = @PoleStatus,
                            total_poles_required = @TotalPoles,
                            poles_installed = @InstalledPoles,
                            finish_color = @FinishColor,
                            ceiling_mount_type = @MountType,
                            cash_wrap_feed_drops = @FeedDrops,
                            electrical_contractor = @Contractor,
                            blocker_reason = @Blocker
                        WHERE store_id = @StoreId;";
                    await using var pCmd = new NpgsqlCommand(poleSql, conn);
                    pCmd.Parameters.AddWithValue("PoleStatus", store.PowerPoles.Status ?? string.Empty);
                    pCmd.Parameters.AddWithValue("TotalPoles", store.PowerPoles.TotalRequired);
                    pCmd.Parameters.AddWithValue("InstalledPoles", store.PowerPoles.InstalledCount);
                    pCmd.Parameters.AddWithValue("FinishColor", store.PowerPoles.FinishColor ?? string.Empty);
                    pCmd.Parameters.AddWithValue("MountType", store.PowerPoles.CeilingMountType ?? string.Empty);
                    pCmd.Parameters.AddWithValue("FeedDrops", store.PowerPoles.CashWrapFeedDrops);
                    pCmd.Parameters.AddWithValue("Contractor", store.PowerPoles.ElectricalContractor ?? string.Empty);
                    pCmd.Parameters.AddWithValue("Blocker", (object?)store.PowerPoles.BlockerReason ?? DBNull.Value);
                    pCmd.Parameters.AddWithValue("StoreId", store.StoreId);
                    await pCmd.ExecuteNonQueryAsync();
                }

                if (store.Cabling != null)
                {
                    const string cableSql = @"
                        UPDATE cabling_operations
                        SET 
                            operational_status = @CablingStatus,
                            total_data_drops = @TotalDrops,
                            pos_drops = @PosDrops,
                            ap_wireless_drops = @ApDrops,
                            fluke_test_certified = @Fluke,
                            cabling_vendor = @Vendor,
                            lead_technician_name = @Tech
                        WHERE store_id = @StoreId;";
                    await using var cCmd = new NpgsqlCommand(cableSql, conn);
                    cCmd.Parameters.AddWithValue("CablingStatus", store.Cabling.Status ?? string.Empty);
                    cCmd.Parameters.AddWithValue("TotalDrops", store.Cabling.TotalDataDrops);
                    cCmd.Parameters.AddWithValue("PosDrops", store.Cabling.PosDrops);
                    cCmd.Parameters.AddWithValue("ApDrops", store.Cabling.ApDrops);
                    cCmd.Parameters.AddWithValue("Fluke", store.Cabling.FlukeCertified);
                    cCmd.Parameters.AddWithValue("Vendor", store.Cabling.Vendor ?? string.Empty);
                    cCmd.Parameters.AddWithValue("Tech", (object?)store.Cabling.LeadTechName ?? DBNull.Value);
                    cCmd.Parameters.AddWithValue("StoreId", store.StoreId);
                    await cCmd.ExecuteNonQueryAsync();
                }

                LastError = null;
                return true;
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                System.Diagnostics.Debug.WriteLine($"SaveStoreAsync error: {ex}");
                return false;
            }
        }

        public async Task<bool> CreateStoreAsync(StoreDeployment store)
        {
            if (!IsConnected)
            {
                int newId = SeedDataService.GetStores().Count > 0 ? SeedDataService.GetStores().Max(s => s.StoreId) + 1 : 100;
                store.StoreId = newId;
                SeedDataService.GetStores().Add(store);
                LastError = null;
                return true;
            }

            try
            {
                await using var conn = new NpgsqlConnection(ConnectionString);
                await conn.OpenAsync();
                await using var tx = await conn.BeginTransactionAsync();

                // 1. Resolve banner_id
                int bannerId = 1;
                await using (var bCmd = new NpgsqlCommand("SELECT banner_id FROM banners WHERE banner_code = @Code OR banner_name = @Name LIMIT 1;", conn, tx))
                {
                    bCmd.Parameters.AddWithValue("Code", store.BannerCode ?? "CR");
                    bCmd.Parameters.AddWithValue("Name", store.BannerName ?? "Charlotte Russe");
                    var bRes = await bCmd.ExecuteScalarAsync();
                    if (bRes != null && bRes is short or int)
                    {
                        bannerId = Convert.ToInt32(bRes);
                    }
                }

                // 2. Resolve district_id
                int districtId = 1;
                await using (var dCmd = new NpgsqlCommand("SELECT district_id FROM districts LIMIT 1;", conn, tx))
                {
                    var dRes = await dCmd.ExecuteScalarAsync();
                    if (dRes != null && dRes is short or int)
                    {
                        districtId = Convert.ToInt32(dRes);
                    }
                }

                // 3. Resolve or insert mall_id
                int? mallId = null;
                if (store.Mall != null && !string.IsNullOrWhiteSpace(store.Mall.PropertyName))
                {
                    await using (var mCmd = new NpgsqlCommand("SELECT mall_id FROM mall_properties WHERE property_name = @Name LIMIT 1;", conn, tx))
                    {
                        mCmd.Parameters.AddWithValue("Name", store.Mall.PropertyName);
                        var mRes = await mCmd.ExecuteScalarAsync();
                        if (mRes != null && mRes is int id)
                        {
                            mallId = id;
                        }
                    }

                    if (!mallId.HasValue)
                    {
                        const string insMallSql = @"
                            INSERT INTO mall_properties (
                                mall_code, property_name, landlord_company, property_type,
                                address_line1, city, state_province, postal_code, country_code,
                                coi_required, delivery_window, night_work_only
                            ) VALUES (
                                @MallCode, @PropertyName, @Landlord, 'Enclosed Regional Mall',
                                @Address, @City, @State, @Postal, @Country,
                                true, '06:00 - 09:30', false
                            ) RETURNING mall_id;";
                        await using var insMallCmd = new NpgsqlCommand(insMallSql, conn, tx);
                        insMallCmd.Parameters.AddWithValue("MallCode", string.IsNullOrWhiteSpace(store.Mall.MallCode) ? $"M-{store.StoreNumber}" : store.Mall.MallCode);
                        insMallCmd.Parameters.AddWithValue("PropertyName", store.Mall.PropertyName);
                        insMallCmd.Parameters.AddWithValue("Landlord", string.IsNullOrWhiteSpace(store.Mall.LandlordCompany) ? "Cadillac Fairview / Oxford Properties" : store.Mall.LandlordCompany);
                        insMallCmd.Parameters.AddWithValue("Address", string.IsNullOrWhiteSpace(store.Address) ? "100 Retail Way" : store.Address);
                        insMallCmd.Parameters.AddWithValue("City", string.IsNullOrWhiteSpace(store.City) ? "Toronto" : store.City);
                        insMallCmd.Parameters.AddWithValue("State", string.IsNullOrWhiteSpace(store.StateProvince) ? "ON" : store.StateProvince);
                        insMallCmd.Parameters.AddWithValue("Postal", string.IsNullOrWhiteSpace(store.PostalCode) ? "M5H 2N2" : store.PostalCode);
                        insMallCmd.Parameters.AddWithValue("Country", string.IsNullOrWhiteSpace(store.CountryCode) ? "CA" : store.CountryCode);
                        var newMallIdRes = await insMallCmd.ExecuteScalarAsync();
                        if (newMallIdRes != null && newMallIdRes is int nmid)
                        {
                            mallId = nmid;
                        }
                    }
                }

                // 4. Insert into stores
                const string insStoreSql = @"
                    INSERT INTO stores (
                        store_number, banner_id, district_id, mall_id, store_name,
                        project_type, operational_status, overall_health, store_status,
                        address_line1, city, state_province, postal_code, country_code,
                        phone_number, pos_terminal_count, opening_date, target_open_date, notes
                    ) VALUES (
                        @StoreNumber, @BannerId, @DistrictId, @MallId, @StoreName,
                        @ProjectType, @OperationalStatus, @OverallHealth, 'Active',
                        @Address, @City, @State, @Postal, @Country,
                        @Phone, @PosTerminals, 
                        COALESCE(CASE WHEN @TargetDate = '' THEN NULL ELSE @TargetDate::DATE END, CURRENT_DATE + INTERVAL '30 days'),
                        CASE WHEN @TargetDate = '' THEN NULL ELSE @TargetDate::DATE END,
                        @Notes
                    ) RETURNING store_id;";

                await using var insCmd = new NpgsqlCommand(insStoreSql, conn, tx);
                insCmd.Parameters.AddWithValue("StoreNumber", store.StoreNumber);
                insCmd.Parameters.AddWithValue("BannerId", bannerId);
                insCmd.Parameters.AddWithValue("DistrictId", districtId);
                insCmd.Parameters.AddWithValue("MallId", (object?)mallId ?? DBNull.Value);
                insCmd.Parameters.AddWithValue("StoreName", store.StoreName);
                insCmd.Parameters.AddWithValue("ProjectType", string.IsNullOrWhiteSpace(store.ProjectType) ? "New Store Opening" : store.ProjectType);
                insCmd.Parameters.AddWithValue("OperationalStatus", string.IsNullOrWhiteSpace(store.OperationalStatus) ? "Planning" : store.OperationalStatus);
                insCmd.Parameters.AddWithValue("OverallHealth", string.IsNullOrWhiteSpace(store.OverallHealth) ? "On Track" : store.OverallHealth);
                insCmd.Parameters.AddWithValue("Address", string.IsNullOrWhiteSpace(store.Address) ? "100 Retail Blvd" : store.Address);
                insCmd.Parameters.AddWithValue("City", string.IsNullOrWhiteSpace(store.City) ? "Toronto" : store.City);
                insCmd.Parameters.AddWithValue("State", string.IsNullOrWhiteSpace(store.StateProvince) ? "ON" : store.StateProvince);
                insCmd.Parameters.AddWithValue("Postal", string.IsNullOrWhiteSpace(store.PostalCode) ? "M5H 2N2" : store.PostalCode);
                insCmd.Parameters.AddWithValue("Country", string.IsNullOrWhiteSpace(store.CountryCode) ? "CA" : store.CountryCode);
                insCmd.Parameters.AddWithValue("Phone", (object?)store.PhoneNumber ?? DBNull.Value);
                insCmd.Parameters.AddWithValue("PosTerminals", store.PosTerminalCount > 0 ? store.PosTerminalCount : 4);
                insCmd.Parameters.AddWithValue("TargetDate", store.TargetOpenDate ?? string.Empty);
                insCmd.Parameters.AddWithValue("Notes", (object?)store.TechnicalSummaryNote ?? DBNull.Value);

                var newStoreIdObj = await insCmd.ExecuteScalarAsync();
                int storeId = Convert.ToInt32(newStoreIdObj);
                store.StoreId = storeId;

                // 5. Insert child records: survey, power poles, cabling, telecom circuits
                const string insSurveySql = @"
                    INSERT INTO site_surveys (store_id, surveyor_name, survey_status, scheduled_date, ceiling_type, ceiling_height_feet, demarc_location, demarc_conduit_available, mdf_rack_location)
                    VALUES (@StoreId, @Surveyor, 'Scheduled', CURRENT_DATE + INTERVAL '5 days', 'Acoustic Drop Tile', 12.0, 'Back Office', true, 'Cash Wrap');";
                await using var sCmd = new NpgsqlCommand(insSurveySql, conn, tx);
                sCmd.Parameters.AddWithValue("StoreId", storeId);
                sCmd.Parameters.AddWithValue("Surveyor", string.IsNullOrWhiteSpace(store.Survey?.SurveyorName) ? "National Site Surveyors" : store.Survey.SurveyorName);
                await sCmd.ExecuteNonQueryAsync();

                const string insPoleSql = @"
                    INSERT INTO power_poles (store_id, pole_requirement_status, total_poles_required, poles_installed, finish_color, ceiling_mount_type, cash_wrap_feed_drops)
                    VALUES (@StoreId, @Status, @Total, @Installed, 'Anodized Aluminum', 'T-Bar Grid Clamp', 4);";
                await using var pCmd = new NpgsqlCommand(insPoleSql, conn, tx);
                pCmd.Parameters.AddWithValue("StoreId", storeId);
                pCmd.Parameters.AddWithValue("Status", store.PowerPoles?.Status ?? "Survey Required");
                pCmd.Parameters.AddWithValue("Total", store.PowerPoles?.TotalRequired > 0 ? store.PowerPoles.TotalRequired : 2);
                pCmd.Parameters.AddWithValue("Installed", store.PowerPoles?.InstalledCount ?? 0);
                await pCmd.ExecuteNonQueryAsync();

                const string insCablingSql = @"
                    INSERT INTO cabling_operations (store_id, operational_status, total_data_drops, pos_drops, ap_wireless_drops, fluke_test_certified, cabling_vendor)
                    VALUES (@StoreId, @Status, @TotalDrops, @PosDrops, 4, false, @Vendor);";
                await using var cCmd = new NpgsqlCommand(insCablingSql, conn, tx);
                cCmd.Parameters.AddWithValue("StoreId", storeId);
                cCmd.Parameters.AddWithValue("Status", store.Cabling?.Status ?? "Vendor Assigned");
                cCmd.Parameters.AddWithValue("TotalDrops", store.Cabling?.TotalDataDrops > 0 ? store.Cabling.TotalDataDrops : 16);
                cCmd.Parameters.AddWithValue("PosDrops", store.Cabling?.PosDrops > 0 ? store.Cabling.PosDrops : 4);
                cCmd.Parameters.AddWithValue("Vendor", string.IsNullOrWhiteSpace(store.Cabling?.Vendor) ? "Black Box Network Services" : store.Cabling.Vendor);
                await cCmd.ExecuteNonQueryAsync();

                if (store.Circuits.Count > 0)
                {
                    foreach (var c in store.Circuits)
                    {
                        const string insCircSql = @"
                            INSERT INTO circuit_details (store_id, circuit_role, carrier_name, service_type, order_status, bandwidth_down_mbps, bandwidth_up_mbps)
                            VALUES (@StoreId, @Role, @Carrier, @Service, @Status, @Down, @Up);";
                        await using var circCmd = new NpgsqlCommand(insCircSql, conn, tx);
                        circCmd.Parameters.AddWithValue("StoreId", storeId);
                        circCmd.Parameters.AddWithValue("Role", c.Role ?? "Primary WAN");
                        circCmd.Parameters.AddWithValue("Carrier", string.IsNullOrWhiteSpace(c.CarrierName) ? "Bell Canada" : c.CarrierName);
                        circCmd.Parameters.AddWithValue("Service", string.IsNullOrWhiteSpace(c.ServiceType) ? "Commercial Fiber DIA" : c.ServiceType);
                        circCmd.Parameters.AddWithValue("Status", string.IsNullOrWhiteSpace(c.OrderStatus) ? "Order Placed" : c.OrderStatus);
                        circCmd.Parameters.AddWithValue("Down", c.BandwidthDownMbps > 0 ? c.BandwidthDownMbps : 100);
                        circCmd.Parameters.AddWithValue("Up", c.BandwidthUpMbps > 0 ? c.BandwidthUpMbps : 100);
                        await circCmd.ExecuteNonQueryAsync();
                    }
                }
                else
                {
                    const string insCircSql = @"
                        INSERT INTO circuit_details (store_id, circuit_role, carrier_name, service_type, order_status, bandwidth_down_mbps, bandwidth_up_mbps)
                        VALUES (@StoreId, 'Primary WAN', 'Bell Canada', 'Commercial Fiber DIA', 'Order Placed', 100, 100);";
                    await using var circCmd = new NpgsqlCommand(insCircSql, conn, tx);
                    circCmd.Parameters.AddWithValue("StoreId", storeId);
                    await circCmd.ExecuteNonQueryAsync();
                }

                await tx.CommitAsync();
                LastError = null;
                return true;
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                System.Diagnostics.Debug.WriteLine($"CreateStoreAsync error: {ex}");
                return false;
            }
        }

        private static readonly string LiveCalendarFilePath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "live_calendar_work_items.json");

        public async Task<List<CalendarWorkItem>> LoadCalendarWorkItemsAsync(List<StoreDeployment>? fleetStores = null)
        {
            await Task.CompletedTask;
            List<CalendarWorkItem> items = new();
            try
            {
                if (File.Exists(LiveCalendarFilePath))
                {
                    var loaded = JsonSerializer.Deserialize<List<CalendarWorkItem>>(File.ReadAllText(LiveCalendarFilePath));
                    if (loaded != null && loaded.Count > 0)
                    {
                        items = loaded;
                    }
                }
            }
            catch
            {
                // Ignore read issues
            }

            if (items.Count == 0)
            {
                items = CreateInitialLiveCalendarItems(fleetStores);
                SaveLiveCalendarItemsToDisk(items);
            }

            return items.OrderBy(w => w.ActivityDate).ThenBy(w => w.TimeWindow).ToList();
        }

        public async Task SaveCalendarWorkItemAsync(CalendarWorkItem item)
        {
            item.FiscalWeek = GetFiscalWeekForDate(item.ActivityDate);
            item.FiscalYear = GetFiscalYearForDate(item.ActivityDate);

            var items = await LoadCalendarWorkItemsAsync();
            var existing = items.FirstOrDefault(w => w.WorkItemId == item.WorkItemId);
            if (existing != null)
            {
                int idx = items.IndexOf(existing);
                items[idx] = item;
            }
            else
            {
                items.Add(item);
            }
            SaveLiveCalendarItemsToDisk(items);
        }

        public async Task DeleteCalendarWorkItemAsync(string workItemId)
        {
            var items = await LoadCalendarWorkItemsAsync();
            items.RemoveAll(w => w.WorkItemId == workItemId);
            SaveLiveCalendarItemsToDisk(items);
        }

        private static void SaveLiveCalendarItemsToDisk(List<CalendarWorkItem> items)
        {
            try
            {
                _lastSelfCalendarSaveUtc = DateTime.UtcNow;
                var opts = new JsonSerializerOptions { WriteIndented = true };
                File.WriteAllText(LiveCalendarFilePath, JsonSerializer.Serialize(items, opts));
            }
            catch
            {
                // Ignore IO lock
            }
        }

        private static FileSystemWatcher? _calendarWatcher;
        private static DateTime _lastSelfCalendarSaveUtc = DateTime.MinValue;
        public static event Action? CalendarChangedExternally;

        public static void StartCalendarWatcher()
        {
            if (_calendarWatcher != null) return;
            try
            {
                string dir = Path.GetDirectoryName(LiveCalendarFilePath) ?? AppDomain.CurrentDomain.BaseDirectory;
                string file = Path.GetFileName(LiveCalendarFilePath);
                _calendarWatcher = new FileSystemWatcher(dir, file)
                {
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName,
                    EnableRaisingEvents = true
                };
                _calendarWatcher.Changed += (s, e) =>
                {
                    if ((DateTime.UtcNow - _lastSelfCalendarSaveUtc).TotalMilliseconds < 1500)
                        return;
                    System.Threading.Thread.Sleep(200);
                    CalendarChangedExternally?.Invoke();
                };
            }
            catch { }
        }

        public static int GetFiscalWeekForDate(DateTime date)
        {
            var wk41Start = new DateTime(2026, 10, 11);
            int diffDays = (int)Math.Floor((date.Date - wk41Start).TotalDays);
            int week = 41 + (int)Math.Floor(diffDays / 7.0);
            if (week < 1) week = ((week % 52) + 52) % 52;
            if (week == 0) week = 52;
            if (week > 52) week = ((week - 1) % 52) + 1;
            return week;
        }

        public static int GetFiscalYearForDate(DateTime date)
        {
            if (date.Month == 1 && date.Day < 4) return date.Year - 1;
            return date.Year;
        }

        public static (DateTime Start, DateTime End) GetDateRangeForFiscalWeek(int fiscalWeek, int fiscalYear = 2026)
        {
            var wk41Start = new DateTime(2026, 10, 11);
            var start = wk41Start.AddDays((fiscalWeek - 41) * 7);
            return (start, start.AddDays(6));
        }

        private static List<CalendarWorkItem> CreateInitialLiveCalendarItems(List<StoreDeployment>? stores)
        {
            var list = new List<CalendarWorkItem>
            {
                // Historical Progression Weeks 37..40 + Current Weeks 41..43
                new() { WorkItemId = "LIVE-3701", ActivityDate = new DateTime(2026, 9, 15), FiscalYear = 2026, FiscalWeek = 37, ActivityType = "Site Survey", Title = "CompuCom Field Survey & Demarc Audit", StoreNumber = "6142", StoreName = "Bluenotes Flagship", BannerCode = "BN", Status = "Completed", AssignedTo = "Terry O'Connor (PS1)", TimeWindow = "08:00 - 11:30", Notes = "Verified 14.5ft acoustic ceiling & Common Demarc B1-14." },
                new() { WorkItemId = "LIVE-3702", ActivityDate = new DateTime(2026, 9, 17), FiscalYear = 2026, FiscalWeek = 37, ActivityType = "Site Survey", Title = "Woodbridge Center Telco Riser Survey", StoreNumber = "6012", StoreName = "rue21", BannerCode = "R21", Status = "Completed", AssignedTo = "Danielle Levy (PS2)", TimeWindow = "09:00 - 12:00", Notes = "Basement Telco Room 02 conduit clear." },

                new() { WorkItemId = "LIVE-3801", ActivityDate = new DateTime(2026, 9, 21), FiscalYear = 2026, FiscalWeek = 38, ActivityType = "Site Survey", Title = "Tri-State Telecom MDF Rack Approval", StoreNumber = "6012", StoreName = "rue21", BannerCode = "R21", Status = "Completed", AssignedTo = "Danielle Levy (PS2)", TimeWindow = "10:00 - 13:00", Notes = "Cash wrap closet rack approved." },
                new() { WorkItemId = "LIVE-3802", ActivityDate = new DateTime(2026, 9, 24), FiscalYear = 2026, FiscalWeek = 38, ActivityType = "Low-Voltage Cabling", Title = "Initial Pathway & J-Hook Installation", StoreNumber = "6142", StoreName = "Bluenotes Flagship", BannerCode = "BN", Status = "Completed", AssignedTo = "Terry O'Connor (PS1)", TimeWindow = "06:00 - 11:00", Notes = "Black Box crew installed ceiling J-hooks." },

                new() { WorkItemId = "LIVE-3901", ActivityDate = new DateTime(2026, 10, 1), FiscalYear = 2026, FiscalWeek = 39, ActivityType = "Site Survey", Title = "Sawgrass Mills Demarc Riser C Survey", StoreNumber = "6004", StoreName = "Charlotte Russe", BannerCode = "CR", Status = "Completed", AssignedTo = "Hector Ruiz (PS3)", TimeWindow = "08:00 - 11:00", Notes = "15ft ceiling verified; 3 drop poles required." },
                new() { WorkItemId = "LIVE-3902", ActivityDate = new DateTime(2026, 10, 3), FiscalYear = 2026, FiscalWeek = 39, ActivityType = "Circuit Install", Title = "Comcast 200M Fiber Conduit Pull", StoreNumber = "6012", StoreName = "rue21", BannerCode = "R21", Status = "Completed", AssignedTo = "Danielle Levy (PS2)", TimeWindow = "07:00 - 12:00", Notes = "Fiber extended from Basement Room 02 to IDF." },

                new() { WorkItemId = "LIVE-4001", ActivityDate = new DateTime(2026, 10, 6), FiscalYear = 2026, FiscalWeek = 40, ActivityType = "Power Pole Install", Title = "Wiremold 25DTP-4 Delivery & Staging", StoreNumber = "6004", StoreName = "Charlotte Russe", BannerCode = "CR", Status = "Completed", AssignedTo = "Hector Ruiz (PS3)", TimeWindow = "07:00 - 10:00", Notes = "3 anodized poles staged in backroom." },
                new() { WorkItemId = "LIVE-4002", ActivityDate = new DateTime(2026, 10, 9), FiscalYear = 2026, FiscalWeek = 40, ActivityType = "Low-Voltage Cabling", Title = "24-Drop Cat6 Plenum Pull — Woodbridge", StoreNumber = "6012", StoreName = "rue21", BannerCode = "R21", Status = "Completed", AssignedTo = "David Vance", TimeWindow = "07:00 - 15:00", Notes = "Direct Line Cabling completed rough-in." },

                // WEEK 41 (Oct 11 - Oct 17, 2026)
                new() { WorkItemId = "LIVE-4101", ActivityDate = new DateTime(2026, 10, 12), FiscalYear = 2026, FiscalWeek = 41, ActivityType = "TTU (Test & Turn-Up)", Title = "AT&T Secondary 5G Failover Turn-Up", StoreNumber = "6012", StoreName = "rue21", BannerCode = "R21", Status = "Completed", AssignedTo = "Danielle Levy (PS2)", TimeWindow = "09:00 - 11:00", Notes = "Failover WAN active." },
                new() { WorkItemId = "LIVE-4102", ActivityDate = new DateTime(2026, 10, 14), FiscalYear = 2026, FiscalWeek = 41, ActivityType = "Circuit Install", Title = "Telus 5G Enterprise Wireless Mount", StoreNumber = "6142", StoreName = "Bluenotes Flagship", BannerCode = "BN", Status = "Completed", AssignedTo = "Terry O'Connor (PS1)", TimeWindow = "08:00 - 10:30", Notes = "Mounted high-gain antenna in back office." },
                new() { WorkItemId = "LIVE-4103", ActivityDate = new DateTime(2026, 10, 16), FiscalYear = 2026, FiscalWeek = 41, ActivityType = "Site Survey", Title = "Metrotown 18ft Open Plenum Structural Survey", StoreNumber = "6105", StoreName = "Urban Planet", BannerCode = "UP", Status = "Completed", AssignedTo = "Claire Tremblay (PA)", TimeWindow = "06:30 - 09:30", Notes = "Approved structural beam clamps for 4 poles." },
                new() { WorkItemId = "LIVE-4104", ActivityDate = new DateTime(2026, 10, 17), FiscalYear = 2026, FiscalWeek = 41, ActivityType = "Low-Voltage Cabling", Title = "Fluke DSX Certification — 24 Cat6 Drops", StoreNumber = "6012", StoreName = "rue21", BannerCode = "R21", Status = "Completed", AssignedTo = "Danielle Levy (PS2)", TimeWindow = "08:00 - 13:00", Notes = "All 24 drops passed certification." },

                // WEEK 42 (Oct 18 - Oct 24, 2026 — CURRENT WEEK)
                new() { WorkItemId = "LIVE-4201", ActivityDate = new DateTime(2026, 10, 18), FiscalYear = 2026, FiscalWeek = 42, ActivityType = "Power Pole Install", Title = "2x Wiremold 25DTP-4 Final Electrical Sign-Off", StoreNumber = "6012", StoreName = "rue21", BannerCode = "R21", Status = "Completed", AssignedTo = "Danielle Levy (PS2)", TimeWindow = "07:00 - 10:00", Notes = "Tri-County Electric passed final inspection." },
                new() { WorkItemId = "LIVE-4202", ActivityDate = new DateTime(2026, 10, 18), FiscalYear = 2026, FiscalWeek = 42, ActivityType = "TTU (Test & Turn-Up)", Title = "Comcast 200M Primary Fiber DIA TTU", StoreNumber = "6012", StoreName = "rue21", BannerCode = "R21", Status = "Completed", AssignedTo = "Marcus Vance (PM)", TimeWindow = "11:00 - 13:30", Notes = "Primary WAN live and stress-tested." },
                new() { WorkItemId = "LIVE-4203", ActivityDate = new DateTime(2026, 10, 19), FiscalYear = 2026, FiscalWeek = 42, ActivityType = "TTU (Test & Turn-Up)", Title = "Telus Secondary Failover WAN Turn-Up", StoreNumber = "6142", StoreName = "Bluenotes Flagship", BannerCode = "BN", Status = "Completed", AssignedTo = "Terry O'Connor (PS1)", TimeWindow = "09:00 - 11:00", Notes = "50/20 Mbps wireless backup online." },
                new() { WorkItemId = "LIVE-4204", ActivityDate = new DateTime(2026, 10, 20), FiscalYear = 2026, FiscalWeek = 42, ActivityType = "Circuit Install", Title = "Charter Spectrum Riser Extension Attempt", StoreNumber = "6020", StoreName = "Mandee", BannerCode = "M", Status = "Blocked / Delayed", AssignedTo = "Sarah Jenkins (PS4)", TimeWindow = "08:00 - 12:00", Notes = "Rejected by Queens Center security — badging required." },
                new() { WorkItemId = "LIVE-4205", ActivityDate = new DateTime(2026, 10, 21), FiscalYear = 2026, FiscalWeek = 42, ActivityType = "Low-Voltage Cabling", Title = "32-Drop Cat6 Plenum Pull — Sawgrass Mills", StoreNumber = "6004", StoreName = "Charlotte Russe", BannerCode = "CR", Status = "In Progress", AssignedTo = "Hector Ruiz (PS3)", TimeWindow = "07:00 - 15:00", Notes = "Granite Telecom pulling POS and AP runs." },
                new() { WorkItemId = "LIVE-4206", ActivityDate = new DateTime(2026, 10, 22), FiscalYear = 2026, FiscalWeek = 42, ActivityType = "TTU (Test & Turn-Up)", Title = "AT&T Commercial 5G Secondary WAN TTU", StoreNumber = "6004", StoreName = "Charlotte Russe", BannerCode = "CR", Status = "Scheduled", AssignedTo = "Hector Ruiz (PS3)", TimeWindow = "10:00 - 12:00", Notes = "Scheduled turn-up with AT&T remote NOC." },
                new() { WorkItemId = "LIVE-4207", ActivityDate = new DateTime(2026, 10, 23), FiscalYear = 2026, FiscalWeek = 42, ActivityType = "Power Pole Install", Title = "Yorkdale Night Shift Ceiling Tie-Wire & Poles", StoreNumber = "6142", StoreName = "Bluenotes Flagship", BannerCode = "BN", Status = "Scheduled", AssignedTo = "Terry O'Connor (PS1)", TimeWindow = "22:30 - 03:00", Notes = "Guild Electric night shift booked." },
                new() { WorkItemId = "LIVE-4208", ActivityDate = new DateTime(2026, 10, 24), FiscalYear = 2026, FiscalWeek = 42, ActivityType = "POS Staging & Cutover", Title = "4-Register POS Staging & Meraki Verification", StoreNumber = "6012", StoreName = "rue21", BannerCode = "R21", Status = "Scheduled", AssignedTo = "Danielle Levy (PS2)", TimeWindow = "09:00 - 14:00", Notes = "Final store readiness check." },

                // WEEK 43 (Oct 25 - Oct 31, 2026)
                new() { WorkItemId = "LIVE-4301", ActivityDate = new DateTime(2026, 10, 26), FiscalYear = 2026, FiscalWeek = 43, ActivityType = "Circuit Install", Title = "Bell Canada Primary Fiber Demarc Pull", StoreNumber = "6142", StoreName = "Bluenotes Flagship", BannerCode = "BN", Status = "Scheduled", AssignedTo = "Terry O'Connor (PS1)", TimeWindow = "06:00 - 09:00", Notes = " Demarc B1-14 to CRU-218B." },
                new() { WorkItemId = "LIVE-4302", ActivityDate = new DateTime(2026, 10, 28), FiscalYear = 2026, FiscalWeek = 43, ActivityType = "Power Pole Install", Title = "3x Wiremold 25DTP-4 Cash Wrap Install", StoreNumber = "6004", StoreName = "Charlotte Russe", BannerCode = "CR", Status = "Scheduled", AssignedTo = "Hector Ruiz (PS3)", TimeWindow = "07:00 - 11:00", Notes = "Florida Power Techs scheduled." }
            };

            return list;
        }

        public async Task<List<FiscalCalendarDay>> LoadFiscalCalendarAsync(int? fiscalYear = null)
        {
            if (!IsConnected)
            {
                await TestConnectionAsync();
            }

            var list = new List<FiscalCalendarDay>();
            try
            {
                await using var conn = new NpgsqlConnection(ConnectionString);
                await conn.OpenAsync();

                string sql = @"
                    SELECT calendar_date, fiscal_year, fiscal_quarter, fiscal_month, fiscal_week, day_of_fiscal_week, is_holiday, holiday_name 
                    FROM fiscal_calendar " +
                    (fiscalYear.HasValue ? "WHERE fiscal_year = @FiscalYear " : "") +
                    "ORDER BY calendar_date;";

                await using var cmd = new NpgsqlCommand(sql, conn);
                if (fiscalYear.HasValue)
                {
                    cmd.Parameters.AddWithValue("FiscalYear", (short)fiscalYear.Value);
                }

                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(new FiscalCalendarDay
                    {
                        CalendarDate = reader.GetDateTime(0),
                        FiscalYear = reader.GetInt16(1),
                        FiscalQuarter = reader.GetInt16(2),
                        FiscalMonth = reader.GetInt16(3),
                        FiscalWeek = reader.GetInt16(4),
                        DayOfFiscalWeek = reader.GetInt16(5),
                        IsHoliday = reader.GetBoolean(6),
                        HolidayName = reader.IsDBNull(7) ? null : reader.GetString(7)
                    });
                }
            }
            catch
            {
                // Fall back to deterministic NRF 4-5-4 generator below
            }

            if (list.Count == 0)
            {
                list = GenerateNrfCalendarDays(fiscalYear);
            }

            // Attach daily CalendarWorkItems to each FiscalCalendarDay
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
            int total = stores.Count;
            if (total == 0) return SeedDataService.GetWeeklyReport(fiscalWeek);

            // Load calendar work items if not passed in
            if (allWorkItems == null)
            {
                allWorkItems = CreateInitialLiveCalendarItems(stores);
                if (File.Exists(LiveCalendarFilePath))
                {
                    try
                    {
                        var diskItems = JsonSerializer.Deserialize<List<CalendarWorkItem>>(File.ReadAllText(LiveCalendarFilePath));
                        if (diskItems != null && diskItems.Count > 0) allWorkItems = diskItems;
                    }
                    catch { }
                }
            }

            var (weekStart, weekEnd) = GetDateRangeForFiscalWeek(fiscalWeek);
            string dateRange = $"{weekStart:MMM dd} – {weekEnd:MMM dd, yyyy}";

            // Filter tasks for THIS specific week
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

            // Base current-state counts from stores
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
            int startW = Math.Clamp(fiscalWeek - 3, 1, 45);
            int endW = startW + 7;
            for (int w = startW; w <= endW; w++)
            {
                var (ws, we) = GetDateRangeForFiscalWeek(w);
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
                ? $"Week {fiscalWeek} ({dateRange}): {weekCompleted}/{weekTotal} scheduled field operations completed ({ttuCount} TTUs, {circuitCount} Circuit Installs, {cablingCount} Cabling, {poleCount} Power Poles, {surveyCount} Surveys)."
                : $"Week {fiscalWeek} ({dateRange}): Cumulative rollout at {overallPct}% across {total} stores. Add daily TTU or Circuit tasks in the Calendar to track day-by-day execution.";

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
                <= 30 => 0.15,
                <= 35 => 0.28,
                36 => 0.36,
                37 => 0.46,
                38 => 0.58,
                39 => 0.70,
                40 => 0.81,
                41 => 0.91,
                42 => 1.00,
                43 => 1.04,
                44 => 1.07,
                45 => 1.10,
                46 => 1.12,
                47 => 1.14,
                _ => 1.15
            };
        }

        #region Authentication & Security (Salt and Pepper)

        public async Task<User?> GetUserByUsernameOrEmailAsync(string usernameOrEmail)
        {
            if (string.IsNullOrWhiteSpace(usernameOrEmail))
                return null;

            try
            {
                await using var conn = new NpgsqlConnection(ConnectionString);
                await conn.OpenAsync();

                const string sql = @"
                    SELECT user_id, username, email, full_name, role, password_hash, password_salt, iterations, is_active
                    FROM app_users
                    WHERE LOWER(username) = LOWER(@id) OR LOWER(email) = LOWER(@id)
                    LIMIT 1;";

                await using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("id", usernameOrEmail.Trim());

                await using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var roleStr = reader.GetString(4);
                    if (!Enum.TryParse<UserRole>(roleStr, true, out var role))
                    {
                        role = UserRole.PM;
                    }

                    return new User
                    {
                        UserId = reader.GetInt32(0),
                        Username = reader.GetString(1),
                        Email = reader.GetString(2),
                        FullName = reader.GetString(3),
                        Role = role,
                        PasswordHash = reader.GetString(5),
                        PasswordSalt = reader.GetString(6),
                        Iterations = reader.GetInt32(7),
                        IsActive = reader.GetBoolean(8)
                    };
                }
            }
            catch (Exception ex)
            {
                LastError = $"Failed to fetch user: {ex.Message}";
            }
            return null;
        }

        public async Task<bool> RecordUserLoginAsync(int userId)
        {
            try
            {
                await using var conn = new NpgsqlConnection(ConnectionString);
                await conn.OpenAsync();

                const string sql = @"
                    UPDATE app_users 
                    SET last_login_at = CURRENT_TIMESTAMP, failed_attempts = 0, updated_at = CURRENT_TIMESTAMP
                    WHERE user_id = @userId;";

                await using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("userId", userId);
                return await cmd.ExecuteNonQueryAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RecordFailedLoginAttemptAsync(string usernameOrEmail)
        {
            try
            {
                await using var conn = new NpgsqlConnection(ConnectionString);
                await conn.OpenAsync();

                const string sql = @"
                    UPDATE app_users 
                    SET failed_attempts = failed_attempts + 1, updated_at = CURRENT_TIMESTAMP
                    WHERE LOWER(username) = LOWER(@id) OR LOWER(email) = LOWER(@id);";

                await using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("id", usernameOrEmail.Trim());
                return await cmd.ExecuteNonQueryAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<(bool Success, string Message)> ResetUserPasswordAsync(string usernameOrEmail, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(usernameOrEmail))
                return (false, "Username or email is required.");

            if (!PasswordSecurityService.ValidatePassword(newPassword, out var failureReason))
                return (false, failureReason);

            try
            {
                await using var conn = new NpgsqlConnection(ConnectionString);
                await conn.OpenAsync();

                // First verify user exists
                const string checkSql = @"
                    SELECT user_id, is_active FROM app_users 
                    WHERE LOWER(username) = LOWER(@id) OR LOWER(email) = LOWER(@id)
                    LIMIT 1;";

                int userId = 0;
                bool isActive = false;
                await using (var checkCmd = new NpgsqlCommand(checkSql, conn))
                {
                    checkCmd.Parameters.AddWithValue("id", usernameOrEmail.Trim());
                    await using var reader = await checkCmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        userId = reader.GetInt32(0);
                        isActive = reader.GetBoolean(1);
                    }
                }

                if (userId == 0)
                    return (false, "No active user account found matching the provided identity.");

                if (!isActive)
                    return (false, "This user account is currently deactivated. Contact an administrator.");

                // Generate new salt and hash with Pepper
                PasswordSecurityService.HashNewPassword(newPassword, out string saltBase64, out string hashBase64);

                const string updateSql = @"
                    UPDATE app_users
                    SET password_hash = @hash,
                        password_salt = @salt,
                        iterations = 100000,
                        password_changed_at = CURRENT_TIMESTAMP,
                        failed_attempts = 0,
                        locked_until = NULL,
                        updated_at = CURRENT_TIMESTAMP
                    WHERE user_id = @userId;";

                await using (var updateCmd = new NpgsqlCommand(updateSql, conn))
                {
                    updateCmd.Parameters.AddWithValue("hash", hashBase64);
                    updateCmd.Parameters.AddWithValue("salt", saltBase64);
                    updateCmd.Parameters.AddWithValue("userId", userId);
                    int rows = await updateCmd.ExecuteNonQueryAsync();
                    if (rows > 0)
                    {
                        return (true, "Password has been securely reset. You can now sign in with your new credentials.");
                    }
                }

                return (false, "Could not update password in the database.");
            }
            catch (Exception ex)
            {
                return (false, $"Database error while resetting password: {ex.Message}");
            }
        }

        #endregion
    }
}
