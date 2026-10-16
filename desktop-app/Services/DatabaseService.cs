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
                    var userInfo = uri.UserInfo.Split(':');
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
                        sc.full_name as contact_name, sc.email as contact_email, sc.phone_mobile as contact_phone
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

                await using var cmd = new NpgsqlCommand(sql, conn);
                await using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var store = new StoreDeployment
                    {
                        StoreId = reader.GetInt32(0),
                        StoreNumber = reader.GetString(1),
                        BannerCode = reader.GetString(2),
                        BannerName = reader.GetString(3),
                        StoreName = reader.GetString(4),
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
                            CountryCode = store.CountryCode
                        };
                    }

                    stores.Add(store);
                }

                if (stores.Count == 0)
                {
                    return SeedDataService.GetStores();
                }

                return stores;
            }
            catch
            {
                IsConnected = false;
                StatusMessage = "⚠️ PostgreSQL query failed. Operating in offline seed mode.";
                return SeedDataService.GetStores();
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
                        COALESCE(m.delivery_window, '06:00 - 09:30'),
                        COALESCE(m.night_work_only, false),
                        COALESCE(m.badging_fee_per_tech, 25),
                        COALESCE(m.special_cabling_rules, ''),
                        COALESCE(m.management_contact_name, ''),
                        COALESCE(m.management_email, ''),
                        COALESCE(m.management_phone, ''),
                        COALESCE(m.security_dispatch_phone, ''),
                        COALESCE(m.address_line1, '')
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
                        LandlordCompany = reader.GetString(3),
                        PropertyType = reader.GetString(4),
                        City = reader.GetString(5),
                        StateProvince = reader.GetString(6),
                        CountryCode = reader.GetString(7),
                        CoiRequired = reader.GetBoolean(8),
                        MinGeneralLiabilityCad = reader.GetDecimal(9),
                        DeliveryWindow = reader.GetString(10),
                        NightWorkOnly = reader.GetBoolean(11),
                        BadgingFeePerTech = reader.GetDecimal(12),
                        SpecialRules = reader.GetString(13),
                        ManagementContactName = reader.GetString(14),
                        ManagementEmail = reader.GetString(15),
                        ManagementPhone = reader.GetString(16),
                        SecurityPhone = reader.GetString(17),
                        Address = reader.GetString(18)
                    });
                }

                if (malls.Count == 0)
                {
                    return new List<MallProperty>(SeedDataService.Malls.Values);
                }

                return malls;
            }
            catch
            {
                return new List<MallProperty>(SeedDataService.Malls.Values);
            }
        }

        public async Task<bool> SaveMallAsync(MallProperty mall)
        {
            try
            {
                await using var conn = new NpgsqlConnection(ConnectionString);
                await conn.OpenAsync();

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
                        management_phone = @ContactPhone
                    WHERE mall_id = @MallId;";

                await using var cmd = new NpgsqlCommand(sql, conn);
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
                cmd.Parameters.AddWithValue("MallId", mall.MallId);

                await cmd.ExecuteNonQueryAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
