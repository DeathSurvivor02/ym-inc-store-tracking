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
                SurveyCeilingHeightTextBox.Text = store.Survey.CeilingHeightFeet.ToString("F1");
                SelectComboItem(SurveyStatusCombo, store.Survey.Status);
                SurveyDemarcLocationTextBox.Text = store.Survey.DemarcLocation;
                SelectComboItemByTag(SurveyConduitCombo, store.Survey.DemarcConduitAvailable.ToString());
                SurveyorTextBox.Text = string.IsNullOrEmpty(store.Survey.SurveyorName) ? 
                    store.Survey.SurveyVendor : $"{store.Survey.SurveyorName} ({store.Survey.SurveyVendor})";
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

            // Mall Rules (No Union Labor)
            if (store.Mall != null)
            {
                MallNameHeading.Text = $"{store.Mall.PropertyName} Compliance & Delivery Matrix";
                MallLandlordCompanyText.Text = store.Mall.LandlordCompany;
                
                MallCoiRequiredText.Text = store.Mall.CoiRequired ? "Mandatory (COI Required)" : "Not Required / Exempt";
                MallCoiRequiredText.Foreground = store.Mall.CoiRequired ? 
                    new SolidColorBrush((Color)ColorConverter.ConvertFromString("#34D399")) : 
                    new SolidColorBrush((Color)ColorConverter.ConvertFromString("#94A3B8"));

                MallAfterHoursText.Text = store.Mall.NightWorkOnly ? "Mandatory Night Shift Only" : "Daytime Work Permitted";
                MallAfterHoursText.Foreground = store.Mall.NightWorkOnly ? 
                    new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FBBF24")) : 
                    new SolidColorBrush((Color)ColorConverter.ConvertFromString("#94A3B8"));

                MallDeliveryText.Text = store.Mall.DeliveryWindow;
                MallLiabilityText.Text = $"${store.Mall.MinGeneralLiabilityCad / 1000000:F0}M CAD General Liability";
                MallBadgingText.Text = $"${store.Mall.BadgingFeePerTech:F0} CAD / tech";
                MallSpecialRulesText.Text = string.IsNullOrEmpty(store.Mall.SpecialRules) ? 
                    "No additional landlord restrictions noted. Standard PPE and loading dock sign-in required." : 
                    store.Mall.SpecialRules;

                MallContactNameText.Text = string.IsNullOrEmpty(store.Mall.ManagementContactName) ? "Mall Operations Office" : store.Mall.ManagementContactName;
                MallContactPhoneText.Text = string.IsNullOrEmpty(store.Mall.ManagementPhone) ? (store.PhoneNumber ?? "N/A") : store.Mall.ManagementPhone;
                MallContactEmailText.Text = string.IsNullOrEmpty(store.Mall.ManagementEmail) ? "operations@mall.com" : store.Mall.ManagementEmail;
                MallSecurityPhoneText.Text = string.IsNullOrEmpty(store.Mall.SecurityPhone) ? "N/A" : store.Mall.SecurityPhone;
            }
            else
            {
                MallNameHeading.Text = "Street Front / Non-Mall Location";
                MallLandlordCompanyText.Text = "Direct Commercial Lease";
                MallCoiRequiredText.Text = "Standard Commercial Policy";
                MallAfterHoursText.Text = "Daytime Permitted";
                MallDeliveryText.Text = "Standard Business Hours";
                MallLiabilityText.Text = "$5M CAD";
                MallBadgingText.Text = "$0 (No Mall Security)";
                MallSpecialRulesText.Text = "Direct street-front retail delivery.";
                MallContactNameText.Text = "Direct Lease Admin";
                MallContactPhoneText.Text = store.PhoneNumber ?? "N/A";
                MallContactEmailText.Text = "operations@yminc.ca";
                MallSecurityPhoneText.Text = "N/A";
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

        private void SaveButton_Click(object sender, RoutedEventArgs e)
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

            SaveNotificationText.Text = $"✓ Store #{_store.StoreNumber} specifications successfully saved!";
        }

        private void OpenMallDirectoryButton_Click(object sender, RoutedEventArgs e)
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

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
