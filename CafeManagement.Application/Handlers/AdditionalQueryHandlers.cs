using AutoMapper;
using CafeManagement.Application.Commands;
using CafeManagement.Application.DTOs;
using CafeManagement.Application.Queries;
using CafeManagement.Core.Entities;
using CafeManagement.Core.Interfaces;
using MediatR;

namespace CafeManagement.Application.Handlers;

public class GetClientByIpAddressQueryHandler : IRequestHandler<GetClientByIpAddressQuery, ClientDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IClientService _clientService;

    public GetClientByIpAddressQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, IClientService clientService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _clientService = clientService;
    }

    public async Task<ClientDto?> Handle(GetClientByIpAddressQuery request, CancellationToken cancellationToken)
    {
        var client = await _clientService.GetClientByIpAddressAsync(request.IpAddress);
        return client != null ? _mapper.Map<ClientDto>(client) : null;
    }
}

public class GetSessionsByClientQueryHandler : IRequestHandler<GetSessionsByClientQuery, IEnumerable<SessionDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetSessionsByClientQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<SessionDto>> Handle(GetSessionsByClientQuery request, CancellationToken cancellationToken)
    {
        var sessions = await _unitOfWork.Sessions.FindAsync(s => s.ClientId == request.ClientId);
        return _mapper.Map<IEnumerable<SessionDto>>(sessions);
    }
}

public class GetSessionsByUserQueryHandler : IRequestHandler<GetSessionsByUserQuery, IEnumerable<SessionDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetSessionsByUserQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<SessionDto>> Handle(GetSessionsByUserQuery request, CancellationToken cancellationToken)
    {
        var sessions = await _unitOfWork.Sessions.FindAsync(s => s.UserId == request.UserId);
        return _mapper.Map<IEnumerable<SessionDto>>(sessions);
    }
}

public class GetSessionsByDateRangeQueryHandler : IRequestHandler<GetSessionsByDateRangeQuery, IEnumerable<SessionDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetSessionsByDateRangeQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<SessionDto>> Handle(GetSessionsByDateRangeQuery request, CancellationToken cancellationToken)
    {
        var sessions = await _unitOfWork.Sessions.FindAsync(s =>
            s.StartTime >= request.StartDate && s.StartTime <= request.EndDate);
        return _mapper.Map<IEnumerable<SessionDto>>(sessions);
    }
}

public class GetSessionSummaryQueryHandler : IRequestHandler<GetSessionSummaryQuery, SessionSummaryDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetSessionSummaryQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<SessionSummaryDto> Handle(GetSessionSummaryQuery request, CancellationToken cancellationToken)
    {
        var allSessions = await _unitOfWork.Sessions.GetAllAsync();
        var activeSessions = allSessions.Where(s => s.Status == Core.Enums.SessionStatus.Active);
        var totalRevenue = allSessions.Sum(s => s.TotalAmount);
        var averageDuration = allSessions.Any() ? allSessions.Average(s => s.DurationMinutes) : 0;

        var clientStatusCounts = new Dictionary<Core.Enums.ClientStatus, int>();

        var clients = await _unitOfWork.Clients.GetAllAsync();
        foreach (var status in Enum.GetValues<Core.Enums.ClientStatus>())
        {
            clientStatusCounts[status] = clients.Count(c => c.Status == status);
        }

        return new SessionSummaryDto
        {
            TotalSessions = allSessions.Count(),
            ActiveSessions = activeSessions.Count(),
            TotalRevenue = totalRevenue,
            AverageSessionDuration = Math.Round((decimal)averageDuration, 2),
            ClientStatusCounts = clientStatusCounts
        };
    }
}

public class UpdateUserBalanceCommandHandler : IRequestHandler<UpdateUserBalanceCommand, UserDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateUserBalanceCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<UserDto> Handle(UpdateUserBalanceCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId);
        if (user == null)
            throw new KeyNotFoundException("User not found");

        user.Balance += request.Amount;
        user.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<UserDto>(user);
    }
}

public class CreateClientCommandHandler : IRequestHandler<CreateClientCommand, ClientDto>
{
    private readonly IClientService _clientService;
    private readonly IMapper _mapper;

    public CreateClientCommandHandler(IClientService clientService, IMapper mapper)
    {
        _clientService = clientService;
        _mapper = mapper;
    }

    public async Task<ClientDto> Handle(CreateClientCommand request, CancellationToken cancellationToken)
    {
        var client = await _clientService.RegisterClientAsync(
            request.ClientDto.Name,
            request.ClientDto.IPAddress,
            request.ClientDto.MACAddress);

        return _mapper.Map<ClientDto>(client);
    }
}

public class UpdateClientStatusCommandHandler : IRequestHandler<UpdateClientStatusCommand, ClientDto>
{
    private readonly IClientService _clientService;
    private readonly IMapper _mapper;

    public UpdateClientStatusCommandHandler(IClientService clientService, IMapper mapper)
    {
        _clientService = clientService;
        _mapper = mapper;
    }

    public async Task<ClientDto> Handle(UpdateClientStatusCommand request, CancellationToken cancellationToken)
    {
        var client = await _clientService.UpdateClientStatusAsync(request.Id, request.StatusDto.Status);
        return _mapper.Map<ClientDto>(client);
    }
}

public class DeleteClientCommandHandler : IRequestHandler<DeleteClientCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteClientCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteClientCommand request, CancellationToken cancellationToken)
    {
        var client = await _unitOfWork.Clients.GetByIdAsync(request.Id);
        if (client == null)
            return false;

        await _unitOfWork.Clients.DeleteAsync(client);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}

public class CreateSessionCommandHandler : IRequestHandler<CreateSessionCommand, SessionDto>
{
    private readonly ISessionService _sessionService;
    private readonly IMapper _mapper;

    public CreateSessionCommandHandler(ISessionService sessionService, IMapper mapper)
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

public class EndSessionCommandHandler : IRequestHandler<EndSessionCommand, SessionDto>
{
    private readonly ISessionService _sessionService;
    private readonly IMapper _mapper;

    public EndSessionCommandHandler(ISessionService sessionService, IMapper mapper)
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

public class ExtendSessionCommandHandler : IRequestHandler<ExtendSessionCommand, SessionDto>
{
    private readonly ISessionService _sessionService;
    private readonly IMapper _mapper;

    public ExtendSessionCommandHandler(ISessionService sessionService, IMapper mapper)
    {
        _sessionService = sessionService;
        _mapper = mapper;
    }

    public async Task<SessionDto> Handle(ExtendSessionCommand request, CancellationToken cancellationToken)
    {
        var session = await _sessionService.ExtendSessionAsync(request.Id, request.ExtendDto.AdditionalMinutes);
        return _mapper.Map<SessionDto>(session);
    }
}

public class UpdateSessionStatusCommandHandler : IRequestHandler<UpdateSessionStatusCommand, SessionDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateSessionStatusCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<SessionDto> Handle(UpdateSessionStatusCommand request, CancellationToken cancellationToken)
    {
        var session = await _unitOfWork.Sessions.GetByIdAsync(request.Id);
        if (session == null)
            throw new KeyNotFoundException("Session not found");

        session.Status = request.Status;
        session.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Sessions.UpdateAsync(session);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<SessionDto>(session);
    }
}