using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using YMInc.StoreDeploymentApp.Models;
using YMInc.StoreDeploymentApp.Services;

namespace YMInc.StoreDeploymentApp.Views
{
    public partial class StoreDetailWindow : Window
    {
        private readonly StoreDeployment _store;
        private readonly User? _currentUser;
        private readonly DatabaseService _databaseService = new();
        private System.Windows.Threading.DispatcherTimer? _heartbeatTimer;
        private readonly System.Windows.Threading.DispatcherTimer _autosaveTimer;
        private string? _pendingAutosaveSection;
        private bool _isPopulating;
        private bool _autosaveEventsWired;

        public StoreDetailWindow(StoreDeployment store, User? currentUser = null, int initialTabIndex = 0)
        {
            InitializeComponent();
            _store = store;
            _currentUser = currentUser;

            _autosaveTimer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(450)
            };
            _autosaveTimer.Tick += OnAutosaveTick;

            PopulateStore(store);

            if (initialTabIndex >= 0 && initialTabIndex < DetailTabControl.Items.Count)
            {
                DetailTabControl.SelectedIndex = initialTabIndex;
            }

            Loaded += async (s, e) =>
            {
                if (initialTabIndex >= 0 && initialTabIndex < DetailTabControl.Items.Count)
                {
                    DetailTabControl.SelectedIndex = initialTabIndex;
                }
                ThemeService.ApplyThemeToWindow(this);
                WireAutosaveEvents();
                DetailTabControl.SelectionChanged += OnTabSelectionChanged;
                RealtimeSyncService.Instance.StoreChanged += OnRemoteStoreChange;
                RealtimeSyncService.Instance.PresenceChanged += OnRemotePresenceChange;
                SaveNotificationText.Text = "🟢 Autosave & Live Co-editing Active";

                await PresenceService.EnterStoreAsync(_store.StoreNumber, _currentUser, GetCurrentTabName());
                await RefreshCollaboratorsAsync();
                StartHeartbeat();
            };

            ThemeService.ThemeChanged += (s, e) => ThemeService.ApplyThemeToWindow(this);

            Closed += (s, e) =>
            {
                _autosaveTimer.Stop();
                _heartbeatTimer?.Stop();
                DetailTabControl.SelectionChanged -= OnTabSelectionChanged;
                RealtimeSyncService.Instance.StoreChanged -= OnRemoteStoreChange;
                RealtimeSyncService.Instance.PresenceChanged -= OnRemotePresenceChange;
                PresenceService.ReleaseEditor(_store.StoreNumber);
            };
        }

        private string GetCurrentTabName()
        {
            return (DetailTabControl.SelectedItem as TabItem)?.Header?.ToString() ?? "General";
        }

