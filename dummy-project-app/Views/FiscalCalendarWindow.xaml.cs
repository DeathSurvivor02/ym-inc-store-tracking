using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using YMInc.StoreDeploymentApp.Models;
using YMInc.StoreDeploymentApp.Services;

namespace YMInc.StoreDeploymentApp.Views
{
    public partial class FiscalCalendarWindow : Window
    {
        private readonly DatabaseService _databaseService = new();
        private List<FiscalCalendarDay> _allDays = new();
        private List<StoreDeployment> _allStores = new();
        private List<CalendarWorkItem> _allWorkItems = new();
        private FiscalCalendarDay? _selectedDay;

        private int _selectedFiscalWeek = 41;
        private string _yearFilter = "2026";
        private string _quarterFilter = "3";
        private string _monthFilter = "October";
        private bool _holidaysOnly = false;
        private bool _focusSelectedWeekOnly = false;
        private bool _onlyActiveDays = false;
        private string _searchQuery = string.Empty;
        private bool _isUpdatingWeekSelector = false;

        public FiscalCalendarWindow(int initialFiscalWeek = 41)
        {
            InitializeComponent();
            _selectedFiscalWeek = Math.Clamp(initialFiscalWeek, 1, 52);
            ApplyWorkspaceBadge();
            PopulateFiscalWeekComboBox();
            UpdateWeekButtonsVisualState();
            UpdateThemeButtonLabel();
            ThemeService.ThemeChanged += OnThemeChanged;
            Closed += (s, e) => ThemeService.ThemeChanged -= OnThemeChanged;
            Loaded += FiscalCalendarWindow_Loaded;
        }

