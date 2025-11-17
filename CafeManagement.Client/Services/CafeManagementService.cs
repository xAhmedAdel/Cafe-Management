using CafeManagement.Client.Services.Interfaces;
using CafeManagement.Application.DTOs;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace CafeManagement.Client.Services;

public class CafeManagementService : ICafeManagementService
{
    private readonly HttpClient _httpClient;
    private readonly string _serverUrl;

    public CafeManagementService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _serverUrl = configuration["ServerSettings:BaseUrl"] ?? "http://localhost:5032";
    }

    public async Task<bool> RegisterClientAsync()
    {
        try
        {
            var clientInfo = new
            {
                Name = Environment.MachineName,
                IpAddress = GetLocalIpAddress(),
                MacAddress = GetMacAddress()
            };

            var response = await _httpClient.PostAsJsonAsync($"{_serverUrl}/api/clients", clientInfo);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> RequestUnlockAsync()
    {
        try
        {
            var response = await _httpClient.PostAsync($"{_serverUrl}/api/sessions/request-unlock", null);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task AdminUnlockAsync()
    {
        // This would typically require admin authentication
        // For now, we'll just make a simple call
        try
        {
            await _httpClient.PostAsync($"{_serverUrl}/api/auth/admin-unlock", null);
        }
        catch
        {
            // Handle exception
        }
    }

    public async Task<SessionDto?> GetCurrentSessionAsync()
    {
        try
        {
            var clientId = await GetClientId();
            var response = await _httpClient.GetAsync($"{_serverUrl}/api/sessions/active/{clientId}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<SessionDto>();
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<SessionDto?> GetActiveSessionAsync(int clientId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_serverUrl}/api/sessions/active/{clientId}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<SessionDto>();
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    public async Task NotifyHeartbeatAsync()
    {
        try
        {
            var clientId = await GetClientId();
            await _httpClient.PostAsync($"{_serverUrl}/api/clients/{clientId}/heartbeat", null);
        }
        catch
        {
            // Handle exception silently for heartbeat
        }
    }

    public async Task<ClientDto?> GetCurrentClientAsync()
    {
        try
        {
            var macAddress = GetMacAddress();
            var response = await _httpClient.GetAsync($"{_serverUrl}/api/clients/mac/{macAddress}");
            if (response.IsSuccessStatusCode)
            {
                var client = await response.Content.ReadFromJsonAsync<ClientDto>();
                return client;
            }
            return null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<SessionDto?> EndSessionAsync(int sessionId)
    {
        try
        {
            var response = await _httpClient.PostAsync($"{_serverUrl}/api/sessions/{sessionId}/end", null);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<SessionDto>();
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    private async Task<int> GetClientId()
    {
        try
        {
            var clientInfo = new
            {
                Name = Environment.MachineName,
                IpAddress = GetLocalIpAddress(),
                MacAddress = GetMacAddress()
            };

            var response = await _httpClient.PostAsJsonAsync($"{_serverUrl}/api/clients", clientInfo);
            if (response.IsSuccessStatusCode)
            {
                var clientDto = await response.Content.ReadFromJsonAsync<ClientDto>();
                return clientDto?.Id ?? 0;
            }

            return 0;
        }
        catch
        {
            return 0;
        }
    }

    private string GetLocalIpAddress()
    {
        try
        {
            using var socket = new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Dgram, 0);
            socket.Connect("8.8.8.8", 65530);
            var endPoint = socket.LocalEndPoint as System.Net.IPEndPoint;
            return endPoint?.Address.ToString() ?? "127.0.0.1";
        }
        catch
        {
            return "127.0.0.1";
        }
    }

    private string GetMacAddress()
    {
        try
        {
            var nics = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
            var firstNic = nics.FirstOrDefault(nic => nic.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up && nic.NetworkInterfaceType != System.Net.NetworkInformation.NetworkInterfaceType.Loopback);
            return firstNic?.GetPhysicalAddress().ToString() ?? "00-00-00-00-00-00";
        }
        catch
        {
            return "00-00-00-00-00-00";
        }
    }
}