using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;

namespace YMInc.StoreDeploymentApp.Services
{
    public static class ThemeService
    {
        private static bool _initialized;
        private static readonly string ThemePrefPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "theme_preference.txt");

        public static bool IsDarkTheme { get; private set; }

        public static string CurrentThemeLabel => IsDarkTheme ? "Dark Theme" : "Light Theme";
        public static string ToggleButtonText => IsDarkTheme ? "☀ Light Theme" : "🌙 Dark Theme";

        public static event EventHandler? ThemeChanged;

        private static readonly DependencyProperty OriginalColorsProperty =
            DependencyProperty.RegisterAttached(
                "OriginalColors",
                typeof(Dictionary<string, Color>),
                typeof(ThemeService),
                new PropertyMetadata(null));

        public static void Initialize()
        {
            if (_initialized) return;
            _initialized = true;

            try
            {
                if (File.Exists(ThemePrefPath))
                {
                    string saved = File.ReadAllText(ThemePrefPath).Trim();
                    IsDarkTheme = saved.Equals("Dark", StringComparison.OrdinalIgnoreCase);
                }
            }
            catch { }

            EventManager.RegisterClassHandler(
                typeof(Window),
                FrameworkElement.LoadedEvent,
                new RoutedEventHandler(OnWindowLoaded));

            UpdateApplicationResources();
        }

        public static void ToggleTheme()
        {
            SetTheme(!IsDarkTheme);
        }

        public static void SetTheme(bool isDark)
        {
            IsDarkTheme = isDark;
            try
            {
                File.WriteAllText(ThemePrefPath, IsDarkTheme ? "Dark" : "Light");
            }
            catch { }

            UpdateApplicationResources();

            if (Application.Current != null)
            {
                foreach (Window win in Application.Current.Windows)
                {
                    ApplyThemeToWindow(win);
                }
            }

            ThemeChanged?.Invoke(null, EventArgs.Empty);
        }

