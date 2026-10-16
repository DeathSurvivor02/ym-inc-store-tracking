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
        private int _currentWeek = 41;
        private string? _selectedSliceFilter;
        private string _countryFilter = "ALL";
        private string _bannerFilter = "ALL";
        private string _searchQuery = string.Empty;

        public MainWindow(User user)
        {
            InitializeComponent();
            _currentUser = user;
            ApplyUserContext();
            SetupDonutEvents();
            Loaded += MainWindow_Loaded;
        }

        private void ApplyUserContext()
        {
            CurrentUserNameText.Text = _currentUser.FullName;
            CurrentUserRoleBadge.Text = _currentUser.RoleDisplayName;
            CurrentUserRoleBadge.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(_currentUser.RoleBadgeColor));

            // Role-based button access (PM & PA can import rosters)
            ImportRosterButton.IsEnabled = _currentUser.CanImportRoster;
            if (!_currentUser.CanImportRoster)
            {
                ImportRosterButton.ToolTip = "Roster import is managed by the PM or PA.";
                ImportRosterButton.Opacity = 0.5;
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
                    FilterActiveNotice.Text = $"Filtered: {sliceName}";
                }
                ApplyFilters();
            };
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadWeeklyProgressReport();
            await LoadStoresDataAsync();
        }

        private async System.Threading.Tasks.Task LoadStoresDataAsync()
        {
            DbStatusText.Text = "Connecting to PostgreSQL 16...";
            _allStores = await _databaseService.LoadStoresAsync();
            DbStatusText.Text = _databaseService.StatusMessage;
            DbStatusText.Foreground = _databaseService.IsConnected ? 
                new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981")) : 
                new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F59E0B"));

            ApplyFilters();
        }

        private void LoadWeeklyProgressReport()
        {
            var report = SeedDataService.GetWeeklyReport(_currentWeek);
            DonutChart.Report = report;

            CompletedCountText.Text = $"{report.CompletedThisWeek} ({(int)((double)report.CompletedThisWeek / report.TotalActiveProjects * 100)}%)";
            InProgressCountText.Text = $"{report.InProgress} ({(int)((double)report.InProgress / report.TotalActiveProjects * 100)}%)";
            PendingCountText.Text = $"{report.ScheduledPending} ({(int)((double)report.ScheduledPending / report.TotalActiveProjects * 100)}%)";
            BlockedCountText.Text = $"{report.BlockedInJeopardy} ({(int)((double)report.BlockedInJeopardy / report.TotalActiveProjects * 100)}%)";
        }

        private void ApplyFilters()
        {
            var filtered = _allStores.AsEnumerable();

            // 1. Country Filter
            if (_countryFilter != "ALL")
            {
                filtered = filtered.Where(s => s.CountryCode == _countryFilter);
            }

            // 2. Banner Filter
            if (_bannerFilter != "ALL")
            {
                filtered = filtered.Where(s => s.BannerCode == _bannerFilter);
            }

            // 3. Donut Slice Clicked Filter
            if (!string.IsNullOrEmpty(_selectedSliceFilter))
            {
                filtered = filtered.Where(s => s.WeeklyStatus.Equals(_selectedSliceFilter, StringComparison.OrdinalIgnoreCase));
            }

            // 4. Search Filter
            if (!string.IsNullOrWhiteSpace(_searchQuery))
            {
                string q = _searchQuery.ToLowerInvariant().Trim();
                filtered = filtered.Where(s =>
                    s.StoreNumber.ToLowerInvariant().Contains(q) ||
                    s.StoreName.ToLowerInvariant().Contains(q) ||
                    s.BannerName.ToLowerInvariant().Contains(q) ||
                    s.City.ToLowerInvariant().Contains(q) ||
                    (s.Mall != null && s.Mall.PropertyName.ToLowerInvariant().Contains(q)));
            }

            StoresDataGrid.ItemsSource = filtered.ToList();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _searchQuery = SearchBox.Text;
            ApplyFilters();
        }

        private void AllStoresBtn_Click(object sender, RoutedEventArgs e)
        {
            _countryFilter = "ALL";
            UpdateButtonStyles(AllStoresBtn, CanadaStoresBtn, UsStoresBtn);
            ApplyFilters();
        }

        private void CanadaStoresBtn_Click(object sender, RoutedEventArgs e)
        {
            _countryFilter = "CA";
            UpdateButtonStyles(CanadaStoresBtn, AllStoresBtn, UsStoresBtn);
            ApplyFilters();
        }

        private void UsStoresBtn_Click(object sender, RoutedEventArgs e)
        {
            _countryFilter = "US";
            UpdateButtonStyles(UsStoresBtn, AllStoresBtn, CanadaStoresBtn);
            ApplyFilters();
        }

        private static void UpdateButtonStyles(Button active, Button inactive1, Button inactive2)
        {
            active.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2563EB"));
            active.Foreground = Brushes.White;
            inactive1.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E293B"));
            inactive1.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E2E8F0"));
            inactive2.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E293B"));
            inactive2.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E2E8F0"));
        }

        private void BannerFilterCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BannerFilterCombo.SelectedItem is ComboBoxItem item && item.Tag is string tag)
            {
                _bannerFilter = tag;
                ApplyFilters();
            }
        }

        private void Wk41Button_Click(object sender, RoutedEventArgs e)
        {
            _currentWeek = 41;
            Wk41Button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2563EB"));
            Wk41Button.Foreground = Brushes.White;
            Wk42Button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E293B"));
            Wk42Button.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#94A3B8"));
            LoadWeeklyProgressReport();
        }

        private void Wk42Button_Click(object sender, RoutedEventArgs e)
        {
            _currentWeek = 42;
            Wk42Button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2563EB"));
            Wk42Button.Foreground = Brushes.White;
            Wk41Button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E293B"));
            Wk41Button.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#94A3B8"));
            LoadWeeklyProgressReport();
        }

        private void InspectSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            OpenSelectedStore();
        }

        private void StoresDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenSelectedStore();
        }

        private void OpenSelectedStore()
        {
            if (StoresDataGrid.SelectedItem is StoreDeployment selectedStore)
            {
                var detailWindow = new StoreDetailWindow(selectedStore) { Owner = this };
                detailWindow.ShowDialog();
            }
            else
            {
                MessageBox.Show("Please select a store from the table to view its specifications.", "Select Store", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void MallRulesButton_Click(object sender, RoutedEventArgs e)
        {
            var mallWindow = new MallRulesWindow { Owner = this };
            mallWindow.ShowDialog();
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

        private async void ReconnectButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadStoresDataAsync();
        }

        private void SignOutButton_Click(object sender, RoutedEventArgs e)
        {
            AuthService.Logout();
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            Close();
        }
    }
}
