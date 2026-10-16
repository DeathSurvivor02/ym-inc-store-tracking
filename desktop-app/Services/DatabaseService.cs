using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;
using YMInc.StoreDeploymentApp.Models;

namespace YMInc.StoreDeploymentApp.Services
{
    public class DatabaseService
    {
        // Default credentials from .env.example
        public string ConnectionString { get; set; } = 
            "Host=localhost;Port=5432;Database=ym_store_tracking;Username=ym_admin;Password=ym_dev_secret_2026;Timeout=3;CommandTimeout=5;";

        public bool IsConnected { get; private set; }
        public string StatusMessage { get; private set; } = "Initializing...";

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                await using var conn = new NpgsqlConnection(ConnectionString);
                await conn.OpenAsync();
                IsConnected = true;
                StatusMessage = "● PostgreSQL 16 Online (localhost:5432/ym_store_tracking)";
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
                        TO_CHAR(s.opening_date, 'YYYY-MM-DD') as opening_date
                    FROM stores s
                    JOIN banners b ON s.banner_id = b.banner_id
                    JOIN districts d ON s.district_id = d.district_id
                    JOIN regions r ON d.region_id = r.region_id
                    ORDER BY s.store_number;";

                await using var cmd = new NpgsqlCommand(sql, conn);
                await using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    stores.Add(new StoreDeployment
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
                        OpeningDate = reader.IsDBNull(20) ? null : reader.GetString(20)
                    });
                }

                // If DB had 0 stores, fallback to seeds
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
    }
}