        private async void OnTabSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source == DetailTabControl)
            {
                string tabName = GetCurrentTabName();
                await PresenceService.UpdateSectionAsync(_store.StoreNumber, tabName);
            }
        }

        private void OnRemotePresenceChange(PresenceChangeEvent evt)
        {
            if (evt.StoreNumber == _store.StoreNumber)
            {
                Dispatcher.InvokeAsync(async () =>
                {
                    await RefreshCollaboratorsAsync();
                });
            }
        }

        private async Task RefreshCollaboratorsAsync()
        {
            var list = await PresenceService.GetStoreCollaboratorsAsync(_store.StoreNumber);
            CollaboratorAvatarsList.ItemsSource = list;
            if (list.Count > 0)
            {
                string names = string.Join(", ", list.Select(c => $"{c.DisplayName} ({c.EditorRole})"));
                LiveActivityNoticeText.Text = $"👥 {names} co-editing right now";
                LiveActivityNoticeText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0F6CBD"));
                LiveActivityNoticeText.FontWeight = FontWeights.SemiBold;
            }
            else
            {
                LiveActivityNoticeText.Text = "Single user in document • Changes broadcast live";
                LiveActivityNoticeText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4B5563"));
                LiveActivityNoticeText.FontWeight = FontWeights.Normal;
            }
        }

        private void StartHeartbeat()
        {
            _heartbeatTimer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            _heartbeatTimer.Tick += async (s, e) =>
            {
                await PresenceService.HeartbeatAsync();
                await RefreshCollaboratorsAsync();
            };
            _heartbeatTimer.Start();
        }

        private void PopulateStore(StoreDeployment store)
        {
            _isPopulating = true;
            try
            {
                BannerCodeBadge.Text = store.BannerCode;
                StoreNumberText.Text = $"Store #{store.StoreNumber}";
                CountryFlagText.Text = store.CountryFlag;
                StoreNameTextBox.Text = store.StoreName;
                StoreMallText.Text = store.MallName;
                TargetDateTextBox.Text = store.TargetOpenDate ?? store.OpeningDate ?? "2026-11-27";

                // Operational Status
                SelectComboItem(OperationalStatusCombo, store.OperationalStatus);

                // Health
                SelectComboItem(HealthCombo, store.OverallHealth);

                // Site Survey
                if (store.Survey != null)
                {
                    SurveyCeilingTypeTextBox.Text = store.Survey.CeilingType;
                    SurveyCeilingHeightTextBox.Text = store.Survey.CeilingHeightFeet > 0 ? store.Survey.CeilingHeightFeet.ToString("F1") : "Unverified / Pending";
                    SelectComboItem(SurveyStatusCombo, store.Survey.Status);
                    SurveyDemarcLocationTextBox.Text = store.Survey.DemarcLocation;
                    SelectComboItemByTag(SurveyConduitCombo, store.Survey.DemarcConduitAvailable.ToString());
                    SurveyorTextBox.Text = string.IsNullOrEmpty(store.Survey.SurveyorName) ? 
                        store.Survey.SurveyVendor : $"{store.Survey.SurveyorName} ({store.Survey.SurveyVendor})";
                    SurveyRackLocationTextBox.Text = store.Survey.MdfRackLocation;
                    SurveyDateTextBox.Text = !string.IsNullOrEmpty(store.Survey.ActualDate) ? store.Survey.ActualDate : store.Survey.ScheduledDate;
                    SurveyNotesTextBox.Text = store.Survey.StructureNotes;
                }

                // Power Poles
                if (store.PowerPoles != null)
                {
                    SelectComboItem(PowerPoleStatusCombo, store.PowerPoles.Status);
                    PowerPoleInstalledTextBox.Text = store.PowerPoles.InstalledCount.ToString();
                    PowerPoleTotalTextBox.Text = store.PowerPoles.TotalRequired.ToString();
                    PowerPoleColorTextBox.Text = store.PowerPoles.FinishColor;
                    PowerPoleMountTextBox.Text = store.PowerPoles.CeilingMountType;
                    PowerPoleDropsTextBox.Text = store.PowerPoles.CashWrapFeedDrops.ToString();
                    PowerPoleContractorTextBox.Text = store.PowerPoles.ElectricalContractor ?? "";
                    PowerPoleBlockerTextBox.Text = store.PowerPoles.BlockerReason ?? "";
                }

                // Cabling
                if (store.Cabling != null)
                {
                    SelectComboItem(CablingStatusCombo, store.Cabling.Status);
                    CablingTotalDropsTextBox.Text = store.Cabling.TotalDataDrops.ToString();
                    CablingPosDropsTextBox.Text = store.Cabling.PosDrops.ToString();
                    CablingApDropsTextBox.Text = store.Cabling.ApDrops.ToString();
                    SelectComboItemByTag(CablingFlukeCombo, store.Cabling.FlukeCertified.ToString());
                    CablingVendorTextBox.Text = store.Cabling.Vendor;
                    CablingTechTextBox.Text = store.Cabling.LeadTechName ?? "";
                }

                // Telecom WAN Circuits
                CircuitsItemsControl.ItemsSource = store.Circuits;

                // Mall Landlord Rules & Contacts
                if (store.Mall != null)
                {
                    MallNameHeading.Text = $"{store.Mall.PropertyName} Compliance & Delivery Matrix";
                    MallReqStatusBadgeText.Text = store.Mall.RequirementsStatusDisplay;
                    if (store.Mall.HasRequirements)
                    {
                        MallReqStatusBadgeBorder.Background = new SolidColorBrush(Color.FromRgb(6, 78, 59));
                        MallReqStatusBadgeText.Foreground = new SolidColorBrush(Color.FromRgb(52, 211, 153));
                    }
                    else if (store.Mall.MallContacted)
                    {
                        MallReqStatusBadgeBorder.Background = new SolidColorBrush(Color.FromRgb(69, 26, 3));
                        MallReqStatusBadgeText.Foreground = new SolidColorBrush(Color.FromRgb(251, 191, 36));
                    }
                    else
                    {
                        MallReqStatusBadgeBorder.Background = new SolidColorBrush(Color.FromRgb(69, 10, 10));
                        MallReqStatusBadgeText.Foreground = new SolidColorBrush(Color.FromRgb(248, 113, 113));
                    }

                    SelectComboItemByTag(MallContactedCombo, store.Mall.MallContacted.ToString());
                    MallTeamMemberTextBox.Text = store.Mall.AssignedTeamMember ?? "";
                    MallLandlordCompanyTextBox.Text = store.Mall.LandlordCompany;
                    SelectComboItemByTag(MallCoiCombo, store.Mall.CoiRequired.ToString());
                    SelectComboItemByTag(MallAfterHoursCombo, store.Mall.NightWorkOnly.ToString());
                    MallDeliveryTextBox.Text = store.Mall.DeliveryWindow;
                    MallLiabilityTextBox.Text = store.Mall.MinGeneralLiabilityCad.ToString("F0");
                    MallCablingCompanyTextBox.Text = store.CablingVendorDisplay;
                    MallContactNameTextBox.Text = store.Mall.ManagementContactName;
                    MallContactPhoneTextBox.Text = store.Mall.ManagementPhone;
                    MallContactEmailTextBox.Text = store.Mall.ManagementEmail;
                    MallSecurityPhoneTextBox.Text = store.Mall.SecurityPhone;
                    MallSpecialRulesTextBox.Text = store.Mall.SpecialRules;
                    MallIntermediateStepsTextBox.Text = store.Mall.IntermediateSteps;
                }
                else
                {
                    MallNameHeading.Text = "Street Front / Non-Mall Location";
                    MallReqStatusBadgeText.Text = "Direct Lease";
                    SelectComboItemByTag(MallContactedCombo, "True");
                    MallTeamMemberTextBox.Text = "";
                    MallLandlordCompanyTextBox.Text = "Direct Commercial Lease";
                    SelectComboItemByTag(MallCoiCombo, "True");
                    SelectComboItemByTag(MallAfterHoursCombo, "False");
                    MallDeliveryTextBox.Text = "Standard Business Hours";
                    MallLiabilityTextBox.Text = "5000000";
                    MallCablingCompanyTextBox.Text = store.CablingVendorDisplay;
                    MallContactNameTextBox.Text = "Direct Lease Admin";
                    MallContactPhoneTextBox.Text = store.PhoneNumber ?? "N/A";
                    MallContactEmailTextBox.Text = "operations@yminc.ca";
                    MallSecurityPhoneTextBox.Text = "N/A";
                    MallSpecialRulesTextBox.Text = "Direct street-front retail delivery.";
                    MallIntermediateStepsTextBox.Text = "";
                }

                RefreshStoreHistory();
            }
            finally
            {
                _isPopulating = false;
            }
        }

        private void WireAutosaveEvents()
        {
            if (_autosaveEventsWired) return;
            _autosaveEventsWired = true;

            // General
            StoreNameTextBox.TextChanged += (s, e) => QueueAutosave("general");
            TargetDateTextBox.TextChanged += (s, e) => QueueAutosave("general");
            OperationalStatusCombo.SelectionChanged += (s, e) => QueueAutosave("general");
            HealthCombo.SelectionChanged += (s, e) => QueueAutosave("general");

            // Survey
            SurveyCeilingTypeTextBox.TextChanged += (s, e) => QueueAutosave("survey");
            SurveyCeilingHeightTextBox.TextChanged += (s, e) => QueueAutosave("survey");
            SurveyStatusCombo.SelectionChanged += (s, e) => QueueAutosave("survey");
            SurveyDemarcLocationTextBox.TextChanged += (s, e) => QueueAutosave("survey");
            SurveyConduitCombo.SelectionChanged += (s, e) => QueueAutosave("survey");
            SurveyorTextBox.TextChanged += (s, e) => QueueAutosave("survey");
            SurveyRackLocationTextBox.TextChanged += (s, e) => QueueAutosave("survey");
            SurveyNotesTextBox.TextChanged += (s, e) => QueueAutosave("survey");

            // Power Poles
            PowerPoleStatusCombo.SelectionChanged += (s, e) => QueueAutosave("poles");
            PowerPoleInstalledTextBox.TextChanged += (s, e) => QueueAutosave("poles");
            PowerPoleTotalTextBox.TextChanged += (s, e) => QueueAutosave("poles");
            PowerPoleColorTextBox.TextChanged += (s, e) => QueueAutosave("poles");
            PowerPoleMountTextBox.TextChanged += (s, e) => QueueAutosave("poles");
            PowerPoleDropsTextBox.TextChanged += (s, e) => QueueAutosave("poles");
            PowerPoleContractorTextBox.TextChanged += (s, e) => QueueAutosave("poles");
            PowerPoleBlockerTextBox.TextChanged += (s, e) => QueueAutosave("poles");

            // Cabling
            CablingStatusCombo.SelectionChanged += (s, e) => QueueAutosave("cabling");
            CablingTotalDropsTextBox.TextChanged += (s, e) => QueueAutosave("cabling");
            CablingPosDropsTextBox.TextChanged += (s, e) => QueueAutosave("cabling");
            CablingApDropsTextBox.TextChanged += (s, e) => QueueAutosave("cabling");
            CablingFlukeCombo.SelectionChanged += (s, e) => QueueAutosave("cabling");
            CablingVendorTextBox.TextChanged += (s, e) => QueueAutosave("cabling");
            CablingTechTextBox.TextChanged += (s, e) => QueueAutosave("cabling");

            // Mall
            MallContactedCombo.SelectionChanged += (s, e) => QueueAutosave("mall");
            MallTeamMemberTextBox.TextChanged += (s, e) => QueueAutosave("mall");
            MallLandlordCompanyTextBox.TextChanged += (s, e) => QueueAutosave("mall");
            MallCoiCombo.SelectionChanged += (s, e) => QueueAutosave("mall");
            MallAfterHoursCombo.SelectionChanged += (s, e) => QueueAutosave("mall");
            MallDeliveryTextBox.TextChanged += (s, e) => QueueAutosave("mall");
            MallLiabilityTextBox.TextChanged += (s, e) => QueueAutosave("mall");
            MallCablingCompanyTextBox.TextChanged += (s, e) => QueueAutosave("mall");
            MallContactNameTextBox.TextChanged += (s, e) => QueueAutosave("mall");
            MallContactPhoneTextBox.TextChanged += (s, e) => QueueAutosave("mall");
            MallContactEmailTextBox.TextChanged += (s, e) => QueueAutosave("mall");
            MallSecurityPhoneTextBox.TextChanged += (s, e) => QueueAutosave("mall");
            MallSpecialRulesTextBox.TextChanged += (s, e) => QueueAutosave("mall");
            MallIntermediateStepsTextBox.TextChanged += (s, e) => QueueAutosave("mall");
        }

        private void QueueAutosave(string section)
        {
            if (_isPopulating) return;
            _pendingAutosaveSection = section;
            _autosaveTimer.Stop();
            _autosaveTimer.Start();
            SaveNotificationText.Text = "⏳ Autosaving changes...";
        }

        private async void OnAutosaveTick(object? sender, EventArgs e)
        {
            _autosaveTimer.Stop();
            string? section = _pendingAutosaveSection;
            _pendingAutosaveSection = null;
            if (string.IsNullOrEmpty(section)) return;

            bool ok = false;
            switch (section)
            {
                case "general":
                    ok = await SaveGeneralSectionAsync();
                    break;
                case "survey":
                    ok = await SaveSurveySectionAsync();
                    break;
                case "poles":
                    ok = await SavePolesSectionAsync();
                    break;
                case "cabling":
                    ok = await SaveCablingSectionAsync();
                    break;
                case "mall":
                    ok = await SaveMallSectionAsync();
                    break;
                default:
                    ok = await SaveAllSectionsAsync();
                    break;
            }

            if (ok)
            {
                SaveNotificationText.Text = $"✓ Autosaved ({DateTime.Now:HH:mm:ss})";
                RefreshStoreHistory();
            }
            else
            {
                SaveNotificationText.Text = $"✓ Updated in local session ({DateTime.Now:HH:mm:ss})";
            }
        }

        private async Task<bool> SaveGeneralSectionAsync()
        {
            _store.StoreName = StoreNameTextBox.Text.Trim();
            _store.TargetOpenDate = TargetDateTextBox.Text.Trim();
            if (OperationalStatusCombo.SelectedItem is ComboBoxItem statusItem)
                _store.OperationalStatus = statusItem.Content?.ToString() ?? _store.OperationalStatus;
            if (HealthCombo.SelectedItem is ComboBoxItem healthItem)
                _store.OverallHealth = healthItem.Content?.ToString() ?? _store.OverallHealth;

            return await _databaseService.SaveStoreGeneralSectionAsync(
                _store.StoreId, _store.StoreName, _store.OperationalStatus, _store.OverallHealth, _store.TargetOpenDate);
        }

        private async Task<bool> SaveSurveySectionAsync()
        {
            if (_store.Survey == null) _store.Survey = new SiteSurvey { StoreId = _store.StoreId };
            _store.Survey.CeilingType = SurveyCeilingTypeTextBox.Text.Trim();
            if (double.TryParse(SurveyCeilingHeightTextBox.Text.Replace("ft", "").Trim(), out double h))
                _store.Survey.CeilingHeightFeet = h;
            if (SurveyStatusCombo.SelectedItem is ComboBoxItem surveyStatusItem)
                _store.Survey.Status = surveyStatusItem.Content?.ToString() ?? _store.Survey.Status;
            _store.Survey.DemarcLocation = SurveyDemarcLocationTextBox.Text.Trim();
            if (SurveyConduitCombo.SelectedItem is ComboBoxItem conduitItem && conduitItem.Tag is string conduitTag)
                _store.Survey.DemarcConduitAvailable = conduitTag == "True";
            _store.Survey.SurveyorName = SurveyorTextBox.Text.Trim();
            _store.Survey.MdfRackLocation = SurveyRackLocationTextBox.Text.Trim();
            _store.Survey.StructureNotes = SurveyNotesTextBox.Text.Trim();

            return await _databaseService.SaveSiteSurveySectionAsync(_store.StoreId, _store.Survey);
        }

        private async Task<bool> SavePolesSectionAsync()
        {
            if (_store.PowerPoles == null) _store.PowerPoles = new PowerPole { StoreId = _store.StoreId };
            if (PowerPoleStatusCombo.SelectedItem is ComboBoxItem ppStatusItem)
                _store.PowerPoles.Status = ppStatusItem.Content?.ToString() ?? _store.PowerPoles.Status;
            if (int.TryParse(PowerPoleInstalledTextBox.Text.Trim(), out int installed))
                _store.PowerPoles.InstalledCount = installed;
            if (int.TryParse(PowerPoleTotalTextBox.Text.Trim(), out int total))
                _store.PowerPoles.TotalRequired = total;
            _store.PowerPoles.FinishColor = PowerPoleColorTextBox.Text.Trim();
            _store.PowerPoles.CeilingMountType = PowerPoleMountTextBox.Text.Trim();
            if (int.TryParse(PowerPoleDropsTextBox.Text.Trim(), out int ppDrops))
                _store.PowerPoles.CashWrapFeedDrops = ppDrops;
            _store.PowerPoles.ElectricalContractor = PowerPoleContractorTextBox.Text.Trim();
            _store.PowerPoles.BlockerReason = string.IsNullOrWhiteSpace(PowerPoleBlockerTextBox.Text) ? null : PowerPoleBlockerTextBox.Text.Trim();

            return await _databaseService.SavePowerPolesSectionAsync(_store.StoreId, _store.PowerPoles);
        }

        private async Task<bool> SaveCablingSectionAsync()
        {
            if (_store.Cabling == null) _store.Cabling = new CablingOperation { StoreId = _store.StoreId };
            if (CablingStatusCombo.SelectedItem is ComboBoxItem cablingStatusItem)
                _store.Cabling.Status = cablingStatusItem.Content?.ToString() ?? _store.Cabling.Status;
            if (int.TryParse(CablingTotalDropsTextBox.Text.Trim(), out int totalDrops))
                _store.Cabling.TotalDataDrops = totalDrops;
            if (int.TryParse(CablingPosDropsTextBox.Text.Trim(), out int posDrops))
                _store.Cabling.PosDrops = posDrops;
            if (int.TryParse(CablingApDropsTextBox.Text.Trim(), out int apDrops))
                _store.Cabling.ApDrops = apDrops;
            if (CablingFlukeCombo.SelectedItem is ComboBoxItem flukeItem && flukeItem.Tag is string flukeTag)
                _store.Cabling.FlukeCertified = flukeTag == "True";
            _store.Cabling.Vendor = CablingVendorTextBox.Text.Trim();
            _store.Cabling.LeadTechName = CablingTechTextBox.Text.Trim();

            return await _databaseService.SaveCablingSectionAsync(_store.StoreId, _store.Cabling);
        }

        private async Task<bool> SaveMallSectionAsync()
        {
            if (_store.Mall == null)
            {
                _store.Mall = new MallProperty
                {
                    MallCode = $"M-{_store.StoreNumber}",
                    PropertyName = string.IsNullOrWhiteSpace(_store.StoreName) ? $"Store #{_store.StoreNumber} Property" : _store.StoreName,
                    City = _store.City,
                    StateProvince = _store.StateProvince,
                    CountryCode = _store.CountryCode
                };
            }

            if (MallContactedCombo.SelectedItem is ComboBoxItem mcItem && mcItem.Tag is string mcTag)
                _store.Mall.MallContacted = mcTag.Equals("True", StringComparison.OrdinalIgnoreCase);

            _store.Mall.AssignedTeamMember = MallTeamMemberTextBox.Text.Trim();

            if (MallCoiCombo.SelectedItem is ComboBoxItem coiItem && coiItem.Tag is string coiTag)
                _store.Mall.CoiRequired = coiTag.Equals("True", StringComparison.OrdinalIgnoreCase);

            if (MallAfterHoursCombo.SelectedItem is ComboBoxItem ahItem && ahItem.Tag is string ahTag)
                _store.Mall.NightWorkOnly = ahTag.Equals("True", StringComparison.OrdinalIgnoreCase);

            _store.Mall.DeliveryWindow = MallDeliveryTextBox.Text.Trim();
            _store.Mall.LandlordCompany = MallLandlordCompanyTextBox.Text.Trim();

            if (decimal.TryParse(MallLiabilityTextBox.Text.Replace("$", "").Replace(",", "").Trim(), out decimal liab))
                _store.Mall.MinGeneralLiabilityCad = liab;

            if (!string.IsNullOrWhiteSpace(MallCablingCompanyTextBox.Text))
            {
                _store.Mall.CablingVendor = MallCablingCompanyTextBox.Text.Trim();
                if (string.IsNullOrWhiteSpace(_store.Cabling.Vendor))
                    _store.Cabling.Vendor = _store.Mall.CablingVendor;
            }

            _store.Mall.ManagementContactName = MallContactNameTextBox.Text.Trim();
            _store.Mall.ManagementPhone = MallContactPhoneTextBox.Text.Trim();
            _store.Mall.ManagementEmail = MallContactEmailTextBox.Text.Trim();
            _store.Mall.SecurityPhone = MallSecurityPhoneTextBox.Text.Trim();
            _store.Mall.SpecialRules = MallSpecialRulesTextBox.Text.Trim();
            _store.Mall.IntermediateSteps = MallIntermediateStepsTextBox.Text.Trim();

            MallReqStatusBadgeText.Text = _store.Mall.RequirementsStatusDisplay;
            return await _databaseService.SaveMallAsync(_store.Mall);
        }

        private async Task<bool> SaveAllSectionsAsync()
        {
            bool g = await SaveGeneralSectionAsync();
            bool s = await SaveSurveySectionAsync();
            bool p = await SavePolesSectionAsync();
            bool c = await SaveCablingSectionAsync();
            bool m = await SaveMallSectionAsync();
            return g && s && p && c && m;
        }

        // Live Ingestion from other teammates without clobbering active user inputs
        private void OnRemoteStoreChange(StoreChangeEvent evt)
        {
            if (_store == null || evt.StoreId != _store.StoreId) return;

            Dispatcher.InvokeAsync(async () =>
            {
                try
                {
                    var refreshed = await _databaseService.LoadStoreByIdAsync(_store.StoreId);
                    if (refreshed == null) return;

                    if (evt.Table == "cabling_operations" && refreshed.Cabling != null)
                    {
                        if (!IsCablingFocused())
                        {
                            _isPopulating = true;
                            _store.Cabling = refreshed.Cabling;
                            SelectComboItem(CablingStatusCombo, refreshed.Cabling.Status);
                            CablingTotalDropsTextBox.Text = refreshed.Cabling.TotalDataDrops.ToString();
                            CablingPosDropsTextBox.Text = refreshed.Cabling.PosDrops.ToString();
                            CablingApDropsTextBox.Text = refreshed.Cabling.ApDrops.ToString();
                            SelectComboItemByTag(CablingFlukeCombo, refreshed.Cabling.FlukeCertified.ToString());
                            CablingVendorTextBox.Text = refreshed.Cabling.Vendor;
                            CablingTechTextBox.Text = refreshed.Cabling.LeadTechName ?? "";
                            _isPopulating = false;
                            SaveNotificationText.Text = $"⚡ Cabling updated live from teammate ({DateTime.Now:HH:mm:ss})";
                        }
                    }
                    else if (evt.Table == "power_poles" && refreshed.PowerPoles != null)
                    {
                        if (!IsPowerPolesFocused())
                        {
                            _isPopulating = true;
                            _store.PowerPoles = refreshed.PowerPoles;
                            SelectComboItem(PowerPoleStatusCombo, refreshed.PowerPoles.Status);
                            PowerPoleInstalledTextBox.Text = refreshed.PowerPoles.InstalledCount.ToString();
                            PowerPoleTotalTextBox.Text = refreshed.PowerPoles.TotalRequired.ToString();
                            PowerPoleColorTextBox.Text = refreshed.PowerPoles.FinishColor;
                            PowerPoleMountTextBox.Text = refreshed.PowerPoles.CeilingMountType;
                            PowerPoleDropsTextBox.Text = refreshed.PowerPoles.CashWrapFeedDrops.ToString();
                            PowerPoleContractorTextBox.Text = refreshed.PowerPoles.ElectricalContractor ?? "";
                            PowerPoleBlockerTextBox.Text = refreshed.PowerPoles.BlockerReason ?? "";
                            _isPopulating = false;
                            SaveNotificationText.Text = $"⚡ Power Poles updated live from teammate ({DateTime.Now:HH:mm:ss})";
                        }
                    }
                    else if (evt.Table == "site_surveys" && refreshed.Survey != null)
                    {
                        if (!IsSurveyFocused())
                        {
                            _isPopulating = true;
                            _store.Survey = refreshed.Survey;
                            SurveyCeilingTypeTextBox.Text = refreshed.Survey.CeilingType;
                            SurveyCeilingHeightTextBox.Text = refreshed.Survey.CeilingHeightFeet > 0 ? refreshed.Survey.CeilingHeightFeet.ToString("F1") : "Unverified / Pending";
                            SelectComboItem(SurveyStatusCombo, refreshed.Survey.Status);
                            SurveyDemarcLocationTextBox.Text = refreshed.Survey.DemarcLocation;
                            SelectComboItemByTag(SurveyConduitCombo, refreshed.Survey.DemarcConduitAvailable.ToString());
                            SurveyorTextBox.Text = string.IsNullOrEmpty(refreshed.Survey.SurveyorName) ? refreshed.Survey.SurveyVendor : $"{refreshed.Survey.SurveyorName} ({refreshed.Survey.SurveyVendor})";
                            SurveyRackLocationTextBox.Text = refreshed.Survey.MdfRackLocation;
                            SurveyNotesTextBox.Text = refreshed.Survey.StructureNotes;
                            _isPopulating = false;
                            SaveNotificationText.Text = $"⚡ Site Survey updated live from teammate ({DateTime.Now:HH:mm:ss})";
                        }
                    }
                    else if (evt.Table == "stores")
                    {
                        if (!IsGeneralFocused())
                        {
                            _isPopulating = true;
                            _store.StoreName = refreshed.StoreName;
                            _store.OperationalStatus = refreshed.OperationalStatus;
                            _store.OverallHealth = refreshed.OverallHealth;
                            _store.TargetOpenDate = refreshed.TargetOpenDate;
                            StoreNameTextBox.Text = refreshed.StoreName;
                            TargetDateTextBox.Text = refreshed.TargetOpenDate ?? refreshed.OpeningDate ?? "";
                            SelectComboItem(OperationalStatusCombo, refreshed.OperationalStatus);
                            SelectComboItem(HealthCombo, refreshed.OverallHealth);
                            _isPopulating = false;
                            SaveNotificationText.Text = $"⚡ Store status updated live from teammate ({DateTime.Now:HH:mm:ss})";
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Remote sync error: {ex.Message}");
                }
            });
        }

        private bool IsGeneralFocused() =>
            StoreNameTextBox.IsFocused || TargetDateTextBox.IsFocused || OperationalStatusCombo.IsFocused || HealthCombo.IsFocused;

        private bool IsSurveyFocused() =>
            SurveyCeilingTypeTextBox.IsFocused || SurveyCeilingHeightTextBox.IsFocused || SurveyDemarcLocationTextBox.IsFocused || 
            SurveyorTextBox.IsFocused || SurveyRackLocationTextBox.IsFocused || SurveyNotesTextBox.IsFocused;

        private bool IsPowerPolesFocused() =>
            PowerPoleInstalledTextBox.IsFocused || PowerPoleTotalTextBox.IsFocused || PowerPoleColorTextBox.IsFocused || 
            PowerPoleMountTextBox.IsFocused || PowerPoleDropsTextBox.IsFocused || PowerPoleContractorTextBox.IsFocused || PowerPoleBlockerTextBox.IsFocused;

        private bool IsCablingFocused() =>
            CablingTotalDropsTextBox.IsFocused || CablingPosDropsTextBox.IsFocused || CablingApDropsTextBox.IsFocused || 
            CablingVendorTextBox.IsFocused || CablingTechTextBox.IsFocused;

        private void SelectComboItem(ComboBox combo, string text)
        {
            foreach (ComboBoxItem item in combo.Items)
            {
                if (item.Content?.ToString()?.Equals(text, StringComparison.OrdinalIgnoreCase) == true ||
                    item.Tag?.ToString()?.Equals(text, StringComparison.OrdinalIgnoreCase) == true)
                {
                    combo.SelectedItem = item;
                    return;
                }
            }
            if (combo.Items.Count > 0 && combo.SelectedIndex < 0)
            {
                combo.SelectedIndex = 0;
            }
        }

        private void SelectComboItemByTag(ComboBox combo, string tag)
        {
            foreach (ComboBoxItem item in combo.Items)
            {
                if (item.Tag?.ToString()?.Equals(tag, StringComparison.OrdinalIgnoreCase) == true)
                {
                    combo.SelectedItem = item;
                    return;
                }
            }
            if (combo.Items.Count > 0 && combo.SelectedIndex < 0)
            {
                combo.SelectedIndex = 0;
            }
        }

        private void HealthCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            QueueAutosave("general");
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveNotificationText.Text = "⏳ Saving all sections...";
            bool allSaved = await SaveAllSectionsAsync();

            AuditHistoryService.RecordChange(
                "Store Specification",
                $"Store #{_store.StoreNumber} — {_store.StoreName}",
                "Manual Save (All Sections)",
                $"Stage: {_store.OperationalStatus}",
                $"Health: {_store.OverallHealth} | Cabling: {_store.Cabling.Vendor} | Landlord: {_store.Mall?.RequirementsStatusDisplay}",
                $"Manual save committed for Store #{_store.StoreNumber} ({_store.StoreName}).",
                _store.StoreNumber);

            RefreshStoreHistory();

            if (allSaved)
            {
                SaveNotificationText.Text = $"✓ All sections committed to DB ({DateTime.Now:HH:mm:ss})";
            }
            else
            {
                SaveNotificationText.Text = $"✓ Updated in local session ({DateTime.Now:HH:mm:ss})";
            }
        }

        private void RefreshStoreHistory()
        {
            if (StoreHistoryGrid != null && _store != null)
            {
                StoreHistoryGrid.ItemsSource = AuditHistoryService.GetEntries(_store.StoreNumber);
            }
        }

        private void ViewHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            var historyWin = new ActivityHistoryWindow(_store?.StoreNumber) { Owner = this };
            historyWin.ShowDialog();
            RefreshStoreHistory();
        }

        private void OpenMallDirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var mallWindow = new MallRulesWindow { Owner = this };
                mallWindow.ShowDialog();
                if (_store.Mall != null && SeedDataService.Malls.TryGetValue(_store.Mall.MallCode, out var updatedMall))
                {
                    _store.Mall = updatedMall;
                    PopulateStore(_store);
                }
                RefreshStoreHistory();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open Mall Directory: {ex.Message}", "Mall Directory Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
