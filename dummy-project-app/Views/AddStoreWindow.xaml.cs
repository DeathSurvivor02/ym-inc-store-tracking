using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using YMInc.StoreDeploymentApp.Models;
using YMInc.StoreDeploymentApp.Services;

namespace YMInc.StoreDeploymentApp.Views
{
    public partial class AddStoreWindow : Window
    {
        private readonly DatabaseService _databaseService;
        public StoreDeployment? CreatedStore { get; private set; }

        public AddStoreWindow(DatabaseService databaseService, bool isDummyMode = false)
        {
            InitializeComponent();
            _databaseService = databaseService ?? new DatabaseService();

            if (isDummyMode)
            {
                AddStoreModeBadge.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFF4CE"));
                AddStoreModeBadge.BorderBrush = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#D97706"));
                AddStoreModeBadgeText.Text = "STANDALONE DUMMY DB";
                AddStoreModeBadgeText.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#795500"));
            }

            // Defaults
            TargetOpenDateBox.Text = DateTime.Now.AddDays(35).ToString("yyyy-MM-dd");

            Loaded += (s, e) =>
            {
                ThemeService.ApplyThemeToWindow(this);
                StoreNumberBox.Focus();
            };
            ThemeService.ThemeChanged += (s, e) => ThemeService.ApplyThemeToWindow(this);
        }

        private async void SaveStoreButton_Click(object sender, RoutedEventArgs e)
        {
            ValidationStatusText.Text = string.Empty;

            string storeNum = StoreNumberBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(storeNum))
            {
                ValidationStatusText.Text = "Store Number is required.";
                StoreNumberBox.Focus();
                return;
            }

            string storeName = StoreNameBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(storeName))
            {
                ValidationStatusText.Text = "Store Display Name is required.";
                StoreNameBox.Focus();
                return;
            }

