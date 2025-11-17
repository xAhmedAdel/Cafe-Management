using CafeManagement.Core.Entities;
using CafeManagement.Core.Enums;
using CafeManagement.Core.Interfaces;

namespace CafeManagement.Infrastructure.Services;

public class ClientService : IClientService
{
    private readonly IUnitOfWork _unitOfWork;

    public ClientService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Client> RegisterClientAsync(string name, string ipAddress, string macAddress)
    {
        var existingClient = await GetClientByMacAddressAsync(macAddress);
        if (existingClient != null)
        {
            existingClient.IPAddress = ipAddress;
            existingClient.Name = name;
            existingClient.UpdatedAt = DateTime.UtcNow;
            // Ensure CurrentSessionId is never 0 - set to null instead
            if (existingClient.CurrentSessionId == 0)
            {
                existingClient.CurrentSessionId = null;
            }
            await _unitOfWork.Clients.UpdateAsync(existingClient);

            var reconnectionLog = new UsageLog
            {
                ClientId = existingClient.Id,
                Action = "Client Reconnected",
                Details = $"Client reconnected with new IP: {ipAddress}"
            };
            await _unitOfWork.UsageLogs.AddAsync(reconnectionLog);
            await _unitOfWork.SaveChangesAsync();

            return existingClient;
        }

        var client = new Client
        {
            Name = name,
            IPAddress = ipAddress,
            MACAddress = macAddress,
            Status = ClientStatus.Online
        };

        await _unitOfWork.Clients.AddAsync(client);
        await _unitOfWork.SaveChangesAsync(); // Save client first to get ID

        var lockScreenConfig = new LockScreenConfig
        {
            ClientId = client.Id, // Now client.Id has a value
            BackgroundColor = "#000000",
            TextColor = "#FFFFFF",
            ShowTimeRemaining = true,
            Message = "Welcome to Cafe Management System"
        };

        await _unitOfWork.LockScreenConfigs.AddAsync(lockScreenConfig);

        var logEntry = new UsageLog
        {
            ClientId = client.Id, // Now client.Id has a value
            Action = "Client Registered",
            Details = $"New client registered: {name} ({ipAddress})"
        };
        await _unitOfWork.UsageLogs.AddAsync(logEntry);

        await _unitOfWork.SaveChangesAsync();

        return client;
    }

    public async Task<Client> UpdateClientStatusAsync(int clientId, ClientStatus status)
    {
        var client = await _unitOfWork.Clients.GetByIdAsync(clientId);
        if (client == null)
            throw new KeyNotFoundException("Client not found");

        var oldStatus = client.Status;
        client.Status = status;
        client.LastSeen = DateTime.UtcNow;
        client.UpdatedAt = DateTime.UtcNow;

        // Ensure CurrentSessionId is never 0 - set to null instead
        if (client.CurrentSessionId == 0)
        {
            client.CurrentSessionId = null;
        }

        await _unitOfWork.Clients.UpdateAsync(client);

        var logEntry = new UsageLog
        {
            ClientId = clientId,
            Action = "Status Changed",
            Details = $"Client status changed from {oldStatus} to {status}"
        };
        await _unitOfWork.UsageLogs.AddAsync(logEntry);

        await _unitOfWork.SaveChangesAsync();

        return client;
    }

    public async Task<Client> GetClientByMacAddressAsync(string macAddress)
    {
        var clients = await _unitOfWork.Clients.FindAsync(c => c.MACAddress == macAddress);
        return clients.FirstOrDefault();
    }

    public async Task<Client> GetClientByIpAddressAsync(string ipAddress)
    {
        var clients = await _unitOfWork.Clients.FindAsync(c => c.IPAddress == ipAddress);
        return clients.FirstOrDefault();
    }

    public async Task<IEnumerable<Client>> GetOnlineClientsAsync()
    {
        var onlineStatuses = new[] { ClientStatus.Online, ClientStatus.InSession, ClientStatus.Locked };
        return await _unitOfWork.Clients.FindAsync(c => onlineStatuses.Contains(c.Status));
    }

    public async Task<bool> IsClientOnlineAsync(int clientId)
    {
        var client = await _unitOfWork.Clients.GetByIdAsync(clientId);
        if (client == null)
            return false;

        var onlineStatuses = new[] { ClientStatus.Online, ClientStatus.InSession, ClientStatus.Locked };
        return onlineStatuses.Contains(client.Status);
    }

    public async Task<Client?> GetClientByIdAsync(int clientId)
    {
        return await _unitOfWork.Clients.GetByIdAsync(clientId);
    }
}