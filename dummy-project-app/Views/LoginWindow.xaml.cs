using System;
using System.Windows;
using YMInc.StoreDeploymentApp.Services;

namespace YMInc.StoreDeploymentApp.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            UpdateThemeButtonLabel();
        }

        private void ThemeToggleButton_Click(object sender, RoutedEventArgs e)
        {
            ThemeService.ToggleTheme();
            UpdateThemeButtonLabel();
        }

        private void UpdateThemeButtonLabel()
        {
            if (ThemeToggleButton != null)
            {
                ThemeToggleButton.Content = ThemeService.ToggleButtonText;
            }
        }

        private void SignInButton_Click(object sender, RoutedEventArgs e)
        {
            AttemptLogin(UsernameTextBox.Text, PasswordBox.Password);
        }

        private void QuickLoginPM_Click(object sender, RoutedEventArgs e)
        {
            UsernameTextBox.Text = "pm";
            PasswordBox.Password = "pm123";
        }

        private void QuickLoginPA_Click(object sender, RoutedEventArgs e)
        {
            UsernameTextBox.Text = "pa";
            PasswordBox.Password = "pa123";
        }

        private void QuickLoginTech_Click(object sender, RoutedEventArgs e)
        {
            UsernameTextBox.Text = "ps1";
            PasswordBox.Password = "ps123";
        }

        private void AttemptLogin(string username, string password)
        {
            try
            {
                var (success, message, user) = AuthService.Login(username, password);
                if (success && user != null)
                {
                    var mainWindow = new MainWindow(user);
                    Application.Current.MainWindow = mainWindow;
                    mainWindow.Show();
                    Close();
                }
                else
                {
                    ErrorText.Text = message;
                    ErrorBanner.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                ErrorText.Text = $"Error opening workspace: {ex.Message}";
                ErrorBanner.Visibility = Visibility.Visible;
            }
        }
    }
}
