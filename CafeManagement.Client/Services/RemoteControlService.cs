using System.IO;
using System.Windows;
using System.Windows.Input;
using CafeManagement.Client.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;

namespace CafeManagement.Client.Services;

public class RemoteControlService : IRemoteControlService
{
    private readonly IScreenCaptureService _screenCaptureService;
    private readonly ISignalRService _signalRService;
    private readonly ILogger<RemoteControlService> _logger;
    private readonly Timer _screenshotTimer;
    private readonly object _captureLock = new object();
    private bool _isRemoteControlActive = false;
    private CancellationTokenSource? _cancellationTokenSource;

    private static class NativeMethods
    {
        [DllImport("user32.dll")]
        public static extern void SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);

        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        public const uint MOUSEEVENTF_LEFTDOWN = 0x02;
        public const uint MOUSEEVENTF_LEFTUP = 0x04;
        public const uint MOUSEEVENTF_RIGHTDOWN = 0x08;
        public const uint MOUSEEVENTF_RIGHTUP = 0x10;
        public const uint MOUSEEVENTF_MIDDLEDOWN = 0x20;
        public const uint MOUSEEVENTF_MIDDLEUP = 0x40;
        public const uint MOUSEEVENTF_MOVE = 0x01;

        public const uint KEYEVENTF_KEYUP = 0x02;
    }

    public RemoteControlService(
        IScreenCaptureService screenCaptureService,
        ISignalRService signalRService,
        ILogger<RemoteControlService> logger)
    {
        _screenCaptureService = screenCaptureService;
        _signalRService = signalRService;
        _logger = logger;

        _screenshotTimer = new Timer(CaptureScreenshotCallback, null, Timeout.Infinite, Timeout.Infinite);
    }

    public async Task SendScreenshotAsync(byte[] imageData)
    {
        try
        {
            // Compress the image to reduce bandwidth
            var compressedData = _screenCaptureService.CompressImage(imageData, 50);

            await _signalRService.SendScreenshot(compressedData);

            _logger.LogDebug($"Screenshot sent: {compressedData.Length} bytes");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending screenshot");
        }
    }

    public async Task HandleRemoteCommandAsync(string command, object[] parameters)
    {
        try
        {
            _logger.LogDebug($"Remote command received: {command}");

            switch (command.ToLower())
            {
                case "mousemove":
                    if (parameters.Length >= 2 && int.TryParse(parameters[0].ToString(), out int x) && int.TryParse(parameters[1].ToString(), out int y))
                    {
                        SetCursorPosition(x, y);
                    }
                    break;

                case "mousedown":
                    if (parameters.Length >= 1 && Enum.TryParse<MouseButton>(parameters[0].ToString(), out var mouseButton))
                    {
                        SimulateMouseDown(mouseButton);
                    }
                    break;

                case "mouseup":
                    if (parameters.Length >= 1 && Enum.TryParse<MouseButton>(parameters[0].ToString(), out mouseButton))
                    {
                        SimulateMouseUp(mouseButton);
                    }
                    break;

                case "click":
                    if (parameters.Length >= 1 && Enum.TryParse<MouseButton>(parameters[0].ToString(), out mouseButton))
                    {
                        SimulateMouseClick(mouseButton);
                    }
                    break;

                case "keydown":
                    if (parameters.Length >= 1 && int.TryParse(parameters[0].ToString(), out int keyCode))
                    {
                        SimulateKeyDown((byte)keyCode);
                    }
                    break;

                case "keyup":
                    if (parameters.Length >= 1 && int.TryParse(parameters[0].ToString(), out keyCode))
                    {
                        SimulateKeyUp((byte)keyCode);
                    }
                    break;

                case "keypress":
                    if (parameters.Length >= 1 && int.TryParse(parameters[0].ToString(), out keyCode))
                    {
                        SimulateKeyPress((byte)keyCode);
                    }
                    break;

                case "textinput":
                    if (parameters.Length >= 1)
                    {
                        SimulateTextInput(parameters[0].ToString());
                    }
                    break;

                default:
                    _logger.LogWarning($"Unknown remote command: {command}");
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error handling remote command: {command}");
        }
    }

    public async Task StartRemoteControlSessionAsync()
    {
        if (_isRemoteControlActive)
        {
            _logger.LogWarning("Remote control session already active");
            return;
        }

        _isRemoteControlActive = true;
        _cancellationTokenSource = new CancellationTokenSource();

        _logger.LogInformation("Starting remote control session");

        // Start sending screenshots every 100ms (10 FPS)
        _screenshotTimer.Change(TimeSpan.Zero, TimeSpan.FromMilliseconds(100));

        // Notify server that remote control session started
        await _signalRService.NotifyRemoteControlStarted();
    }

    public async Task StopRemoteControlSessionAsync()
    {
        if (!_isRemoteControlActive)
        {
            _logger.LogWarning("No remote control session to stop");
            return;
        }

        _isRemoteControlActive = false;
        _cancellationTokenSource?.Cancel();

        _logger.LogInformation("Stopping remote control session");

        // Stop sending screenshots
        _screenshotTimer.Change(Timeout.Infinite, Timeout.Infinite);

        // Notify server that remote control session stopped
        await _signalRService.NotifyRemoteControlStopped();
    }

    private async void CaptureScreenshotCallback(object? state)
    {
        if (!_isRemoteControlActive)
        {
            return;
        }

        try
        {
            lock (_captureLock)
            {
                var imageData = _screenCaptureService.CaptureScreenAsync().GetAwaiter().GetResult();
                if (imageData.Length > 0)
                {
                    _ = SendScreenshotAsync(imageData);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during screenshot capture");
        }
    }

    private void SetCursorPosition(int x, int y)
    {
        NativeMethods.SetCursorPos(x, y);
    }

    private void SimulateMouseDown(MouseButton button)
    {
        switch (button)
        {
            case MouseButton.Left:
                NativeMethods.mouse_event(NativeMethods.MOUSEEVENTF_LEFTDOWN, 0, 0, 0, UIntPtr.Zero);
                break;
            case MouseButton.Right:
                NativeMethods.mouse_event(NativeMethods.MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, UIntPtr.Zero);
                break;
            case MouseButton.Middle:
                NativeMethods.mouse_event(NativeMethods.MOUSEEVENTF_MIDDLEDOWN, 0, 0, 0, UIntPtr.Zero);
                break;
        }
    }

    private void SimulateMouseUp(MouseButton button)
    {
        switch (button)
        {
            case MouseButton.Left:
                NativeMethods.mouse_event(NativeMethods.MOUSEEVENTF_LEFTUP, 0, 0, 0, UIntPtr.Zero);
                break;
            case MouseButton.Right:
                NativeMethods.mouse_event(NativeMethods.MOUSEEVENTF_RIGHTUP, 0, 0, 0, UIntPtr.Zero);
                break;
            case MouseButton.Middle:
                NativeMethods.mouse_event(NativeMethods.MOUSEEVENTF_MIDDLEUP, 0, 0, 0, UIntPtr.Zero);
                break;
        }
    }

    private void SimulateMouseClick(MouseButton button)
    {
        SimulateMouseDown(button);
        Thread.Sleep(50); // Small delay between down and up
        SimulateMouseUp(button);
    }

    private void SimulateKeyDown(byte keyCode)
    {
        NativeMethods.keybd_event(keyCode, 0, 0, UIntPtr.Zero);
    }

    private void SimulateKeyUp(byte keyCode)
    {
        NativeMethods.keybd_event(keyCode, 0, NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);
    }

    private void SimulateKeyPress(byte keyCode)
    {
        SimulateKeyDown(keyCode);
        Thread.Sleep(50);
        SimulateKeyUp(keyCode);
    }

    private void SimulateTextInput(string text)
    {
        foreach (char c in text)
        {
            var vk = VkKeyScan(c);
            if (vk != -1)
            {
                SimulateKeyPress((byte)(vk & 0xFF));
            }
        }
    }

    [DllImport("user32.dll")]
    private static extern short VkKeyScan(char ch);

    public bool IsRemoteControlActive => _isRemoteControlActive;

    public void Dispose()
    {
        _screenshotTimer?.Dispose();
        _cancellationTokenSource?.Dispose();
    }
}

public enum MouseButton
{
    Left,
    Right,
    Middle
}