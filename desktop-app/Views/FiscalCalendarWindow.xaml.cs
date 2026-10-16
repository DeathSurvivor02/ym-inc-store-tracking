using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using YMInc.StoreDeploymentApp.Models;
using YMInc.StoreDeploymentApp.Services;

namespace YMInc.StoreDeploymentApp.Views
{
    public partial class FiscalCalendarWindow : Window
    {
        private readonly DatabaseService _databaseService = new();
        private List<FiscalCalendarDay> _allDays = new();
        private string _yearFilter = "2026";
        private string _quarterFilter = "ALL";
        private bool _holidaysOnly = false;
        private string _searchQuery = string.Empty;

        public FiscalCalendarWindow()
        {
            InitializeComponent();
            Loaded += FiscalCalendarWindow_Loaded;
        }

        private async void FiscalCalendarWindow_Loaded(object sender, RoutedEventArgs e)
        {
            RecordCountText.Text = "Loading from PostgreSQL...";
            _allDays = await _databaseService.LoadFiscalCalendarAsync();
            ApplyFilters();
        }

        private void Filter_Changed(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded) return;

            if (YearFilterCombo?.SelectedItem is ComboBoxItem yItem && yItem.Tag is string yTag)
            {
                _yearFilter = yTag;
            }

            if (QuarterFilterCombo?.SelectedItem is ComboBoxItem qItem && qItem.Tag is string qTag)
            {
                _quarterFilter = qTag;
            }

            _holidaysOnly = HolidaysOnlyCheckBox?.IsChecked == true;

            ApplyFilters();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _searchQuery = SearchBox.Text;
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            var filtered = _allDays.AsEnumerable();

            // 1. Year filter
            if (_yearFilter != "ALL" && int.TryParse(_yearFilter, out int year))
            {
                filtered = filtered.Where(d => d.FiscalYear == year);
            }

            // 2. Quarter filter
            if (_quarterFilter != "ALL" && int.TryParse(_quarterFilter, out int quarter))
            {
                filtered = filtered.Where(d => d.FiscalQuarter == quarter);
            }

            // 3. Holidays only
            if (_holidaysOnly)
            {
                filtered = filtered.Where(d => d.IsHoliday);
            }

            // 4. Search text
            if (!string.IsNullOrWhiteSpace(_searchQuery))
            {
                string q = _searchQuery.ToLowerInvariant().Trim();
                filtered = filtered.Where(d =>
                    d.DateDisplay.Contains(q) ||
                    d.DayOfWeekName.ToLowerInvariant().Contains(q) ||
                    d.PeriodDisplay.ToLowerInvariant().Contains(q) ||
                    d.WeekDisplay.ToLowerInvariant().Contains(q) ||
                    (d.HolidayName != null && d.HolidayName.ToLowerInvariant().Contains(q))
                );
            }

            var list = filtered.ToList();
            CalendarDataGrid.ItemsSource = list;
            RecordCountText.Text = $"Showing {list.Count:N0} of {_allDays.Count:N0} Dates";
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
