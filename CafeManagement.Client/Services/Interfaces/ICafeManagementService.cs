using CafeManagement.Application.DTOs;

namespace CafeManagement.Client.Services.Interfaces;

public interface ICafeManagementService
{
    Task<bool> RegisterClientAsync();
    Task<bool> RequestUnlockAsync();
    Task AdminUnlockAsync();
    Task<SessionDto?> GetCurrentSessionAsync();
    Task<SessionDto?> GetActiveSessionAsync(int clientId);
    Task NotifyHeartbeatAsync();
    Task<ClientDto?> GetCurrentClientAsync();
    Task<SessionDto?> EndSessionAsync(int sessionId);
}

public interface ISignalRService
{
    Task ConnectAsync();
    Task DisconnectAsync();
    Task NotifyUnlockRequested();
    Task NotifySessionStarted(SessionDto session);
    Task NotifySessionEnded(SessionDto session);
    Task NotifyClientStatusUpdated(int clientId, int status);
    Task SendScreenshot(byte[] imageData);
    Task NotifyRemoteControlStarted();
    Task NotifyRemoteControlStopped();
    Task SendRemoteCommand(string command, params object[] parameters);
}

public interface ISystemService
{
    string GetLocalIpAddress();
    string GetMacAddress();
    Task ExitApplication();
    Task RestartApplication();
    Task ShutdownComputer();
    Task LockWorkstation();
    Task UnlockWorkstation();
}


public interface IRemoteControlService
{
    Task SendScreenshotAsync(byte[] imageData);
    Task HandleRemoteCommandAsync(string command, object[] parameters);
    Task StartRemoteControlSessionAsync();
    Task StopRemoteControlSessionAsync();
}

public interface IDeploymentService
{
    Task<bool> CheckForUpdatesAsync();
    Task DownloadUpdateAsync(string updateUrl);
    Task InstallUpdateAsync();
    Task<bool> IsClientConfiguredAsync();
    Task ConfigureClientAsync(string serverUrl, string clientId);
}