using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using YMInc.StoreDeploymentApp.Models;

namespace YMInc.StoreDeploymentApp.Services
{
    public static class AuditHistoryService
    {
        private static readonly object _lock = new();
        private static List<AuditLogEntry>? _entries;
        private static string LogFilePath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "audit_history_log.json");

        public static event Action? HistoryChanged;
        public static DateTime LastSavedTimestamp { get; private set; } = DateTime.Now;
        public static int UnsavedChangesCount { get; private set; } = 0;

        public static List<AuditLogEntry> GetEntries(string? storeNumberFilter = null)
        {
            EnsureLoaded();
            lock (_lock)
            {
                var query = _entries!.AsEnumerable();
                if (!string.IsNullOrWhiteSpace(storeNumberFilter))
                {
                    query = query.Where(e =>
                        string.Equals(e.StoreNumber, storeNumberFilter, StringComparison.OrdinalIgnoreCase) ||
                        e.TargetRecord.Contains(storeNumberFilter, StringComparison.OrdinalIgnoreCase));
                }
                return query.OrderByDescending(e => e.Timestamp).ToList();
            }
        }

        public static void MarkPendingEdit()
        {
            UnsavedChangesCount++;
            HistoryChanged?.Invoke();
        }

        public static void MarkSaved()
        {
            UnsavedChangesCount = 0;
            LastSavedTimestamp = DateTime.Now;
            HistoryChanged?.Invoke();
        }

        public static AuditLogEntry RecordChange(
            string category,
            string targetRecord,
            string fieldName,
            string? previousValue,
            string? newValue,
            string? customSummary = null,
            string? storeNumber = null,
            bool markSaved = true)
        {
            EnsureLoaded();
            var user = AuthService.CurrentUser;
            string prev = string.IsNullOrWhiteSpace(previousValue) ? "(empty)" : previousValue.Trim();
            string next = string.IsNullOrWhiteSpace(newValue) ? "(empty)" : newValue.Trim();

            string summary = !string.IsNullOrWhiteSpace(customSummary)
                ? customSummary
                : $"Updated {fieldName} on {targetRecord}: '{prev}' → '{next}'";

            var entry = new AuditLogEntry
            {
                Id = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant(),
                Timestamp = DateTime.Now,
                OperatorName = user?.FullName ?? "Jordan Vance",
                OperatorRole = user?.Role.ToString() ?? "PM",
                Category = category,
                TargetRecord = targetRecord,
                StoreNumber = storeNumber ?? string.Empty,
                FieldName = fieldName,
                PreviousValue = prev,
                NewValue = next,
                Summary = summary
            };

            lock (_lock)
            {
                _entries!.Insert(0, entry);
                if (_entries.Count > 500)
                {
                    _entries.RemoveRange(500, _entries.Count - 500);
                }
                SaveToDiskInternal();
            }

            if (markSaved)
            {
                LastSavedTimestamp = entry.Timestamp;
                UnsavedChangesCount = 0;
            }

            HistoryChanged?.Invoke();
            return entry;
        }

        public static void ExportToCsv(string filePath)
        {
            var entries = GetEntries();
            var sb = new StringBuilder();
            sb.AppendLine("ID,Timestamp,Operator,Role,Category,TargetRecord,StoreNumber,Field,PreviousValue,NewValue,Summary");
            foreach (var e in entries)
            {
                sb.AppendLine($"\"{Esc(e.Id)}\",\"{Esc(e.TimestampDisplay)}\",\"{Esc(e.OperatorName)}\",\"{Esc(e.OperatorRole)}\",\"{Esc(e.Category)}\",\"{Esc(e.TargetRecord)}\",\"{Esc(e.StoreNumber)}\",\"{Esc(e.FieldName)}\",\"{Esc(e.PreviousValue)}\",\"{Esc(e.NewValue)}\",\"{Esc(e.Summary)}\"");
            }
            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }

        private static string Esc(string? val) => (val ?? string.Empty).Replace("\"", "\"\"");

        private static void EnsureLoaded()
        {
            lock (_lock)
            {
                if (_entries != null) return;

                try
                {
                    if (File.Exists(LogFilePath))
                    {
                        string json = File.ReadAllText(LogFilePath);
                        var loaded = JsonSerializer.Deserialize<List<AuditLogEntry>>(json);
                        if (loaded != null && loaded.Count > 0)
                        {
                            _entries = loaded;
                            return;
                        }
                    }
                }
                catch
                {
                    // Fallback to seed entries
                }

                _entries = CreateInitialSeedEntries();
                SaveToDiskInternal();
            }
        }

        private static void SaveToDiskInternal()
        {
            try
            {
                string json = JsonSerializer.Serialize(_entries, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(LogFilePath, json);
            }
            catch
            {
                // Ignore IO lock
            }
        }

        private static List<AuditLogEntry> CreateInitialSeedEntries()
        {
            var now = DateTime.Now;
            return new List<AuditLogEntry>
            {
                new AuditLogEntry
                {
                    Id = "AUD-1042",
                    Timestamp = now.AddMinutes(-18),
                    OperatorName = "Jordan Vance",
                    OperatorRole = "PM",
                    Category = "Daily Schedule",
                    TargetRecord = "2026-10-14 — Store #0002 (Scarborough Town Centre)",
                    StoreNumber = "0002",
                    FieldName = "TTU (Test & Turn-Up)",
                    PreviousValue = "In Progress",
                    NewValue = "Completed",
                    Summary = "Completed Bell 100M Fiber TTU & Meraki MX68 ping test at Store #0002."
                },
                new AuditLogEntry
                {
                    Id = "AUD-1041",
                    Timestamp = now.AddMinutes(-45),
                    OperatorName = "Jordan Vance",
                    OperatorRole = "PM",
                    Category = "Landlord Compliance",
                    TargetRecord = "MALL-STC — Scarborough Town Centre",
                    StoreNumber = "0002",
                    FieldName = "Assigned Cabling Contractor",
                    PreviousValue = "TBD",
                    NewValue = "Datacom Retail Solutions",
                    Summary = "Updated Assigned Cabling Contractor & verified $5M COI clearance with Oxford Properties."
                },
                new AuditLogEntry
                {
                    Id = "AUD-1040",
                    Timestamp = now.AddHours(-2),
                    OperatorName = "Alex Mercer",
                    OperatorRole = "NET",
                    Category = "Store Specification",
                    TargetRecord = "Store #0301 — Woodfield Mall",
                    StoreNumber = "0301",
                    FieldName = "Primary WAN Circuit Status",
                    PreviousValue = "Ordered — Pending Demarc",
                    NewValue = "Installed — Ready for TTU",
                    Summary = "Comcast Business 200M circuit demarc extended to BOH rack at Store #0301."
                },
                new AuditLogEntry
                {
                    Id = "AUD-1039",
                    Timestamp = now.AddHours(-5),
                    OperatorName = "Samira Patel",
                    OperatorRole = "TECH",
                    Category = "Inline Grid Edit",
                    TargetRecord = "Store #0001 — Vaughan Mills",
                    StoreNumber = "0001",
                    FieldName = "STAGE",
                    PreviousValue = "Circuits & TTU",
                    NewValue = "Certified Ready",
                    Summary = "Inline grid update: Marked Store #0001 (Vaughan Mills) as Certified Ready."
                }
            };
        }
    }
}
