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

                // Register background services
                services.AddHostedService<UnlockPollingService>();

                // Register view models
                services.AddTransient<LockScreenViewModel>();

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
            var lockScreenWindow = new LockScreenWindow(viewModel);
            lockScreenWindow.Show();

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
            MessageBox.Show($"Application startup failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
        }

        base.OnStartup(e);
    }

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