        private static void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is Window win)
            {
                ApplyThemeToWindow(win);
            }
        }

        public static void UpdateApplicationResources()
        {
            var res = Application.Current?.Resources;
            if (res == null) return;

            if (IsDarkTheme)
            {
                SetBrushResource(res, "BgCanvasBrush", "#1E1E1E");
                SetBrushResource(res, "BgPanelBrush", "#252526");
                SetBrushResource(res, "BgHeaderBrush", "#2D2D30");
                SetBrushResource(res, "BgSubtleBrush", "#2A2D2E");
                SetBrushResource(res, "BorderLineBrush", "#3E3E42");
                SetBrushResource(res, "BorderLightBrush", "#333337");
                SetBrushResource(res, "AccentBlueBrush", "#60A5FA");
                SetBrushResource(res, "AccentBlueHoverBrush", "#93C5FD");
                SetBrushResource(res, "TabSelectedHoverBrush", "#2A4365");
                SetBrushResource(res, "TextPrimaryBrush", "#F3F2F1");
                SetBrushResource(res, "TextSecondaryBrush", "#CCCCCC");
                SetBrushResource(res, "TextMutedBrush", "#9CA3AF");
                SetBrushResource(res, "ControlBgBrush", "#2D2D30");
                SetBrushResource(res, "ControlHoverBrush", "#3E3E42");
                SetBrushResource(res, "ControlPressedBrush", "#4E4E52");
                SetBrushResource(res, "InputBgBrush", "#1E1E1E");
                SetBrushResource(res, "InputReadOnlyBgBrush", "#252526");
                SetBrushResource(res, "GridRowAltBrush", "#2A2D2E");
                SetBrushResource(res, "GridRowHoverBrush", "#37373D");
                SetBrushResource(res, "GridRowSelectedBrush", "#094771");
                SetBrushResource(res, "GridRowSelectedTextBrush", "#FFFFFF");
                SetBrushResource(res, SystemColors.WindowBrushKey, "#252526");
                SetBrushResource(res, SystemColors.ControlTextBrushKey, "#F3F2F1");
                SetBrushResource(res, SystemColors.HighlightBrushKey, "#094771");
                SetBrushResource(res, SystemColors.HighlightTextBrushKey, "#FFFFFF");
            }
            else
            {
                SetBrushResource(res, "BgCanvasBrush", "#F3F4F6");
                SetBrushResource(res, "BgPanelBrush", "#FFFFFF");
                SetBrushResource(res, "BgHeaderBrush", "#F8FAFC");
                SetBrushResource(res, "BgSubtleBrush", "#F9FAFB");
                SetBrushResource(res, "BorderLineBrush", "#D1D5DB");
                SetBrushResource(res, "BorderLightBrush", "#E5E7EB");
                SetBrushResource(res, "AccentBlueBrush", "#0F6CBD");
                SetBrushResource(res, "AccentBlueHoverBrush", "#115EA3");
                SetBrushResource(res, "TabSelectedHoverBrush", "#E0EEFA");
                SetBrushResource(res, "TextPrimaryBrush", "#111827");
                SetBrushResource(res, "TextSecondaryBrush", "#4B5563");
                SetBrushResource(res, "TextMutedBrush", "#6B7280");
                SetBrushResource(res, "ControlBgBrush", "#FFFFFF");
                SetBrushResource(res, "ControlHoverBrush", "#EBF3FC");
                SetBrushResource(res, "ControlPressedBrush", "#E5E7EB");
                SetBrushResource(res, "InputBgBrush", "#FFFFFF");
                SetBrushResource(res, "InputReadOnlyBgBrush", "#F3F4F6");
                SetBrushResource(res, "GridRowAltBrush", "#F9FAFB");
                SetBrushResource(res, "GridRowHoverBrush", "#DCEEFE");
                SetBrushResource(res, "GridRowSelectedBrush", "#DBEAFE");
                SetBrushResource(res, "GridRowSelectedTextBrush", "#1E3A8A");
                SetBrushResource(res, SystemColors.WindowBrushKey, "#FFFFFF");
                SetBrushResource(res, SystemColors.ControlTextBrushKey, "#111827");
                SetBrushResource(res, SystemColors.HighlightBrushKey, "#DBEAFE");
                SetBrushResource(res, SystemColors.HighlightTextBrushKey, "#1E3A8A");
            }
        }

        private static void SetBrushResource(ResourceDictionary res, object key, string hex)
        {
            var color = (Color)ColorConverter.ConvertFromString(hex);
            res[key] = new SolidColorBrush(color);
        }

        public static void ApplyThemeToWindow(Window window)
        {
            if (window == null) return;
            ApplyThemeRecursive(window, false);
        }

        private static void ApplyThemeRecursive(DependencyObject obj, bool insideAppChromeBar)
        {
            if (obj == null) return;

            bool isAppChromeBar = insideAppChromeBar;
            if (obj is Border b && b.Background is SolidColorBrush sb)
            {
                string hex = ColorToHex(sb.Color);
                // Top Microsoft Blue Header (#0F6CBD) or Bottom Status Bar (#0078D4 / #0F6CBD)
                if (hex == "#0F6CBD" || hex == "#0078D4" || hex == "#0C5596")
                {
                    isAppChromeBar = true;
                }
            }

            if (!isAppChromeBar)
            {
                // Controls (Buttons, TabItems, DataGridRows, DataGridCells, ScrollBars) have internal templates
                // whose visual triggers (hover, pressed, selection) rely on TemplateBindings and dynamic resources.
                // Overwriting their internal template parts with hardcoded local values breaks mouseover triggers.
                bool isControlTemplatePart = obj is FrameworkElement fe &&
                    (fe.TemplatedParent is ButtonBase ||
                     fe.TemplatedParent is TabItem ||
                     fe.TemplatedParent is DataGridRow ||
                     fe.TemplatedParent is DataGridCell ||
                     fe.TemplatedParent is ScrollBar);

                if (!isControlTemplatePart)
                {
                    TransformElementBrushes(obj);
                }
            }

            // Walk logical children too (for TabItems, ComboBoxItems, non-visual subtrees)
            if (obj is TabControl tc)
            {
                foreach (var item in tc.Items)
                {
                    if (item is TabItem ti && ti.Content is DependencyObject contentObj)
                    {
                        ApplyThemeRecursive(contentObj, false);
                    }
                }
            }

            int childCount = VisualTreeHelper.GetChildrenCount(obj);
            for (int i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);
                ApplyThemeRecursive(child, isAppChromeBar);
            }
        }

        private static void TransformElementBrushes(DependencyObject obj)
        {
            if (obj is Window win)
            {
                MapBrushProperty(win, Control.BackgroundProperty, MapBackgroundColor);
                MapBrushProperty(win, Control.ForegroundProperty, MapForegroundColor);
            }
            else if (obj is ScrollViewer sv)
            {
                MapBrushProperty(sv, Control.BackgroundProperty, MapBackgroundColor);
                MapBrushProperty(sv, Control.ForegroundProperty, MapForegroundColor);
            }
            else if (obj is DataGrid dg)
            {
                MapBrushProperty(dg, Control.BackgroundProperty, MapBackgroundColor);
                MapBrushProperty(dg, Control.ForegroundProperty, MapForegroundColor);
                MapBrushProperty(dg, Control.BorderBrushProperty, MapBorderColor);
                MapBrushProperty(dg, DataGrid.HorizontalGridLinesBrushProperty, MapBorderColor);
                MapBrushProperty(dg, DataGrid.VerticalGridLinesBrushProperty, MapBorderColor);
            }
            else if (obj is Border border)
            {
                MapBrushProperty(border, Border.BackgroundProperty, MapBackgroundColor);
                MapBrushProperty(border, Border.BorderBrushProperty, MapBorderColor);
            }
            else if (obj is Panel panel)
            {
                MapBrushProperty(panel, Panel.BackgroundProperty, MapBackgroundColor);
            }
            else if (obj is TextBlock tb)
            {
                MapBrushProperty(tb, TextBlock.ForegroundProperty, MapForegroundColor);
                MapBrushProperty(tb, TextBlock.BackgroundProperty, MapBackgroundColor);
                if (tb.Inlines.Count > 0)
                {
                    foreach (var inline in tb.Inlines)
                    {
                        if (inline is System.Windows.Documents.Run run)
                        {
                            MapBrushProperty(run, System.Windows.Documents.TextElement.ForegroundProperty, MapForegroundColor);
                        }
                    }
                }
            }
            else if (obj is TextBox tbInput)
            {
                MapBrushProperty(tbInput, Control.BackgroundProperty, MapInputBgColor);
                MapBrushProperty(tbInput, Control.ForegroundProperty, MapForegroundColor);
                MapBrushProperty(tbInput, Control.BorderBrushProperty, MapBorderColor);
            }
            else if (obj is PasswordBox pbox)
            {
                MapBrushProperty(pbox, Control.BackgroundProperty, MapInputBgColor);
                MapBrushProperty(pbox, Control.ForegroundProperty, MapForegroundColor);
                MapBrushProperty(pbox, Control.BorderBrushProperty, MapBorderColor);
            }
            else if (obj is ComboBox cbBox)
            {
                MapBrushProperty(cbBox, Control.BackgroundProperty, MapInputBgColor);
                MapBrushProperty(cbBox, Control.ForegroundProperty, MapForegroundColor);
                MapBrushProperty(cbBox, Control.BorderBrushProperty, MapBorderColor);
            }
            else if (obj is ComboBoxItem cbi)
            {
                MapBrushProperty(cbi, Control.BackgroundProperty, MapInputBgColor);
                MapBrushProperty(cbi, Control.ForegroundProperty, MapForegroundColor);
            }
            else if (obj is Button btn)
            {
                // Preserve Primary Action Button styling
                if (btn.Style != Application.Current?.Resources["PrimaryButton"])
                {
                    MapBrushProperty(btn, Control.BackgroundProperty, MapButtonBgColor);
                    MapBrushProperty(btn, Control.ForegroundProperty, MapForegroundColor);
                    MapBrushProperty(btn, Control.BorderBrushProperty, MapBorderColor);
                }
            }
            else if (obj is Separator sep)
            {
                MapBrushProperty(sep, Control.BackgroundProperty, MapBorderColor);
            }
            else if (obj is Shape shape)
            {
                MapBrushProperty(shape, Shape.FillProperty, MapBorderColor);
                MapBrushProperty(shape, Shape.StrokeProperty, MapBorderColor);
            }
            else if (obj is ProgressBar pb)
            {
                MapBrushProperty(pb, Control.BackgroundProperty, MapBackgroundColor);
            }
            else if (obj is CheckBox cb)
            {
                MapBrushProperty(cb, Control.ForegroundProperty, MapForegroundColor);
            }
        }

        private static void MapBrushProperty(DependencyObject obj, DependencyProperty prop, Func<string, string?> mapper)
        {
            var dict = (Dictionary<string, Color>?)obj.GetValue(OriginalColorsProperty);
            if (dict == null)
            {
                dict = new Dictionary<string, Color>();
                obj.SetValue(OriginalColorsProperty, dict);
            }

            string propKey = prop.Name;
            var currentBrush = obj.GetValue(prop) as SolidColorBrush;

            if (!dict.TryGetValue(propKey, out Color originalColor))
            {
                if (currentBrush == null) return;
                originalColor = currentBrush.Color;
                dict[propKey] = originalColor;
            }

            if (!IsDarkTheme)
            {
                obj.SetValue(prop, new SolidColorBrush(originalColor));
                return;
            }

            string origHex = ColorToHex(originalColor);
            string? darkHex = mapper(origHex);
            if (!string.IsNullOrEmpty(darkHex))
            {
                var darkColor = (Color)ColorConverter.ConvertFromString(darkHex);
                obj.SetValue(prop, new SolidColorBrush(darkColor));
            }
        }

        private static string ColorToHex(Color c)
        {
            if (c.A == 0) return "TRANSPARENT";
            return $"#{c.R:X2}{c.G:X2}{c.B:X2}";
        }

        private static string? MapBackgroundColor(string hex) => hex switch
        {
            "#FAF9F8" or "#F3F4F6" or "#F8FAFC" or "#F1F5F9" or "#F8F9FA" => "#1E1E1E",
            "#FFFFFF" => "#252526",
            "#F9FAFB" => "#2A2D2E",
            "#F3F2F1" or "#EDEBE9" or "#E5E7EB" or "#E2E8F0" => "#2D2D30",
            "#EFF6FC" or "#EFF6FF" or "#DBEAFE" or "#E0F2FE" or "#EBF3FC" => "#172B4D",
            "#DCFCE7" or "#F0FDF4" or "#CCFBF1" or "#DFF6DD" => "#133A25",
            "#FEF3C7" or "#FFFBEB" or "#FFF4CE" => "#3E2709",
            "#FEE2E2" or "#FEF2F2" or "#FDE7E9" or "#FECACA" => "#451A1A",
            "#EDE9FE" => "#2E1A47",
            _ => null
        };

        private static string? MapInputBgColor(string hex) => hex switch
        {
            "#FFFFFF" or "#FAF9F8" or "#F3F4F6" or "#F8FAFC" => "#1E1E1E",
            _ => MapBackgroundColor(hex)
        };

        private static string? MapButtonBgColor(string hex) => hex switch
        {
            "#FFFFFF" or "#FAF9F8" or "#F8FAFC" or "#F3F4F6" or "#F3F2F1" => "#2D2D30",
            "TRANSPARENT" => "TRANSPARENT",
            _ => MapBackgroundColor(hex)
        };

        private static string? MapBorderColor(string hex) => hex switch
        {
            "#D1D5DB" or "#E1DFDD" or "#E5E7EB" or "#D2D0CE" or "#C8C6C4" or "#F3F4F6" or "#E2E8F0" or "#8A8886" or "#797775" => "#3E3E42",
            "#C7E0F4" or "#BFDBFE" or "#93C5FD" => "#2B5278",
            "#BBF7D0" or "#86EFAC" or "#107C10" or "#15803D" or "#16A34A" => "#1E5638",
            "#FDE68A" or "#FCD34D" or "#D97706" or "#795500" => "#6B4411",
            "#FECACA" or "#FCA5A5" or "#F1707B" or "#EF4444" or "#DC2626" or "#B91C1C" or "#A4262C" => "#6E2626",
            _ => null
        };

        private static string? MapForegroundColor(string hex) => hex switch
        {
            "#201F1E" or "#242424" or "#111827" or "#1F2937" or "#323130" or "#374151" or "#1E293B" => "#F3F2F1",
            "#4B5563" or "#605E5C" or "#6B7280" or "#64748B" or "#475569" => "#CBD5E1",
            "#9CA3AF" or "#8A8886" => "#94A3B8",
            "#0F6CBD" or "#1E3A8A" or "#1E40AF" or "#1D4ED8" or "#075985" or "#0078D4" => "#60A5FA",
            "#15803D" or "#166534" or "#107C10" or "#115E59" or "#16A34A" => "#4ADE80",
            "#B45309" or "#92400E" or "#D97706" or "#795500" => "#FCD34D",
            "#B91C1C" or "#991B1B" or "#A4262C" or "#DC2626" or "#EF4444" => "#F87171",
            "#5B21B6" => "#C4B5FD",
            _ => null
        };
    }
}
