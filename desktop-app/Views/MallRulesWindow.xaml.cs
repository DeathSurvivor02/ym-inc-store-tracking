using System.Linq;
using System.Windows;
using YMInc.StoreDeploymentApp.Services;

namespace YMInc.StoreDeploymentApp.Views
{
    public partial class MallRulesWindow : Window
    {
        public MallRulesWindow()
        {
            InitializeComponent();
            MallsGrid.ItemsSource = SeedDataService.Malls.Values.ToList();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
