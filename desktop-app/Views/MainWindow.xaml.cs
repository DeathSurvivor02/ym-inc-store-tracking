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
    public partial class MainWindow : Window
    {
        private readonly User _currentUser;
        private readonly DatabaseService _databaseService = new();
        private List<StoreDeployment> _allStores = new();
        private List<CalendarWorkItem> _allWorkItems = new();
        private int _currentWeek = 42;
        private bool _isShowingHome = true;
        private string? _selectedSliceFilter;
        private string _countryFilter = "ALL";
        private string _bannerFilter = "ALL";
        private string _searchFieldFilter = "ALL";
        private string _searchQuery = string.Empty;
        private string _cellOriginalValue = string.Empty;
        private bool _isInitialized = false;
        private bool _isUpdatingWeekSelector = false;

        public MainWindow(User user)
        {
            InitializeComponent();
            _currentUser = user;
            ApplyUserContext();
            ApplyWorkspaceChrome();
            SetupDonutEvents();
            PopulateFiscalWeekComboBox();
            _isInitialized = true;
            UpdateWeekButtonsVisualState();
            UpdateThemeButtonLabel();
            UpdateHistoryBadge();
            AuditHistoryService.HistoryChanged += UpdateHistoryBadge;
            PreviewKeyDown += MainWindow_PreviewKeyDown;
            ThemeService.ThemeChanged += (_, _) =>
            {
                UpdateThemeButtonLabel();
                UpdateNavigationHeader();
                UpdateWeekButtonsVisualState();
                LoadWeeklyProgressReport();
                Dispatcher.BeginInvoke(new Action(() => ThemeService.ApplyThemeToWindow(this)), System.Windows.Threading.DispatcherPriority.Loaded);
            };
            Loaded += MainWindow_Loaded;
            InitializeLiveSync();
        }

        private System.Windows.Threading.DispatcherTimer? _liveSyncTimer;
        private DateTime _lastKnownStoreUpdateUtc = DateTime.MinValue;

        private void InitializeLiveSync()
        {
            DatabaseService.StartCalendarWatcher();
            DatabaseService.CalendarChangedExternally += OnExternalCalendarChanged;

            _liveSyncTimer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(6)
            };
            _liveSyncTimer.Tick += LiveSyncTimer_Tick;
            _liveSyncTimer.Start();

            Closed += (_, _) =>
            {
                _liveSyncTimer?.Stop();
                DatabaseService.CalendarChangedExternally -= OnExternalCalendarChanged;
            };
        }

        private async void LiveSyncTimer_Tick(object? sender, EventArgs e)
        {
            if (!_isInitialized || !IsVisible) return;
            try
            {
                var latest = await _databaseService.GetLatestStoreUpdateTimestampAsync();
                if (latest.HasValue && latest.Value > _lastKnownStoreUpdateUtc)
                {
                    if (_lastKnownStoreUpdateUtc != DateTime.MinValue)
                    {
                        await LoadStoresDataAsync();
                        if (LastSaveStatusText != null)
                        {
                            LastSaveStatusText.Text = $"⚡ Auto-synced from database at {DateTime.Now:HH:mm:ss}";
                            LastSaveStatusText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#15803D"));
                        }
                    }
                    _lastKnownStoreUpdateUtc = latest.Value;
                }
            }
            catch { }
        }

        private void OnExternalCalendarChanged()
        {
            Dispatcher.BeginInvoke(new Action(async () =>
            {
                _allWorkItems = await _databaseService.LoadCalendarWorkItemsAsync(_allStores);
                LoadWeeklyProgressReport();
                if (LastSaveStatusText != null)
                {
                    LastSaveStatusText.Text = $"⚡ Calendar auto-synced from another instance at {DateTime.Now:HH:mm:ss}";
                    LastSaveStatusText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#15803D"));
                }
            }));
        }

        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.S && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                e.Handled = true;
                SaveAllChangesButton_Click(this, new RoutedEventArgs());
            }
        }

        private void UpdateHistoryBadge()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                var entries = AuditHistoryService.GetEntries();
                if (HistoryToolbarButton != null)
                {
                    HistoryToolbarButton.Content = $"🕒 Change History ({entries.Count})";
                }
                if (LastSaveStatusText != null)
                {
                    LastSaveStatusText.Text = $"✓ Saved {AuditHistoryService.LastSavedTimestamp:HH:mm:ss}";
                }
            }));
        }

        private void ThemeToggleButton_Click(object sender, RoutedEventArgs e)
        {
            ThemeService.ToggleTheme();
            UpdateThemeButtonLabel();
            UpdateNavigationHeader();
            UpdateWeekButtonsVisualState();
            LoadWeeklyProgressReport();
            Dispatcher.BeginInvoke(new Action(() => ThemeService.ApplyThemeToWindow(this)), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private void UpdateThemeButtonLabel()
        {
            if (ThemeToggleButton != null)
            {
                ThemeToggleButton.Content = ThemeService.ToggleButtonText;
            }
        }

        private void ApplyUserContext()
        {
            if (_currentUser == null) return;
            if (CurrentUserNameText != null) CurrentUserNameText.Text = _currentUser.FullName;
            if (CurrentUserRoleBadge != null) CurrentUserRoleBadge.Text = $"({_currentUser.RoleDisplayName})";

            if (ImportRosterButton != null)
            {
                ImportRosterButton.IsEnabled = _currentUser.CanImportRoster;
                if (!_currentUser.CanImportRoster)
                {
                    ImportRosterButton.ToolTip = "Roster import requires PM or PA role.";
                }
            }
        }

        private void ApplyWorkspaceChrome()
        {
            WorkspaceModePill.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DBEAFE"));
            WorkspaceModePill.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0F6CBD"));
            WorkspaceModePillText.Text = "LIVE DB";
            WorkspaceModePillText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E3A8A"));

            ActiveProjectTitleText.Text = "[Environment: Production Database (PostgreSQL / Corporate Roster)]";
            ActiveProjectSubText.Text = "Primary deployment database";

            TopWorkspaceBadge.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0C5596"));
            TopWorkspaceBadge.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#62ABF5"));
            TopWorkspaceBadgeText.Text = "ENVIRONMENT: PRODUCTION DB";
            TopWorkspaceBadgeText.Foreground = Brushes.White;

            UpdateNavigationHeader();
        }

        private void UpdateNavigationHeader()
        {
            bool dark = ThemeService.IsDarkTheme;
            var activeBg = new SolidColorBrush((Color)ColorConverter.ConvertFromString(dark ? "#0F6CBD" : "#FFFFFF"));
            var activeFg = new SolidColorBrush((Color)ColorConverter.ConvertFromString(dark ? "#FFFFFF" : "#1E293B"));
            var inactiveBg = Brushes.Transparent;
            var inactiveFg = new SolidColorBrush((Color)ColorConverter.ConvertFromString(dark ? "#94A3B8" : "#64748B"));

            if (_isShowingHome)
            {
                PageHeaderTitle.Text = "Home — Project Summary (Production DB)";

                NavHomeButton.Background = activeBg;
                NavHomeButton.Foreground = activeFg;
                NavHomeButton.FontWeight = FontWeights.SemiBold;

                NavStoresButton.Background = inactiveBg;
                NavStoresButton.Foreground = inactiveFg;
                NavStoresButton.FontWeight = FontWeights.Normal;

                HomeDashboardView.Visibility = Visibility.Visible;
                StoreFleetExplorerView.Visibility = Visibility.Collapsed;
            }
            else
            {
                PageHeaderTitle.Text = "Store Directory & Specifications (Production DB)";

                NavStoresButton.Background = activeBg;
                NavStoresButton.Foreground = activeFg;
                NavStoresButton.FontWeight = FontWeights.SemiBold;

                NavHomeButton.Background = inactiveBg;
                NavHomeButton.Foreground = inactiveFg;
                NavHomeButton.FontWeight = FontWeights.Normal;

                HomeDashboardView.Visibility = Visibility.Collapsed;
                StoreFleetExplorerView.Visibility = Visibility.Visible;
            }
        }

        private void SetupDonutEvents()
        {
            DonutChart.SliceClicked += (s, sliceName) =>
            {
                _selectedSliceFilter = sliceName;
                FilterActiveNotice.Visibility = string.IsNullOrEmpty(sliceName) ? Visibility.Collapsed : Visibility.Visible;
                if (!string.IsNullOrEmpty(sliceName))
                {
                    FilterActiveNotice.Text = $"Filter: {sliceName}";
                }
                ApplyFilters();
            };
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadStoresDataAsync();
        }

        private async System.Threading.Tasks.Task LoadStoresDataAsync()
        {
            ApplyWorkspaceChrome();
            DbStatusText.Text = "Connecting to PostgreSQL 16...";

            _allStores = await _databaseService.LoadStoresAsync();
            _allWorkItems = await _databaseService.LoadCalendarWorkItemsAsync(_allStores);

            DbStatusText.Text = _databaseService.StatusMessage;
            DbStatusText.Foreground = Brushes.White;

            LoadWeeklyProgressReport();
            ApplyFilters();

            if (HomeStoresPreviewGrid != null && HomeStoresPreviewGrid.SelectedItem == null && HomeStoresPreviewGrid.Items.Count > 0)
            {
                HomeStoresPreviewGrid.SelectedIndex = 0;
            }
        }

        private void LoadWeeklyProgressReport()
        {
            if (!_isInitialized || DonutChart == null || WeekNarrativeText == null || KpiWeekLabel == null) return;

            var report = DatabaseService.GetWeeklyReportFromFleet(_currentWeek, _allStores, _allWorkItems);
            if (report == null) return;

            DonutChart.Report = report;

            WeekNarrativeText.Text = report.WeekNarrative;
            KpiWeekLabel.Text = $"WEEK {report.FiscalWeek} READINESS";
            KpiOverallPctText.Text = $"{report.OverallProgressPct}%";
            KpiWeekDateRangeText.Text = $"NRF Week {report.FiscalWeek} ({report.WeekDateRange})";

            int delta = report.DeltaVsPriorWeekPct;
            KpiDeltaBadge.Text = delta >= 0 ? $"+{delta}% vs Wk {report.FiscalWeek - 1}" : $"{delta}% vs Wk {report.FiscalWeek - 1}";
            KpiDeltaBadge.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(delta >= 0 ? "#15803D" : "#B45309"));

            int taskExecPct = report.WeekTasksTotal > 0
                ? (int)Math.Round(report.WeekTasksCompleted * 100.0 / report.WeekTasksTotal)
                : 0;
            KpiTaskExecPctBadge.Text = $"{taskExecPct}% Completed";
            KpiWeekTasksText.Text = $"{report.WeekTasksCompleted} / {report.WeekTasksTotal} Tasks";
            KpiWeekCategoryBreakdownText.Text = $"TTU: {report.TtuCount} | Circuits: {report.CircuitInstallCount} | Cabling: {report.CablingCount} | Poles: {report.PowerPoleCount}";

            KpiActiveFleetBadge.Text = $"{report.TotalActiveFleet} Stores";
            KpiCertifiedStoresText.Text = $"{report.CompletedThisWeek} Ready";
            KpiInProgressStoresText.Text = $"{report.InProgress} In Progress | {report.ScheduledPending} Pending";
            KpiBlockedStoresText.Text = $"{report.BlockedInJeopardy} Blocked";
            KpiBlockedSubText.Text = report.BlockedInJeopardy > 0
                ? "Requires landlord or carrier escalation"
                : "No stores blocked in selected week";

            DonutWeekBadgeText.Text = $"FISCAL WEEK {report.FiscalWeek}";

            if (report.Slices.Count >= 4)
            {
                CompletedCountText.Text = $"{report.CompletedThisWeek} ({(int)report.Slices[0].Percentage}%)";
                InProgressCountText.Text = $"{report.InProgress} ({(int)report.Slices[1].Percentage}%)";
                PendingCountText.Text = $"{report.ScheduledPending} ({(int)report.Slices[2].Percentage}%)";
                BlockedCountText.Text = $"{report.BlockedInJeopardy} ({(int)report.Slices[3].Percentage}%)";
            }

            if (report.Milestones.Count >= 4)
            {
                var m1 = report.Milestones[0];
                Milestone1Text.Text = $"{m1.Completed}/{m1.Total} ({m1.Pct}%)";
                Milestone1Bar.Value = m1.Pct;

                var m2 = report.Milestones[1];
                Milestone2Text.Text = $"{m2.Completed}/{m2.Total} ({m2.Pct}%)";
                Milestone2Bar.Value = m2.Pct;

                var m3 = report.Milestones[2];
                Milestone3Text.Text = $"{m3.Completed}/{m3.Total} ({m3.Pct}%)";
                Milestone3Bar.Value = m3.Pct;

                var m4 = report.Milestones[3];
                Milestone4Text.Text = $"{m4.Completed}/{m4.Total} ({m4.Pct}%)";
                Milestone4Bar.Value = m4.Pct;
            }

            if (report.Milestones.Count >= 5)
            {
                var m5 = report.Milestones[4];
                Milestone5Text.Text = $"{m5.Completed}/{m5.Total} ({m5.Pct}%)";
                Milestone5Bar.Value = m5.Pct;
            }

            WeekTrendItemsControl.ItemsSource = report.WeeklyHistoryTrend;

            WeekScheduleFeedHeader.Text = $"WEEK {report.FiscalWeek} SCHEDULED FIELD WORK ({report.WeekDateRange})";
            if (report.WeekActivities.Count > 0)
            {
                HomeWeekActivitiesList.ItemsSource = report.WeekActivities;
            }
            else
            {
                HomeWeekActivitiesList.ItemsSource = new List<CalendarWorkItem>
                {
                    new()
                    {
                        DateIso = DateTime.Today.ToString("yyyy-MM-dd"),
                        ActivityType = "No Work Scheduled",
                        StoreNumber = "—",
                        StoreName = "Click 'Add / Edit Daily Schedule...' to add TTU or Circuit work",
                        TechnicianOrVendor = "—",
                        TimeWindow = "—",
                        Status = "Scheduled",
                        Notes = $"No field tasks recorded yet for Week {report.FiscalWeek}"
                    }
                };
            }

            Dispatcher.BeginInvoke(new Action(() => ThemeService.ApplyThemeToWindow(this)), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private void ApplyFilters()
        {
            if (StoresDataGrid == null || HomeStoresPreviewGrid == null) return;

            var filtered = _allStores.AsEnumerable();

            if (_countryFilter != "ALL")
            {
                filtered = filtered.Where(s => s.CountryCode == _countryFilter);
            }

            if (_bannerFilter != "ALL")
            {
                filtered = filtered.Where(s => s.BannerCode == _bannerFilter);
            }

            if (!string.IsNullOrEmpty(_selectedSliceFilter))
            {
                filtered = filtered.Where(s => s.WeeklyStatus.Equals(_selectedSliceFilter, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(_searchQuery))
            {
                string q = _searchQuery.ToLowerInvariant().Trim();
                filtered = filtered.Where(s =>
                {
                    return _searchFieldFilter switch
                    {
                        "MALL" => s.MallName.ToLowerInvariant().Contains(q),
                        "ADDRESS" => s.Address.ToLowerInvariant().Contains(q) || s.FullAddress.ToLowerInvariant().Contains(q),
                        "CITY" => s.City.ToLowerInvariant().Contains(q) || s.StateProvince.ToLowerInvariant().Contains(q),
                        "STORE_NO" => s.StoreNumber.ToLowerInvariant().Contains(q),
                        "PHONE" => (s.PhoneNumber != null && s.PhoneNumber.ToLowerInvariant().Contains(q)) ||
                                   (s.PrimaryContactName != null && s.PrimaryContactName.ToLowerInvariant().Contains(q)) ||
                                   (s.PrimaryContactPhone != null && s.PrimaryContactPhone.ToLowerInvariant().Contains(q)),
                        _ => s.StoreNumber.ToLowerInvariant().Contains(q) ||
                             s.StoreName.ToLowerInvariant().Contains(q) ||
                             s.MallName.ToLowerInvariant().Contains(q) ||
                             s.Address.ToLowerInvariant().Contains(q) ||
                             s.City.ToLowerInvariant().Contains(q) ||
                             s.StateProvince.ToLowerInvariant().Contains(q) ||
                             (s.PhoneNumber != null && s.PhoneNumber.ToLowerInvariant().Contains(q)) ||
                             (s.PrimaryContactName != null && s.PrimaryContactName.ToLowerInvariant().Contains(q)) ||
                             s.BannerName.ToLowerInvariant().Contains(q) ||
                             s.BannerCode.ToLowerInvariant().Contains(q) ||
                             s.OperationalStatus.ToLowerInvariant().Contains(q)
                    };
                });
            }

            var list = filtered.ToList();
            StoresDataGrid.ItemsSource = list;
            HomeStoresPreviewGrid.ItemsSource = list;

            if (StoresDataGrid.SelectedItem == null && list.Count > 0)
            {
                StoresDataGrid.SelectedItem = list[0];
            }
        }

        private void NavHomeButton_Click(object sender, RoutedEventArgs e)
        {
            _isShowingHome = true;
            UpdateNavigationHeader();
        }

        private void NavStoresButton_Click(object sender, RoutedEventArgs e)
        {
            _isShowingHome = false;
            UpdateNavigationHeader();
        }

        private void UpdateWeekButtonsVisualState()
        {
            if (FiscalWeekComboBox != null)
            {
                foreach (var item in FiscalWeekComboBox.Items)
                {
                    if (item is ComboBoxItem cbi && cbi.Tag?.ToString() == _currentWeek.ToString())
                    {
                        if (!cbi.IsSelected)
                        {
                            FiscalWeekComboBox.SelectedItem = cbi;
                        }
                        break;
                    }
                }
            }
        }

        private void SelectWeek(int week)
        {
            _currentWeek = Math.Clamp(week, 38, 43);
            UpdateWeekButtonsVisualState();
            if (_isInitialized)
            {
                LoadWeeklyProgressReport();
            }
        }

        public void SelectFiscalWeek(int week)
        {
            SelectWeek(week);
        }

        private void FiscalWeekComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isInitialized) return;
            if (FiscalWeekComboBox?.SelectedItem is ComboBoxItem item && int.TryParse(item.Tag?.ToString(), out int week))
            {
                SelectWeek(week);
            }
        }

        private void ToolsMenuButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.ContextMenu != null)
            {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                btn.ContextMenu.IsOpen = true;
            }
        }

        private void PrevWeekButton_Click(object sender, RoutedEventArgs e)
        {
            SelectWeek(_currentWeek - 1);
        }

        private void NextWeekButton_Click(object sender, RoutedEventArgs e)
        {
            SelectWeek(_currentWeek + 1);
        }

        private void WeekQuickButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string tagStr && int.TryParse(tagStr, out int wk))
            {
                SelectWeek(wk);
            }
        }

        private void TrendWeekBar_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement el && el.DataContext is WeekProgressionPoint pt)
            {
                SelectWeek(pt.FiscalWeek);
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsLoaded || SearchBox == null) return;
            _searchQuery = SearchBox.Text;
            ApplyFilters();
        }

        private void SearchFieldCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SearchFieldCombo?.SelectedItem is ComboBoxItem item && item.Tag is string tag)
            {
                _searchFieldFilter = tag;
                if (IsLoaded) ApplyFilters();
            }
        }

        private void CountryFilterCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CountryFilterCombo?.SelectedItem is ComboBoxItem item && item.Tag is string tag)
            {
                _countryFilter = tag;
                if (IsLoaded) ApplyFilters();
            }
        }

        private void BannerFilterCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BannerFilterCombo?.SelectedItem is ComboBoxItem item && item.Tag is string tag)
            {
                _bannerFilter = tag;
                if (IsLoaded) ApplyFilters();
            }
        }

        private void HomeStoresPreviewGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (HomeStoresPreviewGrid?.SelectedItem is StoreDeployment store)
            {
                if (HomeEmptyStoreBanner != null) HomeEmptyStoreBanner.Visibility = Visibility.Collapsed;
                if (HomeSelectedStoreBanner != null) HomeSelectedStoreBanner.Visibility = Visibility.Visible;

                if (HomeSelectedStoreNumberBadge != null) HomeSelectedStoreNumberBadge.Text = $"STORE #{store.StoreNumber}";
                if (HomeSelectedStoreBannerBadge != null) HomeSelectedStoreBannerBadge.Text = $"{store.BannerName} ({store.BannerCode})";
                if (HomeSelectedStoreNameText != null) HomeSelectedStoreNameText.Text = store.StoreName;
                if (HomeSelectedStoreLocationText != null) HomeSelectedStoreLocationText.Text = $"📍 {store.City}, {store.StateProvince} ({store.CountryCode}) • {store.MallName}";
                if (HomeSelectedStoreTargetDateText != null) HomeSelectedStoreTargetDateText.Text = $"Target Open: {store.TargetOpenDate}";

                if (HomeSelectedStoreStageText != null) HomeSelectedStoreStageText.Text = store.OperationalStatus;
                if (HomeSelectedStoreHealthText != null) HomeSelectedStoreHealthText.Text = store.OverallHealth;
                if (HomeSelectedStoreCircuitsText != null) HomeSelectedStoreCircuitsText.Text = store.CircuitSummary;
                if (HomeSelectedStoreCablingText != null) HomeSelectedStoreCablingText.Text = store.CablingSummary;
                if (HomeSelectedStorePolesText != null) HomeSelectedStorePolesText.Text = store.PowerPoleSummary;

                if (DbStatusText != null)
                {
                    DbStatusText.Text = $"Selected Store #{store.StoreNumber} ({store.StoreName}) — Stage: {store.OperationalStatus} | Health: {store.OverallHealth}";
                }

                if (StoresDataGrid != null && StoresDataGrid.SelectedItem != store)
                {
                    StoresDataGrid.SelectedItem = store;
                }
            }
            else
            {
                if (HomeEmptyStoreBanner != null) HomeEmptyStoreBanner.Visibility = Visibility.Visible;
                if (HomeSelectedStoreBanner != null) HomeSelectedStoreBanner.Visibility = Visibility.Collapsed;
            }
        }

        private void HomeSelectedStoreOpenSpecs_Click(object sender, RoutedEventArgs e)
        {
            if (HomeStoresPreviewGrid?.SelectedItem is StoreDeployment store)
            {
                OpenStoreDetailDialog(store);
            }
            else
            {
                MessageBox.Show("Please select a store to view specifications.", "Select Store", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void HomeSelectedStoreViewInDirectory_Click(object sender, RoutedEventArgs e)
        {
            if (HomeStoresPreviewGrid?.SelectedItem is StoreDeployment store)
            {
                NavStoresButton_Click(this, new RoutedEventArgs());
                StoresDataGrid.SelectedItem = store;
                StoresDataGrid.ScrollIntoView(store);
            }
        }

        private void HomeSelectedStoreMallRules_Click(object sender, RoutedEventArgs e)
        {
            MallRulesButton_Click(this, new RoutedEventArgs());
        }

        private void StoresDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StoresDataGrid?.SelectedItem is StoreDeployment store)
            {
                InspectorStoreTitle.Text = $"Store {store.StoreNumber} — {store.StoreName}";
                InspectorStoreSubtitle.Text = $"{store.BannerName} ({store.BannerCode}) • {store.City}, {store.StateProvince} ({store.CountryCode})";
                InspectorMallText.Text = store.MallName;
                InspectorAddressText.Text = store.FullAddress;
                InspectorDeliveryText.Text = store.DeliveryOptionDisplay;
                InspectorSurveyText.Text = store.SiteSurveyStatus;
                InspectorPolesText.Text = $"{store.PowerPoleCondition} ({store.PowerPolesCount} poles)";
                InspectorCablingText.Text = $"{store.CablingCat6Drops} drops • {store.PatchPanelType}";
                InspectorCircuitsText.Text = $"{store.PrimaryCircuitVendor} ({store.PrimaryCircuitStatus})";
                InspectorSummaryNoteText.Text = string.IsNullOrWhiteSpace(store.TechnicalSummaryNote)
                    ? "No field summary recorded."
                    : store.TechnicalSummaryNote;

                if (DbStatusText != null)
                {
                    DbStatusText.Text = $"Selected Store #{store.StoreNumber} ({store.StoreName}) — Stage: {store.OperationalStatus} | Health: {store.OverallHealth}";
                }

                if (HomeStoresPreviewGrid != null && HomeStoresPreviewGrid.SelectedItem != store)
                {
                    HomeStoresPreviewGrid.SelectedItem = store;
                }
            }
        }

        private void InspectSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            OpenSelectedStore();
        }

        private void RowOpenSpecsButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is StoreDeployment store)
            {
                OpenStoreDetailDialog(store);
            }
        }

        private void Grid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            _cellOriginalValue = string.Empty;
            if (e.Column.GetCellContent(e.Row) is TextBlock tb)
            {
                _cellOriginalValue = tb.Text ?? string.Empty;
            }
        }

        private void StoreGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit && e.Row.Item is StoreDeployment store)
            {
                string colHeader = e.Column.Header?.ToString() ?? "Field";
                string prevVal = _cellOriginalValue;
                string newVal = (e.EditingElement as TextBox)?.Text ?? prevVal;

                _ = Dispatcher.BeginInvoke(new Action(async () =>
                {
                    await _databaseService.SaveStoreAsync(store);
                    if (store.Mall != null)
                        await _databaseService.SaveMallAsync(store.Mall);

                    if (!string.Equals(prevVal, newVal, StringComparison.Ordinal))
                    {
                        AuditHistoryService.RecordChange(
                            "Inline Grid Edit",
                            $"Store #{store.StoreNumber} — {store.StoreName}",
                            colHeader,
                            prevVal,
                            newVal,
                            $"Edited {colHeader} on Store #{store.StoreNumber} ({store.StoreName}): '{prevVal}' → '{newVal}'",
                            store.StoreNumber);
                    }

                    DbStatusText.Text = $"✓ Saved {colHeader} edit for Store #{store.StoreNumber} ({store.StoreName}) at {DateTime.Now:HH:mm:ss}";
                    LoadWeeklyProgressReport();
                    StoresDataGrid_SelectionChanged(StoresDataGrid, null!);
                }), System.Windows.Threading.DispatcherPriority.Background);
            }
        }

        private void ActivityGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit && e.Row.Item is CalendarWorkItem item)
            {
                string colHeader = e.Column.Header?.ToString() ?? "Schedule Field";
                string prevVal = _cellOriginalValue;
                string newVal = (e.EditingElement as TextBox)?.Text ?? prevVal;

                _ = Dispatcher.BeginInvoke(new Action(async () =>
                {
                    await _databaseService.SaveCalendarWorkItemAsync(item);

                    if (!string.Equals(prevVal, newVal, StringComparison.Ordinal))
                    {
                        AuditHistoryService.RecordChange(
                            "Daily Schedule",
                            $"{item.DateDisplay} — Store #{item.StoreNumber}",
                            $"{item.ActivityType} ({colHeader})",
                            prevVal,
                            newVal,
                            $"Updated {colHeader} on {item.ActivityType} for Store #{item.StoreNumber}: '{prevVal}' → '{newVal}'",
                            item.StoreNumber);
                    }

                    DbStatusText.Text = $"✓ Saved schedule update for Store #{item.StoreNumber} ({item.ActivityType}) at {DateTime.Now:HH:mm:ss}";
                    LoadWeeklyProgressReport();
                }), System.Windows.Threading.DispatcherPriority.Background);
            }
        }

        private async void SaveAllChangesButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                HomeStoresPreviewGrid?.CommitEdit(DataGridEditingUnit.Row, true);
                StoresDataGrid?.CommitEdit(DataGridEditingUnit.Row, true);
                HomeWeekActivitiesList?.CommitEdit(DataGridEditingUnit.Row, true);

                foreach (var store in _allStores)
                {
                    await _databaseService.SaveStoreAsync(store);
                    if (store.Mall != null)
                    {
                        await _databaseService.SaveMallAsync(store.Mall);
                    }
                }

                foreach (var item in _allWorkItems)
                {
                    await _databaseService.SaveCalendarWorkItemAsync(item);
                }

                AuditHistoryService.RecordChange(
                    "System Save",
                    $"All Project Records ({_allStores.Count} Stores, {_allWorkItems.Count} Work Orders)",
                    "Manual Save Checkpoint",
                    "Session State",
                    $"Committed at {DateTime.Now:HH:mm:ss}",
                    $"Operator manually saved all {_allStores.Count} store records, landlord rules, and {_allWorkItems.Count} calendar work orders.");

                LoadWeeklyProgressReport();
                ApplyFilters();
                DbStatusText.Text = $"✓ Saved all {_allStores.Count} stores & {_allWorkItems.Count} schedule records at {DateTime.Now:HH:mm:ss}";
                if (LastSaveStatusText != null)
                {
                    LastSaveStatusText.Text = $"✓ Saved {DateTime.Now:HH:mm:ss}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving changes: {ex.Message}", "Save Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void OpenHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            var historyWin = new ActivityHistoryWindow { Owner = this };
            historyWin.ShowDialog();
        }

        private void StoresDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenSelectedStore();
        }

        private void HomeStoresPreviewGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (HomeStoresPreviewGrid.SelectedItem is StoreDeployment selectedStore)
            {
                OpenStoreDetailDialog(selectedStore);
            }
        }

        private void OpenSelectedStore()
        {
            if (StoresDataGrid.SelectedItem is StoreDeployment selectedStore)
            {
                OpenStoreDetailDialog(selectedStore);
            }
            else
            {
                MessageBox.Show("Please select a store from the table to view its specifications.", "Select Store",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void OpenStoreDetailDialog(StoreDeployment selectedStore)
        {
            var detailWindow = new StoreDetailWindow(selectedStore, _currentUser) { Owner = this };
            detailWindow.ShowDialog();
            _allWorkItems = await _databaseService.LoadCalendarWorkItemsAsync(_allStores);
            LoadWeeklyProgressReport();
            ApplyFilters();
        }

        private async void FiscalCalendarButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var calWindow = new FiscalCalendarWindow(_currentWeek) { Owner = this };
                calWindow.ShowDialog();
                _allWorkItems = await _databaseService.LoadCalendarWorkItemsAsync(_allStores);
                LoadWeeklyProgressReport();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open Fiscal Calendar: {ex.Message}", "Calendar Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void MallRulesButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var mallWindow = new MallRulesWindow { Owner = this };
                mallWindow.ShowDialog();
                await LoadStoresDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open Mall Landlord Rules: {ex.Message}", "Mall Rules Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ImportRosterButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_currentUser.CanImportRoster)
            {
                MessageBox.Show("Your current role does not have permission to import store rosters.", "Access Denied",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var importWindow = new RosterImportWindow { Owner = this };
            importWindow.ShowDialog();
            if (importWindow.DidImport)
            {
                await LoadStoresDataAsync();
            }
        }

        private async void AddNewStoreButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var addStoreWin = new AddStoreWindow(_databaseService, isDummyMode: false) { Owner = this };
                bool? result = addStoreWin.ShowDialog();
                if (result == true && addStoreWin.CreatedStore != null)
                {
                    var created = addStoreWin.CreatedStore;
                    await LoadStoresDataAsync();

                    // Automatically select the newly created store in both views
                    var found = _allStores.FirstOrDefault(s => s.StoreNumber == created.StoreNumber) ?? created;
                    if (HomeStoresPreviewGrid != null)
                    {
                        HomeStoresPreviewGrid.SelectedItem = found;
                        HomeStoresPreviewGrid.ScrollIntoView(found);
                    }
                    if (StoresDataGrid != null)
                    {
                        StoresDataGrid.SelectedItem = found;
                        StoresDataGrid.ScrollIntoView(found);
                    }

                    DbStatusText.Text = $"✓ Successfully added new Store #{created.StoreNumber} ({created.StoreName}) to fleet.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open Add Store dialog: {ex.Message}", "Add Store Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ReconnectButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadStoresDataAsync();
        }

        private void SignOutButton_Click(object sender, RoutedEventArgs e)
        {
            AuthService.Logout();
            var loginWindow = new LoginWindow();
            Application.Current.MainWindow = loginWindow;
            loginWindow.Show();
            Close();
        }
    }
}
