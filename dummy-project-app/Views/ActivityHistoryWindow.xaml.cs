using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using YMInc.StoreDeploymentApp.Models;
using YMInc.StoreDeploymentApp.Services;

namespace YMInc.StoreDeploymentApp.Views
{
    public partial class ActivityHistoryWindow : Window
    {
        private List<AuditLogEntry> _allEntries = new();

        public ActivityHistoryWindow(string? initialStoreFilter = null)
        {
            InitializeComponent();
            if (!string.IsNullOrWhiteSpace(initialStoreFilter))
            {
                StoreFilterBox.Text = initialStoreFilter;
                HeaderSubtitleText.Text = $"Change History & Save Log — Filtered by Store #{initialStoreFilter}";
            }

            AuditHistoryService.HistoryChanged += OnHistoryChanged;
            Closed += (s, e) => AuditHistoryService.HistoryChanged -= OnHistoryChanged;

            ThemeService.ThemeChanged += OnThemeChanged;
            Closed += (s, e) => ThemeService.ThemeChanged -= OnThemeChanged;

            Loaded += (s, e) =>
            {
                ThemeService.ApplyThemeToWindow(this);
                UpdateThemeButtonLabel();
                LoadHistory();
            };
        }

        private void OnThemeChanged(object? sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                UpdateThemeButtonLabel();
                ThemeService.ApplyThemeToWindow(this);
            }));
        }

        private void OnHistoryChanged()
        {
            Dispatcher.BeginInvoke(new Action(LoadHistory));
        }

        private void LoadHistory()
        {
            if (!IsLoaded) return;
            _allEntries = AuditHistoryService.GetEntries();
            LastSavedHeaderBadge.Text = $"Last Saved: {AuditHistoryService.LastSavedTimestamp:HH:mm:ss}";
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            if (!IsLoaded || HistoryDataGrid == null) return;

            var filtered = _allEntries.AsEnumerable();

            if (CategoryFilterCombo?.SelectedItem is ComboBoxItem catItem && catItem.Tag is string catTag && catTag != "ALL")
            {
                filtered = filtered.Where(e => string.Equals(e.Category, catTag, StringComparison.OrdinalIgnoreCase));
            }

            string storeFilter = StoreFilterBox?.Text?.Trim() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(storeFilter))
            {
                filtered = filtered.Where(e =>
                    e.StoreNumber.Contains(storeFilter, StringComparison.OrdinalIgnoreCase) ||
                    e.TargetRecord.Contains(storeFilter, StringComparison.OrdinalIgnoreCase));
            }

            string q = SearchBox?.Text?.Trim() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(q))
            {
                filtered = filtered.Where(e =>
                    e.TargetRecord.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    e.FieldName.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    e.PreviousValue.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    e.NewValue.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    e.OperatorName.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    e.Summary.Contains(q, StringComparison.OrdinalIgnoreCase));
            }

            var list = filtered.ToList();
            HistoryDataGrid.ItemsSource = list;
            EntryCountBadge.Text = $"{list.Count} of {_allEntries.Count} Changes";

            if (list.Count > 0 && HistoryDataGrid.SelectedItem == null)
            {
                HistoryDataGrid.SelectedIndex = 0;
            }
        }

        private void Filter_Changed(object sender, EventArgs e)
        {
            ApplyFilters();
        }

        private void HistoryDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (HistoryDataGrid?.SelectedItem is AuditLogEntry entry)
            {
                DetailTargetText.Text = entry.TargetRecord;
                DetailMetaText.Text = $"{entry.TimestampDisplay}  •  Operator: {entry.OperatorDisplay}  •  ID: {entry.Id}";
                DetailFieldText.Text = $"{entry.Category} — {entry.FieldName}";
                DetailOldValueText.Text = entry.PreviousValue;
                DetailNewValueText.Text = entry.NewValue;
                DetailSummaryText.Text = entry.Summary;
            }
        }

        private void ExportCsvButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog
            {
                Title = "Export Operator Change History to CSV",
                Filter = "CSV Files (*.csv)|*.csv",
                FileName = $"YMInc_ChangeHistory_{DateTime.Now:yyyyMMdd_HHmm}.csv"
            };

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    AuditHistoryService.ExportToCsv(dlg.FileName);
                    FooterStatusText.Text = $"✓ Exported {_allEntries.Count} audit log entries to {Path.GetFileName(dlg.FileName)}";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to export CSV: {ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadHistory();
            FooterStatusText.Text = $"✓ Refreshed audit log at {DateTime.Now:HH:mm:ss}";
        }

        private void ThemeToggleButton_Click(object sender, RoutedEventArgs e)
        {
            ThemeService.ToggleTheme();
            UpdateThemeButtonLabel();
            ApplyFilters();
            Dispatcher.BeginInvoke(new Action(() => ThemeService.ApplyThemeToWindow(this)), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private void UpdateThemeButtonLabel()
        {
            if (ThemeToggleButton != null)
            {
                ThemeToggleButton.Content = ThemeService.ToggleButtonText;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
