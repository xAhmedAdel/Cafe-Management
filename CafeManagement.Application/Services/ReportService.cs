using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CafeManagement.Application.DTOs;
using CafeManagement.Core.Entities;
using CafeManagement.Core.Interfaces;
using CafeManagement.Core.Enums;

namespace CafeManagement.Application.Services;

public interface IReportService
{
    Task<SystemOverviewDto> GetSystemOverviewAsync();
    Task<UserUsageReportDto> GetUserUsageReportAsync(int userId, DateTime startDate, DateTime endDate);
    Task<List<UserUsageReportDto>> GetAllUsersUsageReportAsync(DateTime startDate, DateTime endDate);
    Task<RevenueReportDto> GetRevenueReportAsync(DateTime startDate, DateTime endDate);
    Task<ClientUsageReportDto> GetClientUsageReportAsync(int clientId, DateTime startDate, DateTime endDate);
    Task<List<ClientUsageReportDto>> GetAllClientsUsageReportAsync(DateTime startDate, DateTime endDate);
    Task<ExportReportDto> ExportUserUsageReportAsync(DateTime startDate, DateTime endDate, string format = "PDF");
    Task<ExportReportDto> ExportRevenueReportAsync(DateTime startDate, DateTime endDate, string format = "PDF");
    Task<byte[]> GeneratePdfReportAsync<T>(IEnumerable<T> data, string reportType) where T : class;
    Task<byte[]> GenerateExcelReportAsync<T>(IEnumerable<T> data, string reportType) where T : class;
}

