using System.Windows;
using System.Windows.Controls;
using YMInc.StoreDeploymentApp.Services;

namespace YMInc.StoreDeploymentApp.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void DemoProfileCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DemoProfileCombo.SelectedItem is ComboBoxItem item && item.Tag is string tag)
            {
                var parts = tag.Split(':');
                if (parts.Length == 2)
                {
                    UsernameBox.Text = parts[0];
                    PasswordBox.Password = parts[1];
                    ErrorTextBlock.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameBox.Text;
            string password = PasswordBox.Password;

            var (success, message, user) = AuthService.Login(username, password);

            if (!success || user == null)
            {
                ErrorTextBlock.Text = message;
                ErrorTextBlock.Visibility = Visibility.Visible;
                return;
            }

            // Launch Main Application Window
            var mainWindow = new MainWindow(user);
            mainWindow.Show();
            Close();
        }
    }
}
