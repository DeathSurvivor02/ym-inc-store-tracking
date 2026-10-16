using System;
using System.Collections.Generic;
using System.IO;
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
                StatusMessage = $"● PostgreSQL 16 Online ({builder.Host}/{builder.Database})";
                return true;
            }
            catch (Exception ex)
            {
                IsConnected = false;
                StatusMessage = $"⚠️ PostgreSQL Offline ({ex.Message.Split('\n')[0]}). Using Seed Fixtures.";
                return false;
            }
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
                        COALESCE(mp.cabling_vendor, '') as mall_cabling_vendor, COALESCE(mp.lead_technician_name, '') as mall_lead_tech
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
                                LeadTechnicianName = reader.IsDBNull(41) ? string.Empty : reader.GetString(41)
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
                        COALESCE(m.lead_technician_name, '')
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
                        LeadTechnicianName = reader.IsDBNull(20) ? "" : reader.GetString(20)
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
                        lead_technician_name = @LeadTechnician
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
                        contact_name = @ContactName,
                        email_address = @ContactEmail,
                        phone = @ContactPhone,
                        security_phone = @SecurityPhone,
                        requirements_ttu_cabling = @SpecialRules,
                        coi_required = @CoiRequired,
                        night_work_only = @NightWorkOnly,
                        work_hours_restriction = @DeliveryWindow
                    WHERE mall_id = @MallId;";

                await using var reqCmd = new NpgsqlCommand(reqSql, conn, tx);
                reqCmd.Parameters.AddWithValue("ContactName", mall.ManagementContactName ?? string.Empty);
                reqCmd.Parameters.AddWithValue("ContactEmail", mall.ManagementEmail ?? string.Empty);
                reqCmd.Parameters.AddWithValue("ContactPhone", mall.ManagementPhone ?? string.Empty);
                reqCmd.Parameters.AddWithValue("SecurityPhone", mall.SecurityPhone ?? string.Empty);
                reqCmd.Parameters.AddWithValue("SpecialRules", mall.SpecialRules ?? string.Empty);
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

                // LoadStoresAsync shows only the part of store_name before " - ".
                // Only overwrite the stored name when the user actually changed that displayed value,
                // otherwise saving would permanently truncate the full name in the database.
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
                // Fallback in-memory generator if offline
                DateTime start = new DateTime(2025, 2, 2);
                for (int i = 0; i < 365 * 3; i++)
                {
                    var d = start.AddDays(i);
                    list.Add(new FiscalCalendarDay
                    {
                        CalendarDate = d,
                        FiscalYear = d.Year,
                        FiscalQuarter = ((d.Month - 1) / 3) + 1,
                        FiscalMonth = d.Month,
                        FiscalWeek = (i / 7) + 1,
                        DayOfFiscalWeek = ((int)d.DayOfWeek) + 1,
                        IsHoliday = false
                    });
                }
            }
            return list;
        }

        public static WeeklyProgressReport GetWeeklyReportFromFleet(int fiscalWeek, List<StoreDeployment> stores)
        {
            int total = stores.Count;
            if (total == 0) return SeedDataService.GetWeeklyReport(fiscalWeek);

            int completed = stores.Count(s => s.WeeklyStatus == "Completed & Certified");
            int inProgress = stores.Count(s => s.WeeklyStatus != null && s.WeeklyStatus.StartsWith("In Progress"));
            int scheduled = stores.Count(s => s.WeeklyStatus == "Scheduled / Pending");
            int blocked = stores.Count(s => s.WeeklyStatus == "Blocked / In Jeopardy");

            if (completed == 0 && inProgress == 0 && scheduled == 0 && blocked == 0)
            {
                completed = stores.Count(s => s.OperationalStatus == "Open" || s.OperationalStatus == "Grand Open");
                blocked = stores.Count(s => s.OverallHealth == "At Risk" || s.OverallHealth == "Critical Blocker");
                inProgress = Math.Max(0, total - completed - blocked);
            }

            // Report true counts only; a genuine 0 must not be inflated with the "completed" count.
            int siteSurveysCompleted = stores.Count(s => s.Survey != null && s.Survey.Status.Contains("Approved"));
            int powerPolesInstalled = stores.Count(s => s.PowerPoles != null && s.PowerPoles.InstalledCount >= s.PowerPoles.TotalRequired);
            int cablingCertified = stores.Count(s => s.Cabling != null && s.Cabling.FlukeCertified);
            int circuitsActive = stores.Count(s => s.Circuits.Count >= 2 && s.Circuits.All(c => c.OrderStatus.Contains("Active") || c.OrderStatus.Contains("Turned Up")));
            int posCommissioned = stores.Count(s => s.PosStagingStatus == "Commissioned");

            double completedPct = total > 0 ? (double)completed / total * 100 : 0;
            double inProgressPct = total > 0 ? (double)inProgress / total * 100 : 0;
            double scheduledPct = total > 0 ? (double)scheduled / total * 100 : 0;
            double blockedPct = total > 0 ? (double)blocked / total * 100 : 0;

            string dateRange = fiscalWeek == 41 ? "Oct 11 – Oct 17, 2026" : "Oct 18 – Oct 24, 2026";

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
                OverallProgressPct = (int)Math.Round(completedPct),
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
                    new() { Name = "Completed & Certified", Count = completed, Percentage = completedPct, ColorHex = "#10b981" },
                    new() { Name = "In Progress / On Schedule", Count = inProgress, Percentage = inProgressPct, ColorHex = "#0ea5e9" },
                    new() { Name = "Scheduled / Pending", Count = scheduled, Percentage = scheduledPct, ColorHex = "#f59e0b" },
                    new() { Name = "Blocked / In Jeopardy", Count = blocked, Percentage = blockedPct, ColorHex = "#f43f5e" }
                }
            };
        }
    }
}
