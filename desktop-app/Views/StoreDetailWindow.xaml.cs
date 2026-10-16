using System;
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
        private readonly DatabaseService _databaseService = new();

        public StoreDetailWindow(StoreDeployment store)
        {
            InitializeComponent();
            _store = store;
            PopulateStore(store);
        }

        private void PopulateStore(StoreDeployment store)
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

            // Mall Landlord Rules & Contacts (Directly Editable)
            if (store.Mall != null)
            {
                MallNameHeading.Text = $"{store.Mall.PropertyName} Compliance & Delivery Matrix";
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
            }
            else
            {
                MallNameHeading.Text = "Street Front / Non-Mall Location";
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
            }
        }

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
            // Dynamically react if needed
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Save Header & General Store details
            _store.StoreName = StoreNameTextBox.Text.Trim();
            _store.TargetOpenDate = TargetDateTextBox.Text.Trim();
            if (OperationalStatusCombo.SelectedItem is ComboBoxItem statusItem)
            {
                _store.OperationalStatus = statusItem.Content?.ToString() ?? _store.OperationalStatus;
            }
            if (HealthCombo.SelectedItem is ComboBoxItem healthItem)
            {
                _store.OverallHealth = healthItem.Content?.ToString() ?? _store.OverallHealth;
            }

            // Save Site Survey
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

            // Save Power Poles
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

            // Save Cabling
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

            // Save Mall Landlord Rules & Contacts directly!
            bool mallSaved = true;
            string? mallError = null;
            if (_store.Mall != null)
            {
                if (MallCoiCombo.SelectedItem is ComboBoxItem coiItem && coiItem.Tag is string coiTag)
                    _store.Mall.CoiRequired = coiTag.Equals("True", StringComparison.OrdinalIgnoreCase);

                if (MallAfterHoursCombo.SelectedItem is ComboBoxItem ahItem && ahItem.Tag is string ahTag)
                    _store.Mall.NightWorkOnly = ahTag.Equals("True", StringComparison.OrdinalIgnoreCase);

                _store.Mall.DeliveryWindow = MallDeliveryTextBox.Text.Trim();
                _store.Mall.LandlordCompany = MallLandlordCompanyTextBox.Text.Trim();

                if (decimal.TryParse(MallLiabilityTextBox.Text.Replace("$", "").Replace(",", "").Trim(), out decimal liab))
                    _store.Mall.MinGeneralLiabilityCad = liab;

                _store.Mall.ManagementContactName = MallContactNameTextBox.Text.Trim();
                _store.Mall.ManagementPhone = MallContactPhoneTextBox.Text.Trim();
                _store.Mall.ManagementEmail = MallContactEmailTextBox.Text.Trim();
                _store.Mall.SecurityPhone = MallSecurityPhoneTextBox.Text.Trim();
                _store.Mall.SpecialRules = MallSpecialRulesTextBox.Text.Trim();

                mallSaved = await _databaseService.SaveMallAsync(_store.Mall);
                if (!mallSaved) mallError = _databaseService.LastError;
            }

            bool storeSaved = await _databaseService.SaveStoreAsync(_store);

            if (storeSaved && mallSaved)
            {
                SaveNotificationText.Text = $"✓ Store #{_store.StoreNumber} & mall specifications successfully saved!";
            }
            else
            {
                string detail = !storeSaved ? _databaseService.LastError : mallError;
                SaveNotificationText.Text = $"⚠ Save failed: {detail ?? "unknown error"}";
            }
        }

        private void OpenMallDirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var mallWindow = new MallRulesWindow { Owner = this };
                mallWindow.ShowDialog();
                // Refresh mall specs if updated
                if (_store.Mall != null && SeedDataService.Malls.TryGetValue(_store.Mall.MallCode, out var updatedMall))
                {
                    _store.Mall = updatedMall;
                    PopulateStore(_store);
                }
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
