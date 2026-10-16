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

            var filtered = _allMalls.AsEnumerable();

            if (_countryFilter != "ALL")
            {
                filtered = filtered.Where(m => m.CountryCode.Equals(_countryFilter, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(_searchQuery))
            {
                string q = _searchQuery.Trim().ToLowerInvariant();
                filtered = filtered.Where(m =>
                    m.MallCode.ToLowerInvariant().Contains(q) ||
                    m.PropertyName.ToLowerInvariant().Contains(q) ||
                    m.LandlordCompany.ToLowerInvariant().Contains(q) ||
                    m.City.ToLowerInvariant().Contains(q) ||
                    m.StateProvince.ToLowerInvariant().Contains(q)
                );
            }

            var list = filtered.ToList();
            MallsGrid.ItemsSource = list;
            CountSummaryText.Text = $"Showing {list.Count} of {_allMalls.Count} Mall Properties";
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsLoaded || SearchBox == null) return;
            _searchQuery = SearchBox.Text;
            ApplyFilters();
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

        private void PopulateForm(MallProperty mall)
        {
            FormHeaderTitle.Text = $"Edit: {mall.PropertyName}";
            FormHeaderSubtitle.Text = $"Landlord: {mall.LandlordCompany} | Code: {mall.MallCode}";

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
            FormSpecialRulesText.Text = mall.SpecialRules;
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

            FormHeaderTitle.Text = "➕ Add New Mall Landlord Requirements";
            FormHeaderSubtitle.Text = "Fill in the property details, COI requirement, after-hours protocols, and delivery windows.";

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
            FormSpecialRulesText.Text = "";
            FormContactNameText.Text = "";
            FormContactPhoneText.Text = "";
            FormContactEmailText.Text = "";
            FormSecurityPhoneText.Text = "";

            FormCountryCombo.SelectedIndex = 0;
            FormPropertyTypeCombo.SelectedIndex = 0;
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
            string country = (FormCountryCombo.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "CA";
            string address = FormAddressText.Text.Trim();
            string landlord = FormLandlordText.Text.Trim();
            string propType = (FormPropertyTypeCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Enclosed Regional Mall";
            string city = FormCityText.Text.Trim();
            string state = FormStateProvinceText.Text.Trim();
            string delivery = FormDeliveryWindowText.Text.Trim();
            string cablingVendor = FormCablingVendorText.Text.Trim();
            string leadTech = FormLeadTechText.Text.Trim();
            string contactName = FormContactNameText.Text.Trim();
            string contactPhone = FormContactPhoneText.Text.Trim();
            string contactEmail = FormContactEmailText.Text.Trim();
            string securityPhone = FormSecurityPhoneText.Text.Trim();

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
                    CoiRequired = coiRequired,
                    NightWorkOnly = afterHours,
                    DeliveryWindow = delivery,
                    MinGeneralLiabilityCad = liability,
                    CablingVendor = cablingVendor,
                    LeadTechnicianName = leadTech,
                    SpecialRules = rules,
                    ManagementContactName = contactName,
                    ManagementPhone = contactPhone,
                    ManagementEmail = contactEmail,
                    SecurityPhone = securityPhone
                };

                SeedDataService.Malls[code] = newMall;
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
                _selectedMall.CoiRequired = coiRequired;
                _selectedMall.NightWorkOnly = afterHours;
                _selectedMall.DeliveryWindow = delivery;
                _selectedMall.MinGeneralLiabilityCad = liability;
                _selectedMall.CablingVendor = cablingVendor;
                _selectedMall.LeadTechnicianName = leadTech;
                _selectedMall.SpecialRules = rules;
                _selectedMall.ManagementContactName = contactName;
                _selectedMall.ManagementPhone = contactPhone;
                _selectedMall.ManagementEmail = contactEmail;
                _selectedMall.SecurityPhone = securityPhone;

                SeedDataService.Malls[code] = _selectedMall;
            }

            bool saved = await _databaseService.SaveMallAsync(_selectedMall);

            ApplyFilters();
            MallsGrid.SelectedItem = _selectedMall;
            FormStatusText.Text = saved
                ? $"✓ Requirements and contacts for '{name}' saved successfully!"
                : $"⚠ '{name}' was NOT saved to the database: {_databaseService.LastError ?? "unknown error"}";
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
