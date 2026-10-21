using System;
using System.Windows;
using System.Windows.Media;
using YMInc.StoreDeploymentApp.Services;

namespace YMInc.StoreDeploymentApp.Views
{
    public partial class ResetPasswordWindow : Window
    {
        public bool PasswordWasReset { get; private set; }
        public string? ResetUsername { get; private set; }

        public ResetPasswordWindow(string? prefilledIdentifier = null)
        {
            InitializeComponent();

            if (!string.IsNullOrWhiteSpace(prefilledIdentifier))
            {
                UserIdentifierTextBox.Text = prefilledIdentifier.Trim();
                NewPasswordBox.Focus();
            }

            Loaded += (s, e) => ThemeService.ApplyThemeToWindow(this);
            ThemeService.ThemeChanged += (s, e) => ThemeService.ApplyThemeToWindow(this);
        }

        private async void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            string identifier = UserIdentifierTextBox.Text.Trim();
            string newPassword = NewPasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;

            if (string.IsNullOrWhiteSpace(identifier))
            {
                ShowMessage("Please enter your username or registered corporate email.", isError: true);
                UserIdentifierTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(newPassword))
            {
                ShowMessage("Please enter a new password.", isError: true);
                NewPasswordBox.Focus();
                return;
            }

            if (newPassword.Length < 6)
            {
                ShowMessage("Password must be at least 6 characters in length.", isError: true);
                NewPasswordBox.Focus();
                return;
            }

            if (newPassword != confirmPassword)
            {
                ShowMessage("Passwords do not match. Please re-enter identical passwords.", isError: true);
                ConfirmPasswordBox.Focus();
                return;
            }

            SubmitButton.IsEnabled = false;
            SubmitButton.Content = "Updating...";

            try
            {
                var (success, message) = await AuthService.ResetPasswordAsync(identifier, newPassword);

                if (success)
                {
                    PasswordWasReset = true;
                    ResetUsername = identifier;
                    ShowMessage("✓ " + message, isError: false);

                    MessageBox.Show(
                        "Your password has been securely updated in the database with Salt & Pepper encryption.\n\nYou can now sign in with your new credentials.",
                        "Password Reset Successful",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    DialogResult = true;
                    Close();
                }
                else
                {
                    ShowMessage(message, isError: true);
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Error resetting password: {ex.Message}", isError: true);
            }
            finally
            {
                SubmitButton.IsEnabled = true;
                SubmitButton.Content = "Reset Password";
            }
        }

        private void ShowMessage(string text, bool isError)
        {
            StatusText.Text = text;
            if (isError)
            {
                StatusBanner.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FEE2E2"));
                StatusBanner.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FCA5A5"));
                StatusText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#991B1B"));
            }
            else
            {
                StatusBanner.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DCFCE7"));
                StatusBanner.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#86EFAC"));
                StatusText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#166534"));
            }
            StatusBanner.Visibility = Visibility.Visible;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
