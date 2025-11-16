using System.IO;
using System.IO.Compression;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Win32;
using Microsoft.Win32.TaskScheduler;
using CafeManagement.Client.Services.Interfaces;
using Process = System.Diagnostics.Process;

namespace CafeManagement.Client.Services;

public class DeploymentService : IDeploymentService
{
    private readonly HttpClient _httpClient;
    private readonly string _configFilePath;
    private readonly string _installPath;
    private readonly string _serviceName = "CafeManagementClient";
    private readonly ILogger<DeploymentService> _logger;

    public DeploymentService(ILogger<DeploymentService> logger)
    {
        _httpClient = new HttpClient();
        _logger = logger;
        _configFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CafeManagement", "config.json");
        _installPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? "";
    }

    public async Task<bool> CheckForUpdatesAsync()
    {
        try
        {
            var currentVersion = GetCurrentVersion();
            var serverUrl = GetServerUrl();

            // Check for available updates
            var response = await _httpClient.GetAsync($"{serverUrl}/api/deployment/check-updates?currentVersion={currentVersion}");

            if (response.IsSuccessStatusCode)
            {
                var updateInfo = await response.Content.ReadFromJsonAsync<UpdateInfo>();
                return updateInfo != null && updateInfo.IsUpdateAvailable;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking for updates");
            return false;
        }
    }

    public async Task DownloadUpdateAsync(string updateUrl)
    {
        try
        {
            var tempPath = Path.GetTempFileName();

            _logger.LogInformation($"Downloading update from: {updateUrl}");

            var response = await _httpClient.GetAsync(updateUrl);
            response.EnsureSuccessStatusCode();

            using (var fs = new FileStream(tempPath, FileMode.Create))
            {
                await response.Content.CopyToAsync(fs);
            }

            // Extract update
            await ExtractUpdate(tempPath);

            // Clean up
            File.Delete(tempPath);

            _logger.LogInformation("Update downloaded and extracted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading update");
            throw;
        }
    }

    public async Task InstallUpdateAsync()
    {
        try
        {
            _logger.LogInformation("Starting update installation");

            // Stop current client
            await StopCurrentClient();

            // Run update installer
            var updatePath = Path.Combine(_installPath, "Update", "update.exe");
            if (File.Exists(updatePath))
            {
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = updatePath,
                    Arguments = "/S /D=CafeManagementClient",
                    UseShellExecute = true,
                    Verb = "runas" // Request administrator privileges
                };

                Process.Start(processStartInfo);

                // Exit current application to allow update
                Environment.Exit(0);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error installing update");
            throw;
        }
    }

    public async Task<bool> IsClientConfiguredAsync()
    {
        try
        {
            if (!File.Exists(_configFilePath))
            {
                return false;
            }

            var configJson = await File.ReadAllTextAsync(_configFilePath);
            var config = JsonSerializer.Deserialize<ClientConfiguration>(configJson);

            return config != null && !string.IsNullOrEmpty(config.ServerUrl) && !string.IsNullOrEmpty(config.ClientId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking client configuration");
            return false;
        }
    }

    public async Task ConfigureClientAsync(string serverUrl, string clientId)
    {
        try
        {
            // Ensure config directory exists
            var configDir = Path.GetDirectoryName(_configFilePath);
            if (!Directory.Exists(configDir))
            {
                Directory.CreateDirectory(configDir);
            }

            var config = new ClientConfiguration
            {
                ServerUrl = serverUrl,
                ClientId = clientId,
                InstallDate = DateTime.UtcNow,
                LastUpdate = DateTime.UtcNow,
                AutoStart = true,
                Version = GetCurrentVersion()
            };

            var configJson = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_configFilePath, configJson);

            // Configure Windows startup
            await ConfigureWindowsStartup();

            // Configure Windows service (optional)
            await ConfigureWindowsService();

            _logger.LogInformation($"Client configured successfully: Server={serverUrl}, ClientId={clientId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error configuring client");
            throw;
        }
    }

    public async Task<bool> InstallClientAsync(string serverUrl)
    {
        try
        {
            var clientId = GenerateClientId();

            // Download installer from server
            await DownloadInstaller(serverUrl);

            // Run installer silently
            await RunSilentInstall();

            // Configure client after installation
            await ConfigureClientAsync(serverUrl, clientId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error installing client");
            return false;
        }
    }

    private async Task ConfigureWindowsStartup()
    {
        try
        {
            var appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var appName = Path.GetFileNameWithoutExtension(appPath);

            // Add to Windows startup
            var startupKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
            startupKey?.SetValue(_serviceName, appPath);

            // Create scheduled task for auto-start
            using (var taskService = new TaskService())
            {
                var taskDefinition = TaskService.Instance.NewTaskDefinition();
                taskDefinition.RegistrationInfo.Description = "Cafe Management Client Auto-Start";
                taskDefinition.RegistrationInfo.Author = "CafeManagementSystem";

                // Set trigger to start at logon
                taskDefinition.Triggers.Add(new LogonTrigger());

                // Set action to start the application
                taskDefinition.Actions.Add(new ExecAction(appPath, null, null));

                // Register the task
                taskService.RootFolder.RegisterTaskDefinition("CafeManagementClientStart", taskDefinition);
            }

            _logger.LogInformation("Windows startup configured successfully");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error configuring Windows startup");
        }
    }

    private async Task ConfigureWindowsService()
    {
        try
        {
            var appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;

            // This would require elevated privileges to create a Windows service
            // For now, we'll skip this and rely on startup task
            _logger.LogInformation("Windows service configuration skipped (requires elevated privileges)");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error configuring Windows service");
        }
    }

    private async Task StopCurrentClient()
    {
        try
        {
            // Stop any running instances
            var processes = Process.GetProcessesByName("CafeManagement.Client");
            foreach (var process in processes)
            {
                try
                {
                    process.Kill();
                    process.WaitForExit(5000);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error stopping process: {processId}", process.Id);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error stopping current client");
        }
    }

    private async Task DownloadInstaller(string serverUrl)
    {
        var installerUrl = $"{serverUrl}/api/deployment/installer";
        var installerPath = Path.Combine(Path.GetTempPath(), "cafemanagement-installer.exe");

        var response = await _httpClient.GetAsync(installerUrl);
        response.EnsureSuccessStatusCode();

        using (var fs = new FileStream(installerPath, FileMode.Create))
        {
            await response.Content.CopyToAsync(fs);
        }
    }

    private async Task RunSilentInstall()
    {
        var installerPath = Path.Combine(Path.GetTempPath(), "cafemanagement-installer.exe");

        var processStartInfo = new ProcessStartInfo
        {
            FileName = installerPath,
            Arguments = "/S /D=CafeManagementClient",
            UseShellExecute = true,
            Verb = "runas"
        };

        var process = Process.Start(processStartInfo);
        await process.WaitForExitAsync();
    }

    private async Task ExtractUpdate(string updateFilePath)
    {
        var updateDir = Path.Combine(_installPath, "Update");

        if (Directory.Exists(updateDir))
        {
            Directory.Delete(updateDir, true);
        }

        Directory.CreateDirectory(updateDir);

        ZipFile.ExtractToDirectory(updateFilePath, updateDir);
    }

    private string GetCurrentVersion()
    {
        var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        return version?.ToString() ?? "1.0.0.0";
    }

    private string GetServerUrl()
    {
        try
        {
            if (File.Exists(_configFilePath))
            {
                var configJson = File.ReadAllText(_configFilePath);
                var config = JsonSerializer.Deserialize<ClientConfiguration>(configJson);
                return config?.ServerUrl ?? "http://localhost:5032";
            }
        }
        catch
        {
            // ignored
        }

        return "http://localhost:5032";
    }

    private string GenerateClientId()
    {
        return $"{Environment.MachineName}-{Environment.UserName}-{Guid.NewGuid():N}";
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}

public class ClientConfiguration
{
    public string ServerUrl { get; set; } = "";
    public string ClientId { get; set; } = "";
    public DateTime InstallDate { get; set; }
    public DateTime LastUpdate { get; set; }
    public bool AutoStart { get; set; } = true;
    public string Version { get; set; } = "1.0.0.0";
    public Dictionary<string, object> Settings { get; set; } = new();
}

public class UpdateInfo
{
    public bool IsUpdateAvailable { get; set; }
    public string LatestVersion { get; set; } = "";
    public string UpdateUrl { get; set; } = "";
    public List<string> UpdateNotes { get; set; } = new();
    public DateTime ReleaseDate { get; set; }
    public long UpdateSize { get; set; }
}