using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using YMInc.StoreDeploymentApp.Models;
using YMInc.StoreDeploymentApp.Services;

namespace YMInc.StoreDeploymentApp.Views
{
    public partial class MallRulesWindow : Window
    {
        private readonly DatabaseService _databaseService = new();
        private List<MallProperty> _allMalls = new();
        private MallProperty? _selectedMall;
        private string _countryFilter = "ALL";
        private string _requirementsFilter = "ALL";
        private string _searchQuery = string.Empty;

        public MallRulesWindow()
        {
            InitializeComponent();
            Loaded += async (s, e) => await LoadMallsAsync();
        }

        private async System.Threading.Tasks.Task LoadMallsAsync()
        {
            CountSummaryText.Text = "Loading mall properties from database...";
            _allMalls = (await _databaseService.LoadMallsAsync()).OrderBy(m => m.PropertyName).ToList();
            ApplyFilters();

            if (_allMalls.Count > 0)
            {
                MallsGrid.SelectedIndex = 0;
            }
        }

        private void ApplyFilters()
        {
            if (!IsLoaded || MallsGrid == null || CountSummaryText == null) return;

            // Update KPI counts across all loaded malls (or current country filter)
            var baseSet = _countryFilter == "ALL"
                ? _allMalls
                : _allMalls.Where(m => m.CountryCode.Equals(_countryFilter, StringComparison.OrdinalIgnoreCase)).ToList();

            int hasReqCount = baseSet.Count(m => m.HasRequirements);
            int contactedNoReqCount = baseSet.Count(m => m.MallContacted && !m.HasRequirements);
            int notContactedCount = baseSet.Count(m => !m.MallContacted);

            if (HasReqsBadgeText != null) HasReqsBadgeText.Text = hasReqCount.ToString();
            if (ContactedNoReqsBadgeText != null) ContactedNoReqsBadgeText.Text = contactedNoReqCount.ToString();
            if (NotContactedBadgeText != null) NotContactedBadgeText.Text = notContactedCount.ToString();

            var filtered = baseSet.AsEnumerable();

            switch (_requirementsFilter)
            {
                case "HAS_REQ":
                    filtered = filtered.Where(m => m.HasRequirements);
                    break;
                case "NO_REQ_CONTACTED":
                    filtered = filtered.Where(m => m.MallContacted && !m.HasRequirements);
                    break;
                case "NOT_CONTACTED":
                    filtered = filtered.Where(m => !m.MallContacted);
                    break;
                case "ALL_CONTACTED":
                    filtered = filtered.Where(m => m.MallContacted);
                    break;
                case "ALL_NO_REQ":
                    filtered = filtered.Where(m => !m.HasRequirements);
                    break;
            }

            if (!string.IsNullOrWhiteSpace(_searchQuery))
            {
                string q = _searchQuery.Trim().ToLowerInvariant();
                filtered = filtered.Where(m =>
                    m.MallCode.ToLowerInvariant().Contains(q) ||
                    m.PropertyName.ToLowerInvariant().Contains(q) ||
                    m.LandlordCompany.ToLowerInvariant().Contains(q) ||
                    m.City.ToLowerInvariant().Contains(q) ||
                    m.StateProvince.ToLowerInvariant().Contains(q) ||
                    m.AssignedTeamMember.ToLowerInvariant().Contains(q) ||
                    m.RequirementsStatusDisplay.ToLowerInvariant().Contains(q)
                );
            }

            var list = filtered.ToList();
            MallsGrid.ItemsSource = list;
            CountSummaryText.Text = $"Showing {list.Count} of {_allMalls.Count} Mall Properties ({hasReqCount} Has Req • {contactedNoReqCount} Contacted/No Req • {notContactedCount} Not Contacted)";
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsLoaded || SearchBox == null) return;
            _searchQuery = SearchBox.Text;
            ApplyFilters();
        }

        private void RequirementsFilterCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded || RequirementsFilterCombo == null) return;
            if (RequirementsFilterCombo.SelectedItem is ComboBoxItem item && item.Tag is string tag)
            {
                _requirementsFilter = tag;
                ApplyFilters();
            }
        }

        private void CountryFilterCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded || CountryFilterCombo == null) return;
            if (CountryFilterCombo.SelectedItem is ComboBoxItem item && item.Tag is string tag)
            {
                _countryFilter = tag;
                ApplyFilters();
            }
        }

        private void MallsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded || MallsGrid == null) return;
            if (MallsGrid.SelectedItem is MallProperty mall)
            {
                _selectedMall = mall;
                PopulateForm(mall);
            }
        }

        private void MallsGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit && e.Row.Item is MallProperty mall)
            {
                string colHeader = e.Column.Header?.ToString() ?? "Property Field";
                string newVal = (e.EditingElement as TextBox)?.Text ?? string.Empty;

                Dispatcher.BeginInvoke(new Action(async () =>
                {
                    bool saved = await _databaseService.SaveMallAsync(mall);
                    AuditHistoryService.RecordChange(
                        "Landlord Compliance",
                        $"{mall.MallCode} — {mall.PropertyName}",
                        $"Inline Grid Edit ({colHeader})",
                        "Previous Cell Value",
                        newVal,
                        $"Edited {colHeader} on {mall.PropertyName} ({mall.MallCode}) → '{newVal}'");
                    PopulateForm(mall);
                    if (FormStatusText != null)
                    {
                        FormStatusText.Text = saved
                            ? $"✓ Saved cell edit for '{mall.PropertyName}' ({mall.RequirementsStatusDisplay}) at {DateTime.Now:HH:mm:ss}"
                            : $"✓ Updated '{mall.PropertyName}' in session & audit log ({DateTime.Now:HH:mm:ss})";
                    }
                }), System.Windows.Threading.DispatcherPriority.Background);
            }
        }

        private void PopulateForm(MallProperty mall)
        {
            FormHeaderTitle.Text = $"Edit: {mall.PropertyName} ({mall.RequirementsStatusDisplay})";
            FormHeaderSubtitle.Text = $"Landlord: {mall.LandlordCompany} | Code: {mall.MallCode} | Team: {(string.IsNullOrWhiteSpace(mall.AssignedTeamMember) ? "Unassigned" : mall.AssignedTeamMember)}";

            FormMallCodeText.Text = mall.MallCode;
            FormPropertyNameText.Text = mall.PropertyName;
            FormAddressText.Text = mall.Address;
            FormLandlordText.Text = mall.LandlordCompany;
            FormCityText.Text = mall.City;
            FormStateProvinceText.Text = mall.StateProvince;
            FormDeliveryWindowText.Text = mall.DeliveryWindow;
            FormLiabilityText.Text = mall.MinGeneralLiabilityCad.ToString("F0");
            FormCablingVendorText.Text = mall.CablingVendor;
            FormLeadTechText.Text = mall.LeadTechnicianName ?? "";
            FormTeamMemberText.Text = mall.AssignedTeamMember ?? "";
            FormSpecialRulesText.Text = mall.SpecialRules;
            FormCircuitsRulesText.Text = mall.RequirementsCircuits;
            FormIntermediateStepsText.Text = mall.IntermediateSteps;
            FormContactNameText.Text = mall.ManagementContactName;
            FormContactPhoneText.Text = mall.ManagementPhone;
            FormContactEmailText.Text = mall.ManagementEmail;
            FormSecurityPhoneText.Text = mall.SecurityPhone;

            // Country
            foreach (ComboBoxItem item in FormCountryCombo.Items)
            {
                if (item.Tag?.ToString() == mall.CountryCode)
                {
                    FormCountryCombo.SelectedItem = item;
                    break;
                }
            }

            // Property Type
            bool foundType = false;
            foreach (ComboBoxItem item in FormPropertyTypeCombo.Items)
            {
                if (item.Content?.ToString() == mall.PropertyType)
                {
                    FormPropertyTypeCombo.SelectedItem = item;
                    foundType = true;
                    break;
                }
            }
            if (!foundType && FormPropertyTypeCombo.Items.Count > 0)
            {
                FormPropertyTypeCombo.SelectedIndex = 0;
            }

            // Mall Contacted (Yes/No)
            foreach (ComboBoxItem item in FormMallContactedCombo.Items)
            {
                if (item.Tag?.ToString()?.Equals(mall.MallContacted.ToString(), StringComparison.OrdinalIgnoreCase) == true)
                {
                    FormMallContactedCombo.SelectedItem = item;
                    break;
                }
            }

            // COI Required (Yes/No)
            foreach (ComboBoxItem item in FormCoiCombo.Items)
            {
                if (item.Tag?.ToString()?.Equals(mall.CoiRequired.ToString(), StringComparison.OrdinalIgnoreCase) == true)
                {
                    FormCoiCombo.SelectedItem = item;
                    break;
                }
            }

            // After Hours Required (Yes/No)
            foreach (ComboBoxItem item in FormAfterHoursCombo.Items)
            {
                if (item.Tag?.ToString()?.Equals(mall.NightWorkOnly.ToString(), StringComparison.OrdinalIgnoreCase) == true)
                {
                    FormAfterHoursCombo.SelectedItem = item;
                    break;
                }
            }

            FormStatusText.Text = string.Empty;
        }

        private void AddNewMallButton_Click(object sender, RoutedEventArgs e)
        {
            _selectedMall = null;
            MallsGrid.SelectedItem = null;

            FormHeaderTitle.Text = "New Property Compliance Record";
            FormHeaderSubtitle.Text = "Enter property details, outreach status, COI mandates, and delivery windows.";

            FormMallCodeText.Text = "MALL-NEW";
            FormPropertyNameText.Text = "";
            FormAddressText.Text = "";
            FormLandlordText.Text = "";
            FormCityText.Text = "";
            FormStateProvinceText.Text = "";
            FormDeliveryWindowText.Text = "07:00 - 10:00";
            FormLiabilityText.Text = "5000000";
            FormCablingVendorText.Text = "";
            FormLeadTechText.Text = "";
            FormTeamMemberText.Text = "";
            FormSpecialRulesText.Text = "";
            FormCircuitsRulesText.Text = "";
            FormIntermediateStepsText.Text = "";
            FormContactNameText.Text = "";
            FormContactPhoneText.Text = "";
            FormContactEmailText.Text = "";
            FormSecurityPhoneText.Text = "";

            FormCountryCombo.SelectedIndex = 1; // US default
            FormPropertyTypeCombo.SelectedIndex = 0;
            FormMallContactedCombo.SelectedIndex = 0; // Contacted
            FormCoiCombo.SelectedIndex = 0; // Yes
            FormAfterHoursCombo.SelectedIndex = 1; // No

            FormStatusText.Text = "Ready to create new mall. Fill out the form and click 'Save Requirements'.";
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string code = FormMallCodeText.Text.Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(code))
            {
                MessageBox.Show("Mall Code is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string name = FormPropertyNameText.Text.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Property Name is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Extract form values
            string country = (FormCountryCombo.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "US";
            string address = FormAddressText.Text.Trim();
            string landlord = FormLandlordText.Text.Trim();
            string propType = (FormPropertyTypeCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Enclosed Regional Mall";
            string city = FormCityText.Text.Trim();
            string state = FormStateProvinceText.Text.Trim();
            string delivery = FormDeliveryWindowText.Text.Trim();
            string cablingVendor = FormCablingVendorText.Text.Trim();
            string leadTech = FormLeadTechText.Text.Trim();
            string teamMember = FormTeamMemberText.Text.Trim();
            string contactName = FormContactNameText.Text.Trim();
            string contactPhone = FormContactPhoneText.Text.Trim();
            string contactEmail = FormContactEmailText.Text.Trim();
            string securityPhone = FormSecurityPhoneText.Text.Trim();

            bool mallContacted = true;
            if (FormMallContactedCombo.SelectedItem is ComboBoxItem mcItem && mcItem.Tag?.ToString() == "False")
            {
                mallContacted = false;
            }

            bool coiRequired = true;
            if (FormCoiCombo.SelectedItem is ComboBoxItem coiItem && coiItem.Tag?.ToString() == "False")
            {
                coiRequired = false;
            }

            bool afterHours = false;
            if (FormAfterHoursCombo.SelectedItem is ComboBoxItem ahItem && ahItem.Tag?.ToString() == "True")
            {
                afterHours = true;
            }

            decimal.TryParse(FormLiabilityText.Text.Replace("$", "").Replace(",", "").Trim(), out decimal liability);
            if (liability <= 0) liability = 5000000m;

            string rules = FormSpecialRulesText.Text.Trim();
            string circuitRules = FormCircuitsRulesText.Text.Trim();
            string intermediateSteps = FormIntermediateStepsText.Text.Trim();

            if (_selectedMall == null)
            {
                // Creating new mall
                var newMall = new MallProperty
                {
                    MallId = _allMalls.Count + 1,
                    MallCode = code,
                    PropertyName = name,
                    Address = address,
                    LandlordCompany = landlord,
                    PropertyType = propType,
                    CountryCode = country,
                    City = city,
                    StateProvince = state,
                    MallContacted = mallContacted,
                    AssignedTeamMember = teamMember,
                    CoiRequired = coiRequired,
                    NightWorkOnly = afterHours,
                    DeliveryWindow = delivery,
                    MinGeneralLiabilityCad = liability,
                    CablingVendor = cablingVendor,
                    LeadTechnicianName = leadTech,
                    SpecialRules = rules,
                    RequirementsCircuits = circuitRules,
                    IntermediateSteps = intermediateSteps,
                    ManagementContactName = contactName,
                    ManagementPhone = contactPhone,
                    ManagementEmail = contactEmail,
                    SecurityPhone = securityPhone
                };

                _allMalls.Add(newMall);
                _selectedMall = newMall;
            }
            else
            {
                // Updating existing
                _selectedMall.MallCode = code;
                _selectedMall.PropertyName = name;
                _selectedMall.Address = address;
                _selectedMall.LandlordCompany = landlord;
                _selectedMall.PropertyType = propType;
                _selectedMall.CountryCode = country;
                _selectedMall.City = city;
                _selectedMall.StateProvince = state;
                _selectedMall.MallContacted = mallContacted;
                _selectedMall.AssignedTeamMember = teamMember;
                _selectedMall.CoiRequired = coiRequired;
                _selectedMall.NightWorkOnly = afterHours;
                _selectedMall.DeliveryWindow = delivery;
                _selectedMall.MinGeneralLiabilityCad = liability;
                _selectedMall.CablingVendor = cablingVendor;
                _selectedMall.LeadTechnicianName = leadTech;
                _selectedMall.SpecialRules = rules;
                _selectedMall.RequirementsCircuits = circuitRules;
                _selectedMall.IntermediateSteps = intermediateSteps;
                _selectedMall.ManagementContactName = contactName;
                _selectedMall.ManagementPhone = contactPhone;
                _selectedMall.ManagementEmail = contactEmail;
                _selectedMall.SecurityPhone = securityPhone;
            }

            bool saved = await _databaseService.SaveMallAsync(_selectedMall);

            AuditHistoryService.RecordChange(
                "Landlord Compliance",
                $"{_selectedMall.MallCode} — {_selectedMall.PropertyName}",
                "Landlord Compliance Record Save",
                $"Outreach: {_selectedMall.ContactedDisplay}",
                $"{_selectedMall.RequirementsStatusDisplay} | COI: {_selectedMall.CoiDisplay} | Cabling: {_selectedMall.CablingVendor}",
                $"Saved landlord compliance record for {_selectedMall.PropertyName} ({_selectedMall.MallCode}).");

            ApplyFilters();
            MallsGrid.SelectedItem = _selectedMall;
            FormStatusText.Text = saved
                ? $"✓ Saved '{name}' ({_selectedMall.RequirementsStatusDisplay}) at {DateTime.Now:HH:mm:ss}!"
                : $"✓ Updated '{name}' in session & audit log ({DateTime.Now:HH:mm:ss})";
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
