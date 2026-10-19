using System;
using System.Windows.Media;
using YMInc.StoreDeploymentApp.Services;

namespace YMInc.StoreDeploymentApp.Models
{
    public class AuditLogEntry
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string OperatorName { get; set; } = "Jordan Vance";
        public string OperatorRole { get; set; } = "PM";
        public string Category { get; set; } = "Store Specification";
        public string TargetRecord { get; set; } = string.Empty;
        public string StoreNumber { get; set; } = string.Empty;
        public string FieldName { get; set; } = string.Empty;
        public string PreviousValue { get; set; } = string.Empty;
        public string NewValue { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;

        public string TimestampDisplay => Timestamp.ToString("yyyy-MM-dd HH:mm:ss");
        public string OperatorDisplay => string.IsNullOrWhiteSpace(OperatorRole) ? OperatorName : $"{OperatorName} ({OperatorRole})";

        public Brush CategoryBadgeBg
        {
            get
            {
                bool dark = ThemeService.IsDarkTheme;
                return Category switch
                {
                    "Inline Grid Edit" => new SolidColorBrush(dark ? Color.FromRgb(12, 45, 72) : Color.FromRgb(239, 246, 255)),
                    "Store Specification" => new SolidColorBrush(dark ? Color.FromRgb(6, 78, 59) : Color.FromRgb(220, 252, 231)),
                    "Landlord Compliance" => new SolidColorBrush(dark ? Color.FromRgb(76, 29, 149) : Color.FromRgb(243, 232, 255)),
                    "Daily Schedule" => new SolidColorBrush(dark ? Color.FromRgb(120, 53, 15) : Color.FromRgb(254, 243, 199)),
                    "System Save" => new SolidColorBrush(dark ? Color.FromRgb(30, 58, 138) : Color.FromRgb(219, 234, 254)),
                    _ => new SolidColorBrush(dark ? Color.FromRgb(51, 65, 85) : Color.FromRgb(243, 244, 246))
                };
            }
        }

        public Brush CategoryBadgeFg
        {
            get
            {
                bool dark = ThemeService.IsDarkTheme;
                return Category switch
                {
                    "Inline Grid Edit" => new SolidColorBrush(dark ? Color.FromRgb(96, 165, 250) : Color.FromRgb(15, 108, 189)),
                    "Store Specification" => new SolidColorBrush(dark ? Color.FromRgb(74, 222, 128) : Color.FromRgb(21, 128, 61)),
                    "Landlord Compliance" => new SolidColorBrush(dark ? Color.FromRgb(192, 132, 252) : Color.FromRgb(109, 40, 217)),
                    "Daily Schedule" => new SolidColorBrush(dark ? Color.FromRgb(251, 191, 36) : Color.FromRgb(180, 83, 9)),
                    "System Save" => new SolidColorBrush(dark ? Color.FromRgb(147, 197, 253) : Color.FromRgb(29, 78, 216)),
                    _ => new SolidColorBrush(dark ? Color.FromRgb(203, 213, 225) : Color.FromRgb(55, 65, 81))
                };
            }
        }
    }
}
