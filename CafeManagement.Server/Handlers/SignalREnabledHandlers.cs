using AutoMapper;
using CafeManagement.Application.Commands;
using CafeManagement.Application.DTOs;
using CafeManagement.Application.Queries;
using CafeManagement.Core.Entities;
using CafeManagement.Core.Enums;
using CafeManagement.Core.Interfaces;
using CafeManagement.Server.Services;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace CafeManagement.Server.Handlers;

public class SignalREnabledSessionService : ISessionService
{
    private readonly ISessionService _innerService;
    private readonly INotificationService _notificationService;
    private readonly IMapper _mapper;

    public SignalREnabledSessionService(ISessionService innerService, INotificationService notificationService, IMapper mapper)
    {
        _innerService = innerService;
        _notificationService = notificationService;
        _mapper = mapper;
    }

    public async Task<Session> StartSessionAsync(int clientId, int? userId, int durationMinutes)
    {
        var session = await _innerService.StartSessionAsync(clientId, userId, durationMinutes);
        var sessionDto = _mapper.Map<SessionDto>(session);
        await _notificationService.NotifySessionStarted(sessionDto);
        return session;
    }

    public async Task<Session> EndSessionAsync(int sessionId)
    {
        var session = await _innerService.EndSessionAsync(sessionId);
        var sessionDto = _mapper.Map<SessionDto>(session);
        await _notificationService.NotifySessionEnded(sessionDto);
        return session;
    }

    public async Task<Session> ExtendSessionAsync(int sessionId, int additionalMinutes)
    {
        var session = await _innerService.ExtendSessionAsync(sessionId, additionalMinutes);
        var sessionDto = _mapper.Map<SessionDto>(session);
        await _notificationService.NotifySessionExtended(sessionDto);
        return session;
    }

    public async Task<Session?> GetActiveSessionAsync(int clientId)
    {
        return await _innerService.GetActiveSessionAsync(clientId);
    }

    public async Task<decimal> CalculateSessionCostAsync(int sessionId)
    {
        return await _innerService.CalculateSessionCostAsync(sessionId);
    }
}

public class SignalREnabledClientService : IClientService
{
    private readonly IClientService _innerService;
    private readonly INotificationService _notificationService;
    private readonly IMapper _mapper;

    public SignalREnabledClientService(IClientService innerService, INotificationService notificationService, IMapper mapper)
    {
        _innerService = innerService;
        _notificationService = notificationService;
        _mapper = mapper;
    }

    public async Task<Client> RegisterClientAsync(string name, string ipAddress, string macAddress)
    {
        var client = await _innerService.RegisterClientAsync(name, ipAddress, macAddress);
        var clientDto = _mapper.Map<ClientDto>(client);
        await _notificationService.NotifyClientStatusUpdate(clientDto);
        return client;
    }

    public async Task<Client> UpdateClientStatusAsync(int clientId, ClientStatus status)
    {
        var client = await _innerService.UpdateClientStatusAsync(clientId, status);
        var clientDto = _mapper.Map<ClientDto>(client);
        await _notificationService.NotifyClientStatusUpdate(clientDto);
        return client;
    }

    public async Task<Client> GetClientByMacAddressAsync(string macAddress)
    {
        return await _innerService.GetClientByMacAddressAsync(macAddress);
    }

    public async Task<Client> GetClientByIpAddressAsync(string ipAddress)
    {
        return await _innerService.GetClientByIpAddressAsync(ipAddress);
    }

    public async Task<IEnumerable<Client>> GetOnlineClientsAsync()
    {
        return await _innerService.GetOnlineClientsAsync();
    }

    public async Task<bool> IsClientOnlineAsync(int clientId)
    {
        return await _innerService.IsClientOnlineAsync(clientId);
    }
}

public class SignalRCreateSessionCommandHandler : IRequestHandler<CreateSessionCommand, SessionDto>
{
    private readonly ISessionService _sessionService;
    private readonly IMapper _mapper;

    public SignalRCreateSessionCommandHandler(ISessionService sessionService, IMapper mapper)
    {
        _sessionService = sessionService;
        _mapper = mapper;
    }

    public async Task<SessionDto> Handle(CreateSessionCommand request, CancellationToken cancellationToken)
    {
        var session = await _sessionService.StartSessionAsync(
            request.SessionDto.ClientId,
            request.SessionDto.UserId,
            request.SessionDto.DurationMinutes);

        return _mapper.Map<SessionDto>(session);
    }
}

public class SignalREndSessionCommandHandler : IRequestHandler<EndSessionCommand, SessionDto>
{
    private readonly ISessionService _sessionService;
    private readonly IMapper _mapper;

    public SignalREndSessionCommandHandler(ISessionService sessionService, IMapper mapper)
    {
        _sessionService = sessionService;
        _mapper = mapper;
    }

    public async Task<SessionDto> Handle(EndSessionCommand request, CancellationToken cancellationToken)
    {
        var session = await _sessionService.EndSessionAsync(request.Id);
        return _mapper.Map<SessionDto>(session);
    }
}