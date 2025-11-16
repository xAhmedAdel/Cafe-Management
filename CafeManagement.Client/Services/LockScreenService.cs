using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using CafeManagement.Client.Views;
using CafeManagement.Client.ViewModels;
using CafeManagement.Client.Services.Interfaces;
using System.Windows;

namespace CafeManagement.Client.Services;

public interface ILockScreenService
{
    void ShowLockScreen();
    void HideLockScreen();
    bool IsLocked { get; }
}

public class LockScreenService : ILockScreenService
{
    private readonly ILogger<LockScreenService> _logger;
    private LockScreenWindow? _lockScreenWindow;
    private LockScreenViewModel? _lockScreenViewModel;
    private readonly object _lockObject = new object();

    public bool IsLocked { get; private set; }

    private readonly IServiceProvider _serviceProvider;

    public LockScreenService(ILogger<LockScreenService> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public void ShowLockScreen()
    {
        lock (_lockObject)
        {
            try
            {
                if (IsLocked)
                {
                    _logger.LogDebug("Lock screen is already showing");
                    return;
                }

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    try
                    {
                        // Get services from DI container
                        var cafeService = _serviceProvider.GetService<ICafeManagementService>();
                        var systemService = _serviceProvider.GetService<ISystemService>();
                        var signalRService = _serviceProvider.GetService<ISignalRService>();

                        // Create view model and window
                        _lockScreenViewModel = new LockScreenViewModel(
                            cafeService ?? throw new InvalidOperationException("ICafeManagementService not registered"),
                            systemService ?? throw new InvalidOperationException("ISystemService not registered"),
                            signalRService ?? throw new InvalidOperationException("ISignalRService not registered")
                        );

                        _lockScreenWindow = new LockScreenWindow(_lockScreenViewModel)
                        {
                            WindowState = WindowState.Maximized,
                            WindowStyle = WindowStyle.None,
                            ResizeMode = ResizeMode.NoResize,
                            Topmost = true
                        };

                        _lockScreenWindow.Show();
                        IsLocked = true;

                        _logger.LogInformation("Lock screen displayed successfully");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to show lock screen");
                        IsLocked = false;
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ShowLockScreen");
            }
        }
    }

    public void HideLockScreen()
    {
        lock (_lockObject)
        {
            try
            {
                if (!IsLocked)
                {
                    _logger.LogDebug("Lock screen is not currently showing");
                    return;
                }

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    try
                    {
                        _lockScreenWindow?.Close();
                        _lockScreenWindow = null;
                        _lockScreenViewModel = null;
                        IsLocked = false;

                        _logger.LogInformation("Lock screen hidden successfully");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to hide lock screen");
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in HideLockScreen");
            }
        }
    }
}