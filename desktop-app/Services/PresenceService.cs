using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using YMInc.StoreDeploymentApp.Models;

namespace YMInc.StoreDeploymentApp.Services
{
    public class ActiveEditorLease
    {
        public string SessionId { get; set; } = string.Empty;
        public string StoreNumber { get; set; } = string.Empty;
        public string EditorName { get; set; } = string.Empty;
        public string EditorRole { get; set; } = string.Empty;
        public string UserInitials { get; set; } = string.Empty;
        public string ActiveSection { get; set; } = string.Empty;
        public string ActiveField { get; set; } = string.Empty;
        public DateTime LastHeartbeatUtc { get; set; }

        public Brush AvatarBrush
        {
            get
            {
                if (EditorRole.Contains("PM", StringComparison.OrdinalIgnoreCase))
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0F6CBD"));
                if (EditorRole.Contains("PA", StringComparison.OrdinalIgnoreCase))
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#059669"));
                if (EditorRole.Contains("PS1", StringComparison.OrdinalIgnoreCase))
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7C3AED"));
                if (EditorRole.Contains("PS2", StringComparison.OrdinalIgnoreCase))
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D97706"));
                if (EditorRole.Contains("PS3", StringComparison.OrdinalIgnoreCase))
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DC2626"));
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4F46E5"));
            }
        }

        public string Initials => !string.IsNullOrEmpty(UserInitials) ? UserInitials : (EditorName.Length >= 2 ? EditorName[..2].ToUpper() : "U");
        public string DisplayName => EditorName.Split(' ')[0];
        public string SectionBadge => !string.IsNullOrEmpty(ActiveSection) && ActiveSection != "General" ? $"• {ActiveSection}" : "";
        public string TooltipText => $"{EditorName} ({EditorRole})\nWorking in: {ActiveSection}\nLive Sync Active";
    }

    public static class PresenceService
    {
        public static readonly string CurrentSessionId = Guid.NewGuid().ToString("N");
        private static readonly DatabaseService _db = new();
        private static User? _currentUser;
        private static string? _currentStoreNumber;
        private static string _currentSection = "General";
        private static string? _currentField;

        public static event Action? PresenceUpdated;

        public static void SetCurrentUser(User? user)
        {
            _currentUser = user;
        }

        public static async Task EnterStoreAsync(string storeNumber, User? user, string section = "General")
        {
            _currentStoreNumber = storeNumber;
            _currentSection = section;
            if (user != null) _currentUser = user;

            await HeartbeatAsync();
        }

        public static async Task UpdateSectionAsync(string storeNumber, string section, string? field = null)
        {
            _currentStoreNumber = storeNumber;
            _currentSection = section;
            _currentField = field;
            await HeartbeatAsync();
        }

        public static async Task LeaveStoreAsync(string storeNumber)
        {
            _currentStoreNumber = null;
            _currentSection = "Lobby";
            _currentField = null;
            await _db.RemovePresenceAsync(CurrentSessionId);
            PresenceUpdated?.Invoke();
        }

        public static async Task HeartbeatAsync()
        {
            if (_currentUser == null) return;
            string initials = !string.IsNullOrEmpty(_currentUser.Username) && _currentUser.Username.Length >= 2
                ? _currentUser.Username[..2].ToUpperInvariant()
                : "U";

            await _db.UpdatePresenceHeartbeatAsync(
                CurrentSessionId,
                _currentUser.UserId,
                _currentUser.FullName,
                _currentUser.RoleDisplayName,
                initials,
                _currentStoreNumber,
                _currentSection,
                _currentField);
        }

        public static async Task<List<ActiveEditorLease>> GetStoreCollaboratorsAsync(string storeNumber)
        {
            var list = await _db.GetActiveCollaboratorsAsync(storeNumber);
            return list.Where(l => l.SessionId != CurrentSessionId).ToList();
        }

        public static async Task<List<ActiveEditorLease>> GetAllCollaboratorsAsync()
        {
            var list = await _db.GetAllActiveCollaboratorsAsync();
            return list.Where(l => l.SessionId != CurrentSessionId).ToList();
        }

        public static void RegisterEditor(string storeNumber, User? user)
        {
            _ = EnterStoreAsync(storeNumber, user);
        }

        public static void RefreshHeartbeat(string storeNumber)
        {
            _ = HeartbeatAsync();
        }

        public static void ReleaseEditor(string storeNumber)
        {
            _ = LeaveStoreAsync(storeNumber);
        }

        public static ActiveEditorLease? GetOtherActiveEditor(string storeNumber)
        {
            var list = _db.GetActiveCollaboratorsAsync(storeNumber).GetAwaiter().GetResult();
            return list.FirstOrDefault(l => l.SessionId != CurrentSessionId);
        }
    }
}
