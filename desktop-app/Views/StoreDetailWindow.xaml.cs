using System.Windows;
using System.Windows.Media;
using YMInc.StoreDeploymentApp.Models;

namespace YMInc.StoreDeploymentApp.Views
{
    public partial class StoreDetailWindow : Window
    {
        public StoreDetailWindow(StoreDeployment store)
        {
            InitializeComponent();
            PopulateStore(store);
        }

        private void PopulateStore(StoreDeployment store)
        {
            BannerCodeBadge.Text = store.BannerCode;
            StoreNumberText.Text = $"Store #{store.StoreNumber}";
            StoreNameText.Text = store.StoreName;
            StoreLocationText.Text = $"{store.Address}, {store.City}, {store.StateProvince} {store.PostalCode}";
            TargetDateText.Text = store.TargetOpenDate ?? store.OpeningDate ?? "TBD";
            StageText.Text = store.OperationalStatus;

            HealthText.Text = store.OverallHealth;
            if (store.OverallHealth == "At Risk")
            {
                HealthBadge.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#451A03"));
                HealthText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FBBF24"));
            }
            else if (store.OverallHealth == "Critical Blocker")
            {
                HealthBadge.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#450A0A"));
                HealthText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F87171"));
            }

            // Site Survey
            if (store.Survey != null)
            {
                SurveyCeilingTypeText.Text = store.Survey.CeilingType;
                SurveyCeilingHeightText.Text = $"{store.Survey.CeilingHeightFeet:F1} ft";
                SurveyStatusText.Text = store.Survey.Status;
                SurveyDemarcLocationText.Text = store.Survey.DemarcLocation;
                SurveyConduitText.Text = store.Survey.DemarcConduitAvailable ? "Clear & Available" : "Blocked / Missing";
                SurveyorText.Text = $"{store.Survey.SurveyorName} ({store.Survey.SurveyVendor})";
            }

            // Power Poles
            if (store.PowerPoles != null)
            {
                PowerPoleStatusText.Text = store.PowerPoles.Status;
                PowerPoleCountText.Text = $"{store.PowerPoles.InstalledCount} of {store.PowerPoles.TotalRequired} Installed";
                PowerPoleColorText.Text = store.PowerPoles.FinishColor;
                PowerPoleMountText.Text = store.PowerPoles.CeilingMountType;
                PowerPoleDropsText.Text = $"{store.PowerPoles.CashWrapFeedDrops} Feed Drops";
                PowerPoleContractorText.Text = store.PowerPoles.ElectricalContractor ?? "TBD";

                if (!string.IsNullOrEmpty(store.PowerPoles.BlockerReason))
                {
                    PowerPoleBlockerText.Text = $"⚠️ Blocker Alert: {store.PowerPoles.BlockerReason}";
                    PowerPoleBlockerBorder.Visibility = Visibility.Visible;
                }
            }

            // Cabling
            if (store.Cabling != null)
            {
                CablingStatusText.Text = store.Cabling.Status;
                CablingTotalDropsText.Text = $"{store.Cabling.TotalDataDrops} Drops ({store.Cabling.CableGrade})";
                CablingPosApText.Text = $"{store.Cabling.PosDrops} POS Drops | {store.Cabling.ApDrops} AP Wireless Drops";
                CablingFlukeText.Text = store.Cabling.FlukeCertified ? "✓ Fluke Test Certified" : "Pending Certification";
                CablingVendorText.Text = store.Cabling.Vendor;
                CablingTechText.Text = store.Cabling.LeadTechName ?? "TBD";
            }

            // Circuits
            CircuitsItemsControl.ItemsSource = store.Circuits;

            // Mall Rules
            if (store.Mall != null)
            {
                MallNameHeading.Text = $"{store.Mall.PropertyName} Landlord Rules ({store.Mall.LandlordCompany})";
                MallUnionText.Text = store.Mall.UnionLaborRequired ? $"MANDATORY UNION ({store.Mall.UnionLocalDesignation})" : "Open Shop Permitted";
                MallUnionText.Foreground = store.Mall.UnionLaborRequired ? 
                    new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F87171")) : 
                    new SolidColorBrush((Color)ColorConverter.ConvertFromString("#34D399"));

                MallDeliveryText.Text = store.Mall.DeliveryWindow;
                MallCoiText.Text = $"${store.Mall.MinGeneralLiabilityCad / 1000000:F0}M CAD General Liability";
                MallBadgingText.Text = $"${store.Mall.BadgingFeePerTech:F0} CAD / technician badge";
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