        private void OnThemeChanged(object? sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                UpdateThemeButtonLabel();
                UpdateWeekButtonsVisualState();
                UpdateSelectedWeekProgressionBanner();
                ApplyFilters(_selectedDay?.DateDisplay);
                ThemeService.ApplyThemeToWindow(this);
            }));
        }

        private void ThemeToggleButton_Click(object sender, RoutedEventArgs e)
        {
            ThemeService.ToggleTheme();
        }

        private void UpdateThemeButtonLabel()
        {
            if (ThemeToggleButton != null)
            {
                ThemeToggleButton.Content = ThemeService.ToggleButtonText;
            }
        }

        private void ApplyWorkspaceBadge()
        {
            CalWorkspaceBadge.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0C5596"));
            CalWorkspaceBadge.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#62ABF5"));
            CalWorkspaceBadgeText.Text = "ENVIRONMENT: PRODUCTION DB";
            CalWorkspaceBadgeText.Foreground = Brushes.White;
        }

        private async void FiscalCalendarWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ThemeService.ApplyThemeToWindow(this);
            await ReloadAllCalendarDataAsync();
        }

        private async System.Threading.Tasks.Task ReloadAllCalendarDataAsync(string? preserveSelectedDateIso = null)
        {
            RecordCountText.Text = "Loading...";
            _allStores = await _databaseService.LoadStoresAsync();
            _allWorkItems = await _databaseService.LoadCalendarWorkItemsAsync();
            _allDays = await _databaseService.LoadFiscalCalendarAsync();

            PopulateStoreCombo();
            UpdateSelectedWeekProgressionBanner();
            ApplyFilters(preserveSelectedDateIso);
            Dispatcher.BeginInvoke(new Action(() => ThemeService.ApplyThemeToWindow(this)), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private void PopulateStoreCombo()
        {
            int previousIndex = NewActivityStoreCombo.SelectedIndex;
            NewActivityStoreCombo.Items.Clear();

            foreach (var store in _allStores.OrderBy(s => s.StoreNumber))
            {
                NewActivityStoreCombo.Items.Add(new ComboBoxItem
                {
                    Content = $"{store.StoreNumber} — {store.StoreName} ({store.City})",
                    Tag = store
                });
            }

            NewActivityStoreCombo.Items.Add(new ComboBoxItem
            {
                Content = "FLEET — Multi-Store / Regional Wave",
                Tag = null
            });

            if (NewActivityStoreCombo.Items.Count > 0)
            {
                NewActivityStoreCombo.SelectedIndex = previousIndex >= 0 && previousIndex < NewActivityStoreCombo.Items.Count
                    ? previousIndex
                    : 0;
            }
        }

        private void PopulateFiscalWeekComboBox()
        {
            if (CalFiscalWeekComboBox == null) return;
            CalFiscalWeekComboBox.Items.Clear();

            for (int w = 1; w <= 52; w++)
            {
                var (start, end) = DatabaseService.GetDateRangeForFiscalWeek(w);
                var item = new ComboBoxItem
                {
                    Content = $"WK {w:D2} ({start:MMM dd} – {end:MMM dd})",
                    Tag = w.ToString(),
                    FontFamily = (FontFamily)FindResource("MonoFont"),
                    FontSize = 11
                };
                CalFiscalWeekComboBox.Items.Add(item);
            }
        }

        private void UpdateWeekButtonsVisualState()
        {
            if (CalFiscalWeekComboBox == null) return;
            _isUpdatingWeekSelector = true;
            try
            {
                foreach (var item in CalFiscalWeekComboBox.Items)
                {
                    if (item is ComboBoxItem cbi && cbi.Tag?.ToString() == _selectedFiscalWeek.ToString())
                    {
                        if (CalFiscalWeekComboBox.SelectedItem != cbi)
                        {
                            CalFiscalWeekComboBox.SelectedItem = cbi;
                        }
                        cbi.BringIntoView();
                        break;
                    }
                }
            }
            finally
            {
                _isUpdatingWeekSelector = false;
            }
        }

        private void UpdateSelectedWeekProgressionBanner()
        {
            var report = DatabaseService.GetWeeklyReportFromFleet(_selectedFiscalWeek, _allStores, _allWorkItems);

            BannerWeekTitle.Text = $"WEEK {report.FiscalWeek} PROGRESSION";
            int delta = report.DeltaVsPriorWeekPct;
            BannerWeekDelta.Text = delta >= 0 ? $"+{delta}% vs Wk {report.FiscalWeek - 1}" : $"{delta}% vs Wk {report.FiscalWeek - 1}";
            BannerWeekDelta.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(delta >= 0 ? "#107C10" : "#B45309"));

            BannerFleetPct.Text = $"{report.OverallProgressPct}%";
            BannerWeekDates.Text = report.WeekDateRange;
            BannerStoreBreakdown.Text = $"{report.CompletedThisWeek} Certified • {report.InProgress} In Progress • {report.ScheduledPending} Pending • {report.BlockedInJeopardy} Blocked";

            int taskPct = report.WeekTasksTotal > 0
                ? (int)Math.Round(report.WeekTasksCompleted * 100.0 / report.WeekTasksTotal)
                : 0;
            BannerTaskCountText.Text = $"{report.WeekTasksCompleted} / {report.WeekTasksTotal} Completed ({taskPct}%)";
            BannerWeekTaskBar.Value = taskPct;
            BannerWeekNarrative.Text = report.WeekNarrative;

            BadgeTtuCount.Text = $"TTU (Test & Turn-Up): {report.TtuCount}";
            BadgeCircuitCount.Text = $"Circuit Installs: {report.CircuitInstallCount}";
            BadgeCablingCount.Text = $"Low-Voltage Cabling: {report.CablingCount}";
            BadgePoleCount.Text = $"Power Poles: {report.PowerPoleCount}";
            BadgeSurveyCount.Text = $"Site Surveys: {report.SurveyCount}";
        }

        private void SelectFiscalWeek(int week)
        {
            _selectedFiscalWeek = Math.Clamp(week, 1, 52);
            UpdateWeekButtonsVisualState();
            UpdateSelectedWeekProgressionBanner();

            var firstDayOfWeek = _allDays.FirstOrDefault(d => d.FiscalYear == 2026 && d.FiscalWeek == _selectedFiscalWeek);
            if (firstDayOfWeek != null)
            {
                if (_monthFilter != "ALL" && !string.Equals(_monthFilter, firstDayOfWeek.FiscalMonthName, StringComparison.OrdinalIgnoreCase))
                {
                    foreach (ComboBoxItem item in MonthFilterCombo.Items)
                    {
                        if (item.Tag is string tag && tag.Equals(firstDayOfWeek.FiscalMonthName, StringComparison.OrdinalIgnoreCase))
                        {
                            MonthFilterCombo.SelectedItem = item;
                            break;
                        }
                    }
                }
            }

            ApplyFilters(firstDayOfWeek?.DateDisplay);
        }

        private void CalFiscalWeekComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded || _isUpdatingWeekSelector) return;
            if (CalFiscalWeekComboBox?.SelectedItem is ComboBoxItem item && int.TryParse(item.Tag?.ToString(), out int week))
            {
                if (week != _selectedFiscalWeek)
                {
                    SelectFiscalWeek(week);
                }
            }
        }

        private void PrevWeekButton_Click(object sender, RoutedEventArgs e)
        {
            SelectFiscalWeek(_selectedFiscalWeek - 1);
        }

        private void NextWeekButton_Click(object sender, RoutedEventArgs e)
        {
            SelectFiscalWeek(_selectedFiscalWeek + 1);
        }

        private void WeekBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string tag && int.TryParse(tag, out int wk))
            {
                SelectFiscalWeek(wk);
            }
        }

        private void Filter_Changed(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded) return;

            if (YearFilterCombo?.SelectedItem is ComboBoxItem yItem && yItem.Tag is string yTag)
                _yearFilter = yTag;

            if (QuarterFilterCombo?.SelectedItem is ComboBoxItem qItem && qItem.Tag is string qTag)
                _quarterFilter = qTag;

            if (MonthFilterCombo?.SelectedItem is ComboBoxItem mItem && mItem.Tag is string mTag)
                _monthFilter = mTag;

            _holidaysOnly = HolidaysOnlyCheckBox?.IsChecked == true;
            _focusSelectedWeekOnly = FocusSelectedWeekCheckBox?.IsChecked == true;
            _onlyActiveDays = OnlyActiveDaysCheckBox?.IsChecked == true;

            ApplyFilters(_selectedDay?.DateDisplay);
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsLoaded || SearchBox == null) return;
            _searchQuery = SearchBox.Text;
            ApplyFilters(_selectedDay?.DateDisplay);
        }

        private void ApplyFilters(string? preferredDateIso = null)
        {
            if (DayCardsItemsControl == null || CalendarDataGrid == null || RecordCountText == null) return;
            var filtered = _allDays.AsEnumerable();

            if (_yearFilter != "ALL" && int.TryParse(_yearFilter, out int year))
            {
                filtered = filtered.Where(d => d.FiscalYear == year);
            }

            if (_focusSelectedWeekOnly)
            {
                filtered = filtered.Where(d => d.FiscalWeek == _selectedFiscalWeek);
            }
            else
            {
                if (_quarterFilter != "ALL" && int.TryParse(_quarterFilter, out int quarter))
                {
                    filtered = filtered.Where(d => d.FiscalQuarter == quarter);
                }

                if (_monthFilter != "ALL")
                {
                    filtered = filtered.Where(d => d.FiscalMonthName.Equals(_monthFilter, StringComparison.OrdinalIgnoreCase));
                }
            }

            if (_holidaysOnly)
            {
                filtered = filtered.Where(d => d.IsHoliday);
            }

            if (_onlyActiveDays)
            {
                filtered = filtered.Where(d => d.HasWorkItems);
            }

            if (!string.IsNullOrWhiteSpace(_searchQuery))
            {
                string q = _searchQuery.ToLowerInvariant().Trim();
                filtered = filtered.Where(d =>
                    d.DateDisplay.Contains(q) ||
                    d.DayOfWeekName.ToLowerInvariant().Contains(q) ||
                    d.PeriodDisplay.ToLowerInvariant().Contains(q) ||
                    d.WeekDisplay.ToLowerInvariant().Contains(q) ||
                    (d.HolidayName != null && d.HolidayName.ToLowerInvariant().Contains(q)) ||
                    d.WorkItems.Any(w =>
                        w.ActivityType.ToLowerInvariant().Contains(q) ||
                        w.StoreNumber.ToLowerInvariant().Contains(q) ||
                        w.StoreName.ToLowerInvariant().Contains(q) ||
                        w.TechnicianOrVendor.ToLowerInvariant().Contains(q) ||
                        w.Notes.ToLowerInvariant().Contains(q))
                );
            }

            var list = filtered.ToList();
            DayCardsItemsControl.ItemsSource = list;
            CalendarDataGrid.ItemsSource = list;

            int totalActivitiesInView = list.Sum(d => d.WorkItems.Count);
            RecordCountText.Text = $"{list.Count:N0} Days | {totalActivitiesInView:N0} Work Orders";

            FiscalCalendarDay? dayToSelect = null;
            if (!string.IsNullOrEmpty(preferredDateIso))
            {
                dayToSelect = list.FirstOrDefault(d => d.DateDisplay == preferredDateIso)
                              ?? _allDays.FirstOrDefault(d => d.DateDisplay == preferredDateIso);
            }

            dayToSelect ??= list.FirstOrDefault(d => d.FiscalWeek == _selectedFiscalWeek && d.HasWorkItems)
                            ?? list.FirstOrDefault(d => d.FiscalWeek == _selectedFiscalWeek)
                            ?? list.FirstOrDefault();

            if (dayToSelect != null)
            {
                SetSelectedDay(dayToSelect);
            }

            Dispatcher.BeginInvoke(new Action(() => ThemeService.ApplyThemeToWindow(this)), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private void SetSelectedDay(FiscalCalendarDay day)
        {
            _selectedDay = day;
            SelectedDayFiscalMeta.Text = $"FY{day.FiscalYear} • Q{day.FiscalQuarter} • {day.WeekDisplay} • Day {day.DayOfFiscalWeek}";
            SelectedDayTitleText.Text = $"{day.DayOfWeekName}, {day.CalendarDate:MMM dd, yyyy}";

            if (day.IsHoliday)
            {
                SelectedDaySubText.Text = $"Retail Holiday: {day.HolidayName} | {day.WorkItems.Count} work order(s)";
            }
            else
            {
                SelectedDaySubText.Text = day.WorkItems.Count == 0
                    ? "No work orders scheduled — add TTU, Circuit, or Cabling work below"
                    : $"{day.WorkItems.Count} work order(s) scheduled ({day.WorkItems.Count(w => w.IsCompleted)} completed)";
            }

            EmptyDayNoticeBorder.Visibility = day.WorkItems.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            SelectedDayActivitiesList.ItemsSource = null;
            SelectedDayActivitiesList.ItemsSource = day.WorkItems;

            if (day.FiscalYear == 2026 && _selectedFiscalWeek != day.FiscalWeek)
            {
                _selectedFiscalWeek = day.FiscalWeek;
                UpdateWeekButtonsVisualState();
                UpdateSelectedWeekProgressionBanner();
            }

            Dispatcher.BeginInvoke(new Action(() => ThemeService.ApplyThemeToWindow(this)), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private void DayCard_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement el && el.DataContext is FiscalCalendarDay day)
            {
                SetSelectedDay(day);
            }
        }

        private void CalendarDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CalendarDataGrid.SelectedItem is FiscalCalendarDay day)
            {
                SetSelectedDay(day);
            }
        }

        private async void AddActivityButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedDay == null)
            {
                MessageBox.Show("Please select a calendar day first.", "Select Date", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string activityType = (NewActivityTypeCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "TTU (Test & Turn-Up)";
            string status = (NewActivityStatusCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Scheduled";
            string timeWindow = (NewActivityWindowCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "08:00 - 12:00";
            string vendor = string.IsNullOrWhiteSpace(NewActivityVendorBox.Text) ? "Field Tech" : NewActivityVendorBox.Text.Trim();
            string notes = string.IsNullOrWhiteSpace(NewActivityNotesBox.Text)
                ? $"{activityType} scheduled for {_selectedDay.DateDisplay}."
                : NewActivityNotesBox.Text.Trim();

            string storeNumber = "FLEET";
            string storeName = "Multi-Store Wave";
            if (NewActivityStoreCombo.SelectedItem is ComboBoxItem storeItem && storeItem.Tag is StoreDeployment store)
            {
                storeNumber = store.StoreNumber;
                storeName = store.StoreName;
            }

            var newItem = new CalendarWorkItem
            {
                Id = Guid.NewGuid().ToString("N")[..10],
                DateIso = _selectedDay.DateDisplay,
                FiscalYear = _selectedDay.FiscalYear,
                FiscalWeek = _selectedDay.FiscalWeek,
                StoreNumber = storeNumber,
                StoreName = storeName,
                ActivityType = activityType,
                Status = status,
                TechnicianOrVendor = vendor,
                TimeWindow = timeWindow,
                Notes = notes
            };

            await _databaseService.SaveCalendarWorkItemAsync(newItem);
            AuditHistoryService.RecordChange(
                "Daily Schedule",
                $"{_selectedDay.DateDisplay} — Store #{storeNumber} ({storeName})",
                $"Scheduled {activityType}",
                "(None)",
                $"{status} ({vendor}, {timeWindow})",
                $"Scheduled {activityType} for Store #{storeNumber} on {_selectedDay.DateDisplay}: {notes}",
                storeNumber);

            NewActivityNotesBox.Text = string.Empty;

            FooterStatusText.Text = $"✓ Saved '{activityType}' for {storeNumber} on {_selectedDay.DateDisplay} ({_selectedDay.WeekDisplay}).";
            await ReloadAllCalendarDataAsync(_selectedDay.DateDisplay);
        }

        private async void MarkActivityDone_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is CalendarWorkItem item)
            {
                string prev = item.Status;
                item.Status = "Completed";
                await _databaseService.SaveCalendarWorkItemAsync(item);
                AuditHistoryService.RecordChange(
                    "Daily Schedule",
                    $"{item.DateIso} — Store #{item.StoreNumber} ({item.StoreName})",
                    $"{item.ActivityType} Status",
                    prev,
                    "Completed",
                    $"Marked '{item.ActivityType}' for Store #{item.StoreNumber} on {item.DateIso} as Completed.",
                    item.StoreNumber);
                FooterStatusText.Text = $"✓ Saved '{item.ActivityType}' ({item.StoreNumber}) as Completed.";
                await ReloadAllCalendarDataAsync(_selectedDay?.DateDisplay);
            }
        }

        private async void MarkActivityInProgress_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is CalendarWorkItem item)
            {
                string prev = item.Status;
                item.Status = "In Progress";
                await _databaseService.SaveCalendarWorkItemAsync(item);
                AuditHistoryService.RecordChange(
                    "Daily Schedule",
                    $"{item.DateIso} — Store #{item.StoreNumber} ({item.StoreName})",
                    $"{item.ActivityType} Status",
                    prev,
                    "In Progress",
                    $"Marked '{item.ActivityType}' for Store #{item.StoreNumber} on {item.DateIso} as In Progress.",
                    item.StoreNumber);
                FooterStatusText.Text = $"✓ Saved '{item.ActivityType}' ({item.StoreNumber}) as In Progress.";
                await ReloadAllCalendarDataAsync(_selectedDay?.DateDisplay);
            }
        }

        private async void MarkActivityBlocked_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is CalendarWorkItem item)
            {
                string prev = item.Status;
                item.Status = "Blocked";
                await _databaseService.SaveCalendarWorkItemAsync(item);
                AuditHistoryService.RecordChange(
                    "Daily Schedule",
                    $"{item.DateIso} — Store #{item.StoreNumber} ({item.StoreName})",
                    $"{item.ActivityType} Status",
                    prev,
                    "Blocked",
                    $"Flagged '{item.ActivityType}' for Store #{item.StoreNumber} on {item.DateIso} as Blocked.",
                    item.StoreNumber);
                FooterStatusText.Text = $"✓ Saved '{item.ActivityType}' ({item.StoreNumber}) as Blocked.";
                await ReloadAllCalendarDataAsync(_selectedDay?.DateDisplay);
            }
        }

        private async void DeleteActivity_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is CalendarWorkItem item)
            {
                await _databaseService.DeleteCalendarWorkItemAsync(item.Id);
                AuditHistoryService.RecordChange(
                    "Daily Schedule",
                    $"{item.DateIso} — Store #{item.StoreNumber} ({item.StoreName})",
                    $"Deleted {item.ActivityType}",
                    $"{item.Status} ({item.TechnicianOrVendor})",
                    "Deleted",
                    $"Removed '{item.ActivityType}' for Store #{item.StoreNumber} from {item.DateIso}.",
                    item.StoreNumber);
                FooterStatusText.Text = $"✓ Removed '{item.ActivityType}' ({item.StoreNumber}) from {item.DateIso}.";
                await ReloadAllCalendarDataAsync(_selectedDay?.DateDisplay);
            }
        }

        private async void SaveScheduleButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in _allWorkItems)
            {
                await _databaseService.SaveCalendarWorkItemAsync(item);
            }
            AuditHistoryService.RecordChange(
                "System Save",
                $"Fiscal Calendar Schedule ({_allWorkItems.Count} Work Orders)",
                "Schedule Save Checkpoint",
                "Active Calendar State",
                $"Committed at {DateTime.Now:HH:mm:ss}",
                $"Operator manually saved all {_allWorkItems.Count} scheduled daily work orders.");
            FooterStatusText.Text = $"✓ All {_allWorkItems.Count} scheduled work orders saved at {DateTime.Now:HH:mm:ss}.";
        }

        private void OpenHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            var historyWin = new ActivityHistoryWindow { Owner = this };
            historyWin.ShowDialog();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