public class ReportService : IReportService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<ReportService> _logger;

    public ReportService(IApplicationDbContext context, ILogger<ReportService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<SystemOverviewDto> GetSystemOverviewAsync()
    {
        try
        {
            var now = DateTime.UtcNow;
            var todayStart = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);
            var weekStart = todayStart.AddDays(-7);
            var monthStart = todayStart.AddDays(-30);

            var users = await _context.Users.ToListAsync();
            var clients = await _context.Clients.ToListAsync();
            var sessions = await _context.Sessions
                .Include(s => s.User)
                .Include(s => s.Client)
                .ToListAsync();

            var activeUsers = users.Count(u => u.IsActive);
            var activeClients = clients.Count(c => c.Status == ClientStatus.Online);
            var activeSessions = sessions.Count(s => s.Status == SessionStatus.Active);

            // Revenue calculations
            var todayRevenue = sessions
                .Where(s => s.StartTime >= todayStart && s.Status != SessionStatus.Cancelled)
                .Sum(s => s.TotalAmount);

            var weekRevenue = sessions
                .Where(s => s.StartTime >= weekStart && s.Status != SessionStatus.Cancelled)
                .Sum(s => s.TotalAmount);

            var monthRevenue = sessions
                .Where(s => s.StartTime >= monthStart && s.Status != SessionStatus.Cancelled)
                .Sum(s => s.TotalAmount);

            var totalRevenue = sessions
                .Where(s => s.Status != SessionStatus.Cancelled)
                .Sum(s => s.TotalAmount);

            // Average session duration (for completed sessions)
            var completedSessions = sessions.Where(s => s.EndTime.HasValue).ToList();
            var averageSessionDuration = completedSessions.Any()
                ? completedSessions.Average(s => (s.EndTime!.Value - s.StartTime).TotalMinutes)
                : 0;

            // Client status distribution
            var clientStatusGroups = clients
                .GroupBy(c => c.Status)
                .Select(g => new ClientStatusCountDto
                {
                    Status = g.Key.ToString(),
                    Count = g.Count(),
                    Percentage = clients.Any() ? (decimal)g.Count() / clients.Count() * 100 : 0
                })
                .ToList();

            // Peak utilization calculation (simplified - based on active vs total clients)
            var peakUtilizationPercentage = activeClients > 0
                ? (decimal)activeSessions / activeClients * 100
                : 0;

            return new SystemOverviewDto
            {
                TotalUsers = users.Count,
                ActiveUsers = activeUsers,
                TotalClients = clients.Count,
                ActiveClients = activeClients,
                ActiveSessions = activeSessions,
                TodayRevenue = todayRevenue,
                WeekRevenue = weekRevenue,
                MonthRevenue = monthRevenue,
                TotalRevenue = totalRevenue,
                AverageSessionDuration = (decimal)averageSessionDuration,
                PeakUtilizationPercentage = Math.Round(peakUtilizationPercentage, 2),
                ClientStatusDistribution = clientStatusGroups
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating system overview");
            throw;
        }
    }

    public async Task<UserUsageReportDto> GetUserUsageReportAsync(int userId, DateTime startDate, DateTime endDate)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId)
                ?? throw new ArgumentException($"User with ID {userId} not found");

            var sessions = await _context.Sessions
                .Include(s => s.Client)
                .Where(s => s.UserId == userId && s.StartTime >= startDate && s.StartTime <= endDate)
                .OrderByDescending(s => s.StartTime)
                .ToListAsync();

            var totalUsageMinutes = sessions
                .Where(s => s.EndTime.HasValue)
                .Sum(s => (decimal)(s.EndTime!.Value - s.StartTime).TotalMinutes);

            var totalRevenue = sessions
                .Where(s => s.Status != SessionStatus.Cancelled)
                .Sum(s => s.TotalAmount);

            var averageSessionDuration = sessions.Any(s => s.EndTime.HasValue)
                ? sessions.Where(s => s.EndTime.HasValue)
                    .Average(s => (s.EndTime!.Value - s.StartTime).TotalMinutes)
                : 0;

            var recentSessions = sessions
                .Take(10)
                .Select(s => new UserSessionDetailDto
                {
                    SessionId = s.Id,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    DurationMinutes = s.EndTime.HasValue
                        ? (decimal)(s.EndTime!.Value - s.StartTime).TotalMinutes
                        : 0,
                    Cost = s.TotalAmount,
                    ClientName = s.Client?.Name ?? "Unknown",
                    Status = s.Status.ToString()
                })
                .ToList();

            return new UserUsageReportDto
            {
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email ?? "",
                TotalSessions = sessions.Count,
                TotalUsageMinutes = totalUsageMinutes,
                TotalRevenue = totalRevenue,
                AverageSessionDuration = (decimal)averageSessionDuration,
                FirstSessionDate = sessions.Min(s => s.StartTime),
                LastSessionDate = sessions.Max(s => s.StartTime),
                RecentSessions = recentSessions
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating user usage report for user {UserId}", userId);
            throw;
        }
    }

    public async Task<List<UserUsageReportDto>> GetAllUsersUsageReportAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var users = await _context.Users
                .Where(u => u.IsActive)
                .ToListAsync();

            var reports = new List<UserUsageReportDto>();

            foreach (var user in users)
            {
                try
                {
                    var report = await GetUserUsageReportAsync(user.Id, startDate, endDate);
                    reports.Add(report);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not generate report for user {UserId}", user.Id);
                }
            }

            return reports.OrderByDescending(r => r.TotalRevenue).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating all users usage report");
            throw;
        }
    }

    public async Task<RevenueReportDto> GetRevenueReportAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var sessions = await _context.Sessions
                .Include(s => s.User)
                .Include(s => s.Client)
                .Where(s => s.StartTime >= startDate && s.StartTime <= endDate)
                .ToListAsync();

            var totalSessions = sessions.Count;
            var userSessions = sessions.Count(s => s.UserId.HasValue);
            var guestSessions = sessions.Count(s => !s.UserId.HasValue);

            var totalRevenue = sessions
                .Where(s => s.Status != SessionStatus.Cancelled)
                .Sum(s => s.TotalAmount);

            var userRevenue = sessions
                .Where(s => s.Status != SessionStatus.Cancelled && s.UserId.HasValue)
                .Sum(s => s.TotalAmount);

            var guestRevenue = sessions
                .Where(s => s.Status != SessionStatus.Cancelled && !s.UserId.HasValue)
                .Sum(s => s.TotalAmount);

            var averageRevenuePerSession = totalSessions > 0 ? totalRevenue / totalSessions : 0;

            // Daily breakdown
            var dailyBreakdown = sessions
                .Where(s => s.Status != SessionStatus.Cancelled)
                .GroupBy(s => s.StartTime.Date)
                .Select(g => new DailyRevenueDto
                {
                    Date = g.Key,
                    Revenue = g.Sum(s => s.TotalAmount),
                    Sessions = g.Count(),
                    UniqueUsers = g.Count(s => s.UserId.HasValue),
                    AverageSessionDuration = g.Where(s => s.EndTime.HasValue)
                        .Any()
                        ? (decimal)g.Where(s => s.EndTime.HasValue)
                            .Average(s => (s.EndTime!.Value - s.StartTime).TotalMinutes)
                        : 0
                })
                .OrderBy(d => d.Date)
                .ToList();

            // Top users
            var topUsers = sessions
                .Where(s => s.Status != SessionStatus.Cancelled && s.UserId.HasValue)
                .GroupBy(s => s.User!)
                .Select(g => new TopUserDto
                {
                    UserId = g.Key.Id,
                    Username = g.Key.Username,
                    TotalRevenue = g.Sum(s => s.TotalAmount),
                    Sessions = g.Count(),
                    TotalMinutes = g.Where(s => s.EndTime.HasValue)
                        .Sum(s => (decimal)(s.EndTime!.Value - s.StartTime).TotalMinutes)
                })
                .OrderByDescending(u => u.TotalRevenue)
                .Take(10)
                .ToList();

            return new RevenueReportDto
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalRevenue = totalRevenue,
                UserRevenue = userRevenue,
                GuestRevenue = guestRevenue,
                TotalSessions = totalSessions,
                UserSessions = userSessions,
                GuestSessions = guestSessions,
                AverageRevenuePerSession = averageRevenuePerSession,
                DailyBreakdown = dailyBreakdown,
                TopUsers = topUsers
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating revenue report for period {StartDate} to {EndDate}", startDate, endDate);
            throw;
        }
    }

    public async Task<ClientUsageReportDto> GetClientUsageReportAsync(int clientId, DateTime startDate, DateTime endDate)
    {
        try
        {
            var client = await _context.Clients
                .FirstOrDefaultAsync(c => c.Id == clientId)
                ?? throw new ArgumentException($"Client with ID {clientId} not found");

            var sessions = await _context.Sessions
                .Include(s => s.User)
                .Where(s => s.ClientId == clientId && s.StartTime >= startDate && s.StartTime <= endDate)
                .OrderByDescending(s => s.StartTime)
                .ToListAsync();

            var totalUsageMinutes = sessions
                .Where(s => s.EndTime.HasValue)
                .Sum(s => (decimal)(s.EndTime!.Value - s.StartTime).TotalMinutes);

            var totalRevenue = sessions
                .Where(s => s.Status != SessionStatus.Cancelled)
                .Sum(s => s.TotalAmount);

            // Calculate utilization (simplified)
            var totalPeriodMinutes = (decimal)(endDate - startDate).TotalMinutes;
            var utilizationPercentage = totalPeriodMinutes > 0
                ? totalUsageMinutes / totalPeriodMinutes * 100
                : 0;

            var sessionHistory = sessions
                .Take(20)
                .Select(s => new ClientSessionDto
                {
                    SessionId = s.Id,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    DurationMinutes = s.EndTime.HasValue
                        ? (decimal)(s.EndTime!.Value - s.StartTime).TotalMinutes
                        : 0,
                    Cost = s.TotalAmount,
                    Username = s.User?.Username,
                    Status = s.Status.ToString()
                })
                .ToList();

            return new ClientUsageReportDto
            {
                ClientId = client.Id,
                ClientName = client.Name,
                IpAddress = client.IPAddress,
                Status = client.Status.ToString(),
                TotalSessions = sessions.Count,
                TotalUsageMinutes = totalUsageMinutes,
                TotalRevenue = totalRevenue,
                UtilizationPercentage = Math.Round(utilizationPercentage, 2),
                LastActivity = sessions.Max(s => s.EndTime ?? s.StartTime),
                SessionHistory = sessionHistory
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating client usage report for client {ClientId}", clientId);
            throw;
        }
    }

    public async Task<List<ClientUsageReportDto>> GetAllClientsUsageReportAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var clients = await _context.Clients.ToListAsync();
            var reports = new List<ClientUsageReportDto>();

            foreach (var client in clients)
            {
                try
                {
                    var report = await GetClientUsageReportAsync(client.Id, startDate, endDate);
                    reports.Add(report);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not generate report for client {ClientId}", client.Id);
                }
            }

            return reports.OrderByDescending(r => r.TotalRevenue).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating all clients usage report");
            throw;
        }
    }

    public async Task<ExportReportDto> ExportUserUsageReportAsync(DateTime startDate, DateTime endDate, string format = "PDF")
    {
        try
        {
            var reports = await GetAllUsersUsageReportAsync(startDate, endDate);

            return format.ToUpper() switch
            {
                "PDF" => await GeneratePdfExport(reports, $"UserUsageReport_{startDate:yyyy-MM-dd}_to_{endDate:yyyy-MM-dd}"),
                "EXCEL" => await GenerateExcelExport(reports, $"UserUsageReport_{startDate:yyyy-MM-dd}_to_{endDate:yyyy-MM-dd}"),
                _ => throw new ArgumentException($"Unsupported format: {format}")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting user usage report");
            throw;
        }
    }

    public async Task<ExportReportDto> ExportRevenueReportAsync(DateTime startDate, DateTime endDate, string format = "PDF")
    {
        try
        {
            var report = await GetRevenueReportAsync(startDate, endDate);

            return format.ToUpper() switch
            {
                "PDF" => await GeneratePdfExport(new[] { report }, $"RevenueReport_{startDate:yyyy-MM-dd}_to_{endDate:yyyy-MM-dd}"),
                "EXCEL" => await GenerateExcelExport(new[] { report }, $"RevenueReport_{startDate:yyyy-MM-dd}_to_{endDate:yyyy-MM-dd}"),
                _ => throw new ArgumentException($"Unsupported format: {format}")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting revenue report");
            throw;
        }
    }

    public async Task<byte[]> GeneratePdfReportAsync<T>(IEnumerable<T> data, string reportType) where T : class
    {
        // This would require a PDF library like iTextSharp or PdfSharp
        // For now, return a placeholder implementation
        await Task.CompletedTask;
        return System.Text.Encoding.UTF8.GetBytes($"PDF Report: {reportType}\n{System.Text.Json.JsonSerializer.Serialize(data)}");
    }

    public async Task<byte[]> GenerateExcelReportAsync<T>(IEnumerable<T> data, string reportType) where T : class
    {
        // This would require an Excel library like EPPlus or ClosedXML
        // For now, return a placeholder implementation
        await Task.CompletedTask;
        return System.Text.Encoding.UTF8.GetBytes($"Excel Report: {reportType}\n{System.Text.Json.JsonSerializer.Serialize(data)}");
    }

    private async Task<ExportReportDto> GeneratePdfExport<T>(IEnumerable<T> data, string fileName) where T : class
    {
        var pdfData = await GeneratePdfReportAsync(data, fileName);

        return new ExportReportDto
        {
            ReportType = typeof(T).Name,
            Data = pdfData,
            FileName = $"{fileName}.pdf",
            MimeType = "application/pdf"
        };
    }

    private async Task<ExportReportDto> GenerateExcelExport<T>(IEnumerable<T> data, string fileName) where T : class
    {
        var excelData = await GenerateExcelReportAsync(data, fileName);

        return new ExportReportDto
        {
            ReportType = typeof(T).Name,
            Data = excelData,
            FileName = $"{fileName}.xlsx",
            MimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
        };
    }
}