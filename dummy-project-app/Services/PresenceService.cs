using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using YMInc.StoreDeploymentApp.Models;

namespace YMInc.StoreDeploymentApp.Services
{
    public class ActiveEditorLease
    {
        public string StoreNumber { get; set; } = string.Empty;
        public string EditorName { get; set; } = string.Empty;
        public string EditorRole { get; set; } = string.Empty;
        public string MachineName { get; set; } = string.Empty;
        public int ProcessId { get; set; }
        public DateTime LastHeartbeatUtc { get; set; }
    }

    public static class PresenceService
    {
        private static readonly string LockFilePath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "active_editor_leases.json");

        private static readonly object _sync = new();

        public static void RegisterEditor(string storeNumber, User? user)
        {
            if (string.IsNullOrEmpty(storeNumber) || user == null) return;
            lock (_sync)
            {
                var leases = LoadLeases();
                leases.RemoveAll(l => (DateTime.UtcNow - l.LastHeartbeatUtc).TotalSeconds > 35);
                leases.RemoveAll(l => l.StoreNumber == storeNumber && l.ProcessId == Environment.ProcessId);

                leases.Add(new ActiveEditorLease
                {
                    StoreNumber = storeNumber,
                    EditorName = user.FullName,
                    EditorRole = user.RoleDisplayName,
                    MachineName = Environment.MachineName,
                    ProcessId = Environment.ProcessId,
                    LastHeartbeatUtc = DateTime.UtcNow
                });
                SaveLeases(leases);
            }
        }

        public static void RefreshHeartbeat(string storeNumber)
        {
            if (string.IsNullOrEmpty(storeNumber)) return;
            lock (_sync)
            {
                var leases = LoadLeases();
                var mine = leases.FirstOrDefault(l => l.StoreNumber == storeNumber && l.ProcessId == Environment.ProcessId);
                if (mine != null)
                {
                    mine.LastHeartbeatUtc = DateTime.UtcNow;
                    SaveLeases(leases);
                }
            }
        }

        public static void ReleaseEditor(string storeNumber)
        {
            if (string.IsNullOrEmpty(storeNumber)) return;
            lock (_sync)
            {
                var leases = LoadLeases();
                leases.RemoveAll(l => l.StoreNumber == storeNumber && l.ProcessId == Environment.ProcessId);
                SaveLeases(leases);
            }
        }

        public static ActiveEditorLease? GetOtherActiveEditor(string storeNumber)
        {
            if (string.IsNullOrEmpty(storeNumber)) return null;
            lock (_sync)
            {
                var leases = LoadLeases();
                return leases.FirstOrDefault(l =>
                    l.StoreNumber == storeNumber &&
                    l.ProcessId != Environment.ProcessId &&
                    (DateTime.UtcNow - l.LastHeartbeatUtc).TotalSeconds <= 30);
            }
        }

        private static List<ActiveEditorLease> LoadLeases()
        {
            try
            {
                if (File.Exists(LockFilePath))
                {
                    string json = File.ReadAllText(LockFilePath);
                    return JsonSerializer.Deserialize<List<ActiveEditorLease>>(json) ?? new();
                }
            }
            catch { }
            return new();
        }

        private static void SaveLeases(List<ActiveEditorLease> leases)
        {
            try
            {
                var opts = new JsonSerializerOptions { WriteIndented = true };
                File.WriteAllText(LockFilePath, JsonSerializer.Serialize(leases, opts));
            }
            catch { }
        }
    }
}