            string address = AddressBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(address))
            {
                ValidationStatusText.Text = "Street Address is required.";
                AddressBox.Focus();
                return;
            }

            string city = CityBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(city))
            {
                ValidationStatusText.Text = "City is required.";
                CityBox.Focus();
                return;
            }

            string state = StateProvinceBox.Text.Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(state))
            {
                ValidationStatusText.Text = "Province / State is required.";
                StateProvinceBox.Focus();
                return;
            }

            string postal = PostalCodeBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(postal))
            {
                ValidationStatusText.Text = "Postal / ZIP Code is required.";
                PostalCodeBox.Focus();
                return;
            }

            string targetDate = TargetOpenDateBox.Text.Trim();
            if (!DateTime.TryParse(targetDate, out _))
            {
                ValidationStatusText.Text = "Target Open Date must be a valid date in YYYY-MM-DD format.";
                TargetOpenDateBox.Focus();
                return;
            }

            // Banner details
            var bannerItem = BannerCombo.SelectedItem as ComboBoxItem;
            string bannerCode = bannerItem?.Tag?.ToString() ?? "CR";
            string bannerName = bannerItem?.Content?.ToString()?.Split('(')[0].Trim() ?? "Charlotte Russe";

            // Project Type
            string projectType = (ProjectTypeCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "New Store Opening";

            // Country
            var countryItem = CountryCombo.SelectedItem as ComboBoxItem;
            string countryCode = countryItem?.Tag?.ToString() ?? "CA";

            // Status
            string operationalStatus = (OperationalStatusCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Planning";
            string overallHealth = (OverallHealthCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "On Track";

            // Infrastructure
            int.TryParse(PosCountBox.Text.Trim(), out int posCount);
            if (posCount <= 0) posCount = 4;

            int.TryParse(PowerPolesBox.Text.Trim(), out int poleCount);
            if (poleCount <= 0) poleCount = 2;

            int.TryParse(CablingDropsBox.Text.Trim(), out int cablingDrops);
            if (cablingDrops <= 0) cablingDrops = 16;

            string primaryCarrier = PrimaryCarrierBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(primaryCarrier)) primaryCarrier = countryCode == "US" ? "AT&T Business" : "Bell Canada";

            string secondaryCarrier = SecondaryCarrierBox.Text.Trim();

            string mallName = MallNameBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(mallName)) mallName = $"{storeName} Center";

            SaveStoreButton.IsEnabled = false;
            ValidationStatusText.Foreground = new System.Windows.Media.SolidColorBrush(
                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#0F6CBD"));
            ValidationStatusText.Text = "Registering store in deployment database...";

            try
            {
                var newStore = new StoreDeployment
                {
                    StoreNumber = storeNum,
                    BannerCode = bannerCode,
                    BannerName = bannerName,
                    StoreName = storeName,
                    ProjectType = projectType,
                    OperationalStatus = operationalStatus,
                    OverallHealth = overallHealth,
                    CountryCode = countryCode,
                    Address = address,
                    City = city,
                    StateProvince = state,
                    PostalCode = postal,
                    PosTerminalCount = posCount,
                    TargetOpenDate = targetDate,
                    OpeningDate = targetDate,
                    GrossSqft = 7500,
                    SalesSqft = 6200,
                    Mall = new MallProperty
                    {
                        MallCode = $"MALL-{storeNum}",
                        PropertyName = mallName,
                        LandlordCompany = "Regional Retail Property Management",
                        Address = address,
                        City = city,
                        StateProvince = state,
                        PostalCode = postal,
                        CountryCode = countryCode,
                        DeliveryWindow = "06:00 - 09:30",
                        CablingVendor = "Black Box Network Services",
                        MallContacted = true
                    },
                    Survey = new SiteSurvey
                    {
                        SurveyVendor = "National Site Surveyors",
                        SurveyorName = "Assigned Field Engineer",
                        ScheduledDate = DateTime.Now.AddDays(5).ToString("yyyy-MM-dd"),
                        Status = operationalStatus == "Planning" ? "Scheduled" : "Completed & Approved",
                        CeilingType = "Drop Ceiling (2x4 Acoustic)",
                        CeilingHeightFeet = 14.0,
                        DemarcLocation = "Back Office / Demarc",
                        DemarcConduitAvailable = true,
                        MdfRackLocation = "Back Office Rack B",
                        StructureNotes = "New storefront deployment intake."
                    },
                    PowerPoles = new PowerPole
                    {
                        Status = operationalStatus == "Planning" ? "Survey Required" : "Delivered On Site",
                        TotalRequired = poleCount,
                        InstalledCount = operationalStatus == "Grand Open" ? poleCount : 0,
                        FinishColor = "Anodized Aluminum",
                        CeilingMountType = "T-Bar Grid Clamp with Structural Tie-Wire",
                        CashWrapFeedDrops = 4
                    },
                    Cabling = new CablingOperation
                    {
                        Vendor = "Black Box Network Services",
                        LeadTechName = "Assigned Lead",
                        Status = operationalStatus == "Grand Open" ? "Fluke Certified & Closed" : "Vendor Assigned",
                        TotalDataDrops = cablingDrops,
                        PosDrops = posCount,
                        ApDrops = 4,
                        FlukeCertified = operationalStatus == "Grand Open",
                        ScheduledStartDate = DateTime.Now.AddDays(10).ToString("yyyy-MM-dd"),
                        ScheduledCompletionDate = DateTime.Now.AddDays(16).ToString("yyyy-MM-dd")
                    },
                    Circuits = new List<TelecomCircuit>
                    {
                        new()
                        {
                            Role = "Primary WAN",
                            CarrierName = primaryCarrier,
                            ServiceType = "Commercial Fiber DIA 100M",
                            OrderStatus = operationalStatus == "Grand Open" ? "Tested & Turned Up (Active)" : "Order Placed",
                            BandwidthDownMbps = 100,
                            BandwidthUpMbps = 100,
                            DemarcExtended = true
                        }
                    },
                    WeeklyCompletionPct = operationalStatus == "Grand Open" ? 100 : (operationalStatus == "Planning" ? 15 : 50),
                    WeeklyStatus = operationalStatus == "Grand Open" ? "Completed & Certified" : "In Progress / On Schedule",
                    WeeklySummaryNote = $"Store #{storeNum} registered for {projectType} under {bannerName} banner."
                };

                if (!string.IsNullOrWhiteSpace(secondaryCarrier))
                {
                    newStore.Circuits.Add(new TelecomCircuit
                    {
                        Role = "Secondary Failover WAN",
                        CarrierName = secondaryCarrier,
                        ServiceType = "Enterprise 5G Wireless",
                        OrderStatus = operationalStatus == "Grand Open" ? "Tested & Turned Up (Active)" : "Order Placed",
                        BandwidthDownMbps = 100,
                        BandwidthUpMbps = 25,
                        DemarcExtended = true
                    });
                }

                bool success = await _databaseService.CreateStoreAsync(newStore);
                if (!success)
                {
                    ValidationStatusText.Foreground = new System.Windows.Media.SolidColorBrush(
                        (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#DC2626"));
                    ValidationStatusText.Text = _databaseService.LastError ?? "Failed to save new store to database.";
                    SaveStoreButton.IsEnabled = true;
                    return;
                }

                AuditHistoryService.RecordChange(
                    "Store Created",
                    $"Store #{newStore.StoreNumber} — {newStore.StoreName}",
                    "Fleet Roster",
                    "(None)",
                    $"Store #{newStore.StoreNumber} ({newStore.BannerName}) added to project.",
                    $"Operator manually added new store #{newStore.StoreNumber} in {newStore.City}, {newStore.StateProvince} ({newStore.CountryCode}) to the project fleet.",
                    newStore.StoreNumber);

                CreatedStore = newStore;
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                ValidationStatusText.Foreground = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#DC2626"));
                ValidationStatusText.Text = $"Error: {ex.Message}";
                SaveStoreButton.IsEnabled = true;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
