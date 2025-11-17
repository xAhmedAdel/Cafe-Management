using System.Drawing;
using System.Windows.Forms;
using System.Windows;
using System.Windows.Interop;
using CafeManagement.Client.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;

namespace CafeManagement.Client.Services;

public class SystemTrayService : ISystemTrayService, IDisposable
{
    private readonly ILogger<SystemTrayService> _logger;
    private NotifyIcon? _notifyIcon;
    private Window? _mainWindow;
    private bool _disposed = false;

    // Windows API for showing/hiding windows
    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    private const int SW_RESTORE = 9;

    public SystemTrayService(ILogger<SystemTrayService> logger)
    {
        _logger = logger;
    }

    public void Initialize()
    {
        try
        {
            _notifyIcon = new NotifyIcon
            {
                Icon = new Icon(SystemIcons.Application, 40, 40),
                Text = "Cafe Management Client",
                Visible = true
            };

            // Create context menu
            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Show", null, OnShowClicked);
            contextMenu.Items.Add("-");
            contextMenu.Items.Add("Exit", null, OnExitClicked);

            _notifyIcon.ContextMenuStrip = contextMenu;
            _notifyIcon.DoubleClick += OnNotifyIconDoubleClick;

            _logger.LogInformation("System tray service initialized");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize system tray service");
        }
    }

    public void SetMainWindow(Window window)
    {
        _mainWindow = window;
        UpdateToolTip("Cafe Management - Minimized");
    }

    public void ShowBalloonTip(string title, string text, ToolTipIcon icon)
    {
        try
        {
            if (_notifyIcon != null)
            {
                _notifyIcon.ShowBalloonTip(3000, title, text, icon);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to show balloon tip");
        }
    }

    public void UpdateToolTip(string text)
    {
        try
        {
            if (_notifyIcon != null)
            {
                _notifyIcon.Text = text.Length > 63 ? text.Substring(0, 60) + "..." : text;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update tool tip");
        }
    }

    public void Show()
    {
        try
        {
            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to show system tray icon");
        }
    }

    public void Hide()
    {
        try
        {
            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to hide system tray icon");
        }
    }

    public void ShowMainWindow()
    {
        try
        {
            if (_mainWindow != null)
            {
                var helper = new WindowInteropHelper(_mainWindow);
                ShowWindow(helper.Handle, SW_RESTORE);
                SetForegroundWindow(helper.Handle);

                _mainWindow.WindowState = WindowState.Normal;
                _mainWindow.Show();
                _mainWindow.Activate();

                UpdateToolTip("Cafe Management - Active");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to show main window");
        }
    }

    private void OnNotifyIconDoubleClick(object? sender, EventArgs e)
    {
        ShowMainWindow();
    }

    private void OnShowClicked(object? sender, EventArgs e)
    {
        ShowMainWindow();
    }

    private void OnExitClicked(object? sender, EventArgs e)
    {
        try
        {
            System.Windows.Application.Current.Shutdown();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to shutdown application");
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _notifyIcon?.Dispose();
            _notifyIcon = null;
            _disposed = true;
        }
    }
}