using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using CafeManagement.Client.Services;
using CafeManagement.Client.Services.Interfaces;
using CafeManagement.Client.ViewModels;
using CafeManagement.Client.Views;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace CafeManagement.Client;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    private IHost? _host;
    public IServiceProvider ServiceProvider => _host?.Services ?? throw new InvalidOperationException("Host not initialized");

    protected override void OnStartup(StartupEventArgs e)
    {
        // Configure services
        _host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                // Register HttpClient
                services.AddHttpClient<CafeManagementService>();

                // Register services
                services.AddSingleton<ISignalRService, SignalRService>();
                services.AddSingleton<ICafeManagementService, CafeManagementService>();
                services.AddSingleton<ISystemService, SystemService>();
                services.AddSingleton<ILockScreenService, LockScreenService>();
                services.AddSingleton<IScreenCaptureService, ScreenCaptureService>();
                services.AddSingleton<IUserSessionService, UserSessionService>();
                services.AddSingleton<ISystemTrayService, SystemTrayService>();

                // Register background services
                services.AddHostedService<UnlockPollingService>();
                services.AddHostedService<ConnectionRetryService>();
                services.AddSingleton<ConnectionRetryService>();

                // Register view models
                services.AddTransient<LockScreenViewModel>();
                services.AddTransient<UserLoginViewModel>();

                // Register views
                services.AddTransient<LockScreenWindow>();
            })
            .ConfigureLogging(logging =>
            {
                logging.AddConsole();
                logging.AddDebug();
            })
            .Build();

        try
        {
            // Start the host
            _host.Start();

            // Create the view model and window
            var viewModel = _host.Services.GetRequiredService<LockScreenViewModel>();
            var systemTrayService = _host.Services.GetRequiredService<ISystemTrayService>();
            var lockScreenWindow = new LockScreenWindow(viewModel, systemTrayService);

            // Start in fullscreen mode
            lockScreenWindow.ShowInTaskbar = false;
            lockScreenWindow.ShowLockScreen();

            // Initialize the view model
            _ = Task.Run(async () => await viewModel.InitializeAsync());

            // Start timer for updating current time
            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            timer.Tick += (s, e) =>
            {
                if (lockScreenWindow.DataContext is LockScreenViewModel vm)
                {
                    vm.UpdateCurrentTime();
                }
            };
            timer.Start();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Application startup failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
        }

        base.OnStartup(e);
    }

    private void HideFromTaskbar(Window window)
    {
        // Get window handle
        var helper = new System.Windows.Interop.WindowInteropHelper(window);
        var hwnd = helper.Handle;

        // Set window extended style to remove from taskbar
        var extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
        SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TOOLWINDOW);
    }

    // Windows API constants and functions
    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_TOOLWINDOW = 0x00000080;

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hwnd, int index);

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

    protected override void OnExit(ExitEventArgs e)
    {
        _host?.Dispose();
        base.OnExit(e);
    }
}

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility visibility)
        {
            return visibility == Visibility.Visible;
        }
        return false;
    }
}

