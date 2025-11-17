using CafeManagement.Core.Enums;

namespace CafeManagement.Application.DTOs;

public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Role { get; set; }
    public int AvailableMinutes { get; set; }
    public decimal Balance { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class SessionDto
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public int UserId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int Status { get; set; }
    public int DurationMinutes { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal HourlyRate { get; set; }
    public ClientDto? Client { get; set; }
    public UserDto? User { get; set; }
}

public class UserLoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int ClientId { get; set; }
}

public class UserLoginResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public UserDto? User { get; set; }
    public SessionDto? Session { get; set; }
}

public class CreateUserRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Email { get; set; }
    public int InitialMinutes { get; set; } = 0;
}

public class AddTimeRequest
{
    public int UserId { get; set; }
    public int MinutesToAdd { get; set; }
    public string? Reason { get; set; }
}

// Legacy DTOs for compatibility
public class CreateUserDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public decimal Balance { get; set; } = 0.00m;
}

public class UpdateUserDto
{
    public string? Email { get; set; }
    public decimal? Balance { get; set; }
}

public class UserLoginDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class UserLoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public UserDto User { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
}

public class SessionSummaryDto
{
    public int Id { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int DurationMinutes { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public int TotalSessions { get; set; }
    public int ActiveSessions { get; set; }
    public decimal TotalRevenue { get; set; }
    public double AverageSessionDuration { get; set; }
    public Dictionary<string, int> ClientStatusCounts { get; set; } = new();
}

public class CreateSessionDto
{
    public int ClientId { get; set; }
    public int UserId { get; set; }
    public int DurationMinutes { get; set; }
    public decimal HourlyRate { get; set; }
}

public class ExtendSessionDto
{
    public int AdditionalMinutes { get; set; }
}