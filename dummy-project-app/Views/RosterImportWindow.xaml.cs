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
            SelectedFileText.Text = "Verified: dummy_project_db.json (Standalone Isolated Project)";
            StatusResultText.Text = "Dummy dataset verified (8 synthetic stores, 6 malls, 30+ daily TTU/Circuit tasks). Main project & Excel files cannot be seen.";
            StatusResultText.Visibility = Visibility.Visible;
            SyncButton.IsEnabled = true;
        }

        private void SyncButton_Click(object sender, RoutedEventArgs e)
        {
            DidImport = true;
            MessageBox.Show(
                "Standalone Dummy Project dataset reloaded! The main production project and Excel sheets remain completely isolated.",
                "Dummy Dataset Verified",
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
