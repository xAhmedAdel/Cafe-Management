using CafeManagement.Core.Entities;
using CafeManagement.Core.Enums;
using CafeManagement.Core.Interfaces;
using CafeManagement.Core.ValueObjects;

namespace CafeManagement.Infrastructure.Services;

public class SessionService : ISessionService
{
    private readonly IUnitOfWork _unitOfWork;

    public SessionService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Session> StartSessionAsync(int clientId, int? userId, int durationMinutes)
    {
        var client = await _unitOfWork.Clients.GetByIdAsync(clientId);
        if (client == null)
            throw new KeyNotFoundException("Client not found");

        if (client.Status == ClientStatus.InSession)
            throw new InvalidOperationException("Client already has an active session");

        var user = userId.HasValue ? await _unitOfWork.Users.GetByIdAsync(userId.Value) : null;

        var session = new Session
        {
            ClientId = clientId,
            UserId = userId,
            StartTime = DateTime.UtcNow,
            DurationMinutes = durationMinutes,
            HourlyRate = 2.00m,
            Status = SessionStatus.Active,
            TotalAmount = CalculateSessionCost(durationMinutes, 2.00m).Amount
        };

        await _unitOfWork.Sessions.AddAsync(session);

        client.Status = ClientStatus.InSession;
        client.CurrentSessionId = session.Id;
        client.LastSeen = DateTime.UtcNow;

        await _unitOfWork.Clients.UpdateAsync(client);

        var logEntry = new UsageLog
        {
            ClientId = clientId,
            UserId = userId,
            Action = "Session Started",
            Details = $"Session started for {durationMinutes} minutes at ${session.HourlyRate:F2}/hr"
        };
        await _unitOfWork.UsageLogs.AddAsync(logEntry);

        await _unitOfWork.SaveChangesAsync();

        return session;
    }

    public async Task<Session> EndSessionAsync(int sessionId)
    {
        var session = await _unitOfWork.Sessions.GetByIdAsync(sessionId);
        if (session == null)
            throw new KeyNotFoundException("Session not found");

        if (session.Status != SessionStatus.Active)
            throw new InvalidOperationException("Session is not active");

        var client = await _unitOfWork.Clients.GetByIdAsync(session.ClientId);
        if (client == null)
            throw new KeyNotFoundException("Client not found");

        session.EndTime = DateTime.UtcNow;
        session.Status = SessionStatus.Completed;
        session.DurationMinutes = (int)Math.Round((session.EndTime.Value - session.StartTime).TotalMinutes);
        session.TotalAmount = CalculateSessionCost(session.DurationMinutes, session.HourlyRate).Amount;

        client.Status = ClientStatus.Online;
        client.CurrentSessionId = null;
        client.LastSeen = DateTime.UtcNow;

        await _unitOfWork.Clients.UpdateAsync(client);

        var logEntry = new UsageLog
        {
            ClientId = session.ClientId,
            UserId = session.UserId,
            Action = "Session Ended",
            Details = $"Session ended. Duration: {session.DurationMinutes} minutes. Cost: ${session.TotalAmount:F2}"
        };
        await _unitOfWork.UsageLogs.AddAsync(logEntry);

        await _unitOfWork.SaveChangesAsync();

        return session;
    }

    public async Task<Session> ExtendSessionAsync(int sessionId, int additionalMinutes)
    {
        var session = await _unitOfWork.Sessions.GetByIdAsync(sessionId);
        if (session == null)
            throw new KeyNotFoundException("Session not found");

        if (session.Status != SessionStatus.Active)
            throw new InvalidOperationException("Cannot extend non-active session");

        session.DurationMinutes += additionalMinutes;
        session.TotalAmount = CalculateSessionCost(session.DurationMinutes, session.HourlyRate).Amount;
        session.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Sessions.UpdateAsync(session);

        var logEntry = new UsageLog
        {
            ClientId = session.ClientId,
            UserId = session.UserId,
            Action = "Session Extended",
            Details = $"Session extended by {additionalMinutes} minutes. New cost: ${session.TotalAmount:F2}"
        };
        await _unitOfWork.UsageLogs.AddAsync(logEntry);

        await _unitOfWork.SaveChangesAsync();

        return session;
    }

    public async Task<Session?> GetActiveSessionAsync(int clientId)
    {
        var sessions = await _unitOfWork.Sessions.FindAsync(s =>
            s.ClientId == clientId &&
            s.Status == SessionStatus.Active);

        return sessions.FirstOrDefault();
    }

    public async Task<decimal> CalculateSessionCostAsync(int sessionId)
    {
        var session = await _unitOfWork.Sessions.GetByIdAsync(sessionId);
        if (session == null)
            throw new KeyNotFoundException("Session not found");

        if (session.EndTime.HasValue)
        {
            var actualDuration = (int)Math.Round((session.EndTime.Value - session.StartTime).TotalMinutes);
            return CalculateSessionCost(actualDuration, session.HourlyRate).Amount;
        }

        return CalculateSessionCost(session.DurationMinutes, session.HourlyRate).Amount;
    }

    private Money CalculateSessionCost(int durationMinutes, decimal hourlyRate)
    {
        var hours = durationMinutes / 60.0m;
        var cost = hours * hourlyRate;
        return Money.FromDecimal(Math.Round(cost, 2));
    }
}