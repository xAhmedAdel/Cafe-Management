using System.ComponentModel.DataAnnotations;

namespace CafeManagement.Application.DTOs;

public class UserUsageReportDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = "";
    public string Email { get; set; } = "";
    public int TotalSessions { get; set; }
    public decimal TotalUsageMinutes { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageSessionDuration { get; set; }
    public DateTime FirstSessionDate { get; set; }
    public DateTime LastSessionDate { get; set; }
    public List<UserSessionDetailDto> RecentSessions { get; set; } = new();
}

public class UserSessionDetailDto
{
    public int SessionId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public decimal DurationMinutes { get; set; }
    public decimal Cost { get; set; }
    public string ClientName { get; set; } = "";
    public string Status { get; set; } = "";
}

public class RevenueReportDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal UserRevenue { get; set; }
    public decimal GuestRevenue { get; set; }
    public int TotalSessions { get; set; }
    public int UserSessions { get; set; }
    public int GuestSessions { get; set; }
    public decimal AverageRevenuePerSession { get; set; }
    public List<DailyRevenueDto> DailyBreakdown { get; set; } = new();
    public List<TopUserDto> TopUsers { get; set; } = new();
}

public class DailyRevenueDto
{
    public DateTime Date { get; set; }
    public decimal Revenue { get; set; }
    public int Sessions { get; set; }
    public int UniqueUsers { get; set; }
    public decimal AverageSessionDuration { get; set; }
}

public class TopUserDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = "";
    public decimal TotalRevenue { get; set; }
    public int Sessions { get; set; }
    public decimal TotalMinutes { get; set; }
}

public class ClientUsageReportDto
{
    public int ClientId { get; set; }
    public string ClientName { get; set; } = "";
    public string IpAddress { get; set; } = "";
    public string Status { get; set; } = "";
    public int TotalSessions { get; set; }
    public decimal TotalUsageMinutes { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal UtilizationPercentage { get; set; }
    public DateTime? LastActivity { get; set; }
    public List<ClientSessionDto> SessionHistory { get; set; } = new();
}

public class ClientSessionDto
{
    public int SessionId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public decimal DurationMinutes { get; set; }
    public decimal Cost { get; set; }
    public string? Username { get; set; }
    public string Status { get; set; } = "";
}

public class SystemOverviewDto
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int TotalClients { get; set; }
    public int ActiveClients { get; set; }
    public int ActiveSessions { get; set; }
    public decimal TodayRevenue { get; set; }
    public decimal WeekRevenue { get; set; }
    public decimal MonthRevenue { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageSessionDuration { get; set; }
    public decimal PeakUtilizationPercentage { get; set; }
    public List<ClientStatusCountDto> ClientStatusDistribution { get; set; } = new();
}

public class ClientStatusCountDto
{
    public string Status { get; set; } = "";
    public int Count { get; set; }
    public decimal Percentage { get; set; }
}

public class ReportParametersDto
{
    [Required]
    public DateTime StartDate { get; set; }
    [Required]
    public DateTime EndDate { get; set; }
    public int? UserId { get; set; }
    public int? ClientId { get; set; }
    public string? GroupBy { get; set; }
    public bool IncludeGuestSessions { get; set; } = true;
    public bool IncludeUserSessions { get; set; } = true;
    public int? TopCount { get; set; } = 10;
    public string Format { get; set; } = "PDF";
}

public class ExportReportDto
{
    public string ReportType { get; set; } = "";
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Format { get; set; } = "PDF";
    public byte[] Data { get; set; } = Array.Empty<byte>();
    public string FileName { get; set; } = "";
    public string MimeType { get; set; } = "";
}