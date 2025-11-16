using CafeManagement.Core.Entities;
using CafeManagement.Core.Enums;

namespace CafeManagement.Core.Interfaces;

public interface IClientService
{
    Task<Client> RegisterClientAsync(string name, string ipAddress, string macAddress);
    Task<Client> UpdateClientStatusAsync(int clientId, ClientStatus status);
    Task<Client> GetClientByMacAddressAsync(string macAddress);
    Task<Client> GetClientByIpAddressAsync(string ipAddress);
    Task<Client?> GetClientByIdAsync(int clientId);
    Task<IEnumerable<Client>> GetOnlineClientsAsync();
    Task<bool> IsClientOnlineAsync(int clientId);
}