using CafeManagement.Core.Enums;

namespace CafeManagement.Application.DTOs;

public class ClientDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string IPAddress { get; set; } = string.Empty;
    public string MACAddress { get; set; } = string.Empty;
    public ClientStatus Status { get; set; }
    public string Configuration { get; set; } = string.Empty;
    public DateTime? LastSeen { get; set; }
    public int? CurrentSessionId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsOnline { get; set; }
}

public class CreateClientDto
{
    public string Name { get; set; } = string.Empty;
    public string IPAddress { get; set; } = string.Empty;
    public string MACAddress { get; set; } = string.Empty;
    public string Configuration { get; set; } = string.Empty;
}

public class UpdateClientStatusDto
{
    public ClientStatus Status { get; set; }
}

public class ClientStatusUpdateDto
{
    public int ClientId { get; set; }
    public ClientStatus Status { get; set; }
    public string? Message { get; set; }
}