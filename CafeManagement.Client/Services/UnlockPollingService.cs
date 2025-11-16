using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Net.Http;
using System.Net.Http.Json;
using CafeManagement.Client.Services.Interfaces;

namespace CafeManagement.Client.Services;

public class UnlockPollingService : BackgroundService
{
    private readonly ILogger<UnlockPollingService> _logger;
    private readonly IConfiguration _configuration;
    private readonly ILockScreenService _lockScreenService;
    private readonly HttpClient _httpClient;
    private readonly int _clientId;
    private readonly int _pollingIntervalSeconds = 2; // Check every 2 seconds

    public UnlockPollingService(
        ILogger<UnlockPollingService> logger,
        IConfiguration configuration,
        ILockScreenService lockScreenService)
    {
        _logger = logger;
        _configuration = configuration;
        _lockScreenService = lockScreenService;
        _httpClient = new HttpClient();

        // For demo purposes, use client ID 1. In a real implementation,
        // this should come from configuration or be unique per machine
        _clientId = int.TryParse(_configuration["ClientSettings:ClientId"], out var id) ? id : 1;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation($"Unlock polling service started for client {_clientId}");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckUnlockCommand();
                await Task.Delay(TimeSpan.FromSeconds(_pollingIntervalSeconds), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Unlock polling service stopping");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in unlock polling service");
                await Task.Delay(TimeSpan.FromSeconds(_pollingIntervalSeconds), stoppingToken);
            }
        }
    }

    private async Task CheckUnlockCommand()
    {
        try
        {
            var serverUrl = _configuration["ServerSettings:BaseUrl"] ?? "http://localhost:5032";
            var response = await _httpClient.GetAsync($"{serverUrl}/api/admin/clients/{_clientId}/should-unlock");

            if (response.IsSuccessStatusCode)
            {
                var shouldUnlock = await response.Content.ReadFromJsonAsync<bool>();
                if (shouldUnlock && _lockScreenService.IsLocked)
                {
                    _logger.LogInformation("Unlock command received via polling");
                    _lockScreenService.HideLockScreen();
                }
            }
            else
            {
                _logger.LogWarning($"Failed to check unlock status: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking unlock command");
        }
    }

    public override void Dispose()
    {
        _httpClient?.Dispose();
        base.Dispose();
    }
}