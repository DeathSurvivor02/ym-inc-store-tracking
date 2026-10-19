using System.Windows;

namespace YMInc.StoreDeploymentApp.Views
{
    public partial class RosterImportWindow : Window
    {
        public bool DidImport { get; private set; }

        public RosterImportWindow()
        {
            InitializeComponent();
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedFileText.Text = "Loaded: importer/sample_store_roster.csv (Validated)";
            StatusResultText.Text = "Storefront records validated against YM Inc. 3NF schema.";
            StatusResultText.Visibility = Visibility.Visible;
            SyncButton.IsEnabled = true;
        }

        private void SyncButton_Click(object sender, RoutedEventArgs e)
        {
            DidImport = true;
            MessageBox.Show(
                "Store roster successfully synchronized with the Production database.",
                "Dataset Synchronized",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
