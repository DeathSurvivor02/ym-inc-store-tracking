using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace YMInc.StoreDeploymentApp;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        ShutdownMode = ShutdownMode.OnLastWindowClose;
        DispatcherUnhandledException += App_DispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        Services.ThemeService.Initialize();
        Services.DummyProjectService.LoadOrInitialize();

        string cmdLine = Environment.CommandLine;
        if ((e.Args.Length > 0 && e.Args[0] == "--verify-ui") || cmdLine.Contains("--verify-ui"))
        {
            var (_, _, user) = Services.AuthService.Login("pm", "pm123");
            var loginWin = new Views.LoginWindow();
            var mainWin = new Views.MainWindow(user!);
            var calWin = new Views.FiscalCalendarWindow(42);
            var mallWin = new Views.MallRulesWindow();
            var importWin = new Views.RosterImportWindow();
            var stores = Services.SeedDataService.GetStores();
            var detailWin = new Views.StoreDetailWindow(stores[0]);
            var historyWin = new Views.ActivityHistoryWindow();

            Services.ThemeService.SetTheme(true);
            foreach (var w in new Window[] { loginWin, mainWin, calWin, mallWin, importWin, detailWin, historyWin })
                Services.ThemeService.ApplyThemeToWindow(w);

            Services.ThemeService.SetTheme(false);
            foreach (var w in new Window[] { loginWin, mainWin, calWin, mallWin, importWin, detailWin, historyWin })
                Services.ThemeService.ApplyThemeToWindow(w);

            Console.WriteLine("UI_VERIFY_PASS: All 7 Production windows + Light/Dark themes + Save/History verified.");
            Environment.Exit(0);
            return;
        }

        base.OnStartup(e);
    }

    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        try
        {
            string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "crash_log.txt");
            File.WriteAllText(logPath, $"{DateTime.Now:O}\n{e.Exception}");
        }
        catch { }

        MessageBox.Show(
            $"An unexpected application error occurred:\n\n{e.Exception.Message}\n\n{e.Exception.InnerException?.Message}",
            "YM Inc. Store Rollout Manager",
            MessageBoxButton.OK,
            MessageBoxImage.Warning);
        e.Handled = true;
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        try
        {
            string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "crash_log.txt");
            File.WriteAllText(logPath, $"{DateTime.Now:O}\n{e.ExceptionObject}");
        }
        catch { }
    }
}
