using CafeManagement.Core.Enums;

namespace CafeManagement.Application.DTOs;

public class SessionDto
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public int? UserId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int DurationMinutes { get; set; }
    public decimal HourlyRate { get; set; }
    public decimal TotalAmount { get; set; }
    public SessionStatus Status { get; set; }
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ClientDto? Client { get; set; }
    public UserDto? User { get; set; }
}

public class CreateSessionDto
{
    public int ClientId { get; set; }
    public int? UserId { get; set; }
    public int DurationMinutes { get; set; }
    public decimal HourlyRate { get; set; } = 2.00m;
    public string Notes { get; set; } = string.Empty;
}

public class ExtendSessionDto
{
    public int AdditionalMinutes { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public class SessionSummaryDto
{
    public int TotalSessions { get; set; }
    public int ActiveSessions { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageSessionDuration { get; set; }
    public Dictionary<ClientStatus, int> ClientStatusCounts { get; set; } = new();
}