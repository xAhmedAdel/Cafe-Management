using AutoMapper;
using CafeManagement.Application.DTOs;
using CafeManagement.Application.Queries;
using CafeManagement.Core.Entities;
using CafeManagement.Core.Enums;
using CafeManagement.Core.Interfaces;
using MediatR;

namespace CafeManagement.Application.Handlers;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetUserByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<UserDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(request.Id);
        return user != null ? _mapper.Map<UserDto>(user) : null;
    }
}

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, IEnumerable<UserDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllUsersQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _unitOfWork.Users.GetAllAsync();
        return _mapper.Map<IEnumerable<UserDto>>(users);
    }
}

public class GetUsersByRoleQueryHandler : IRequestHandler<GetUsersByRoleQuery, IEnumerable<UserDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetUsersByRoleQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<UserDto>> Handle(GetUsersByRoleQuery request, CancellationToken cancellationToken)
    {
        var users = await _unitOfWork.Users.FindAsync(u => u.Role == request.Role);
        return _mapper.Map<IEnumerable<UserDto>>(users);
    }
}

public class SearchUsersQueryHandler : IRequestHandler<SearchUsersQuery, IEnumerable<UserDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SearchUsersQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<UserDto>> Handle(SearchUsersQuery request, CancellationToken cancellationToken)
    {
        var searchTerm = request.SearchTerm.ToLower();
        var users = await _unitOfWork.Users.FindAsync(u =>
            u.Username.ToLower().Contains(searchTerm) ||
            u.Email.ToLower().Contains(searchTerm));
        return _mapper.Map<IEnumerable<UserDto>>(users);
    }
}

public class GetClientByIdQueryHandler : IRequestHandler<GetClientByIdQuery, ClientDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetClientByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ClientDto?> Handle(GetClientByIdQuery request, CancellationToken cancellationToken)
    {
        var client = await _unitOfWork.Clients.GetByIdAsync(request.Id);
        return client != null ? _mapper.Map<ClientDto>(client) : null;
    }
}

public class GetAllClientsQueryHandler : IRequestHandler<GetAllClientsQuery, IEnumerable<ClientDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllClientsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ClientDto>> Handle(GetAllClientsQuery request, CancellationToken cancellationToken)
    {
        var clients = await _unitOfWork.Clients.GetAllAsync();
        return _mapper.Map<IEnumerable<ClientDto>>(clients);
    }
}

public class GetClientsByStatusQueryHandler : IRequestHandler<GetClientsByStatusQuery, IEnumerable<ClientDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetClientsByStatusQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ClientDto>> Handle(GetClientsByStatusQuery request, CancellationToken cancellationToken)
    {
        var clients = await _unitOfWork.Clients.FindAsync(c => c.Status == request.Status);
        return _mapper.Map<IEnumerable<ClientDto>>(clients);
    }
}

public class GetOnlineClientsQueryHandler : IRequestHandler<GetOnlineClientsQuery, IEnumerable<ClientDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IClientService _clientService;

    public GetOnlineClientsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, IClientService clientService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _clientService = clientService;
    }

    public async Task<IEnumerable<ClientDto>> Handle(GetOnlineClientsQuery request, CancellationToken cancellationToken)
    {
        var clients = await _clientService.GetOnlineClientsAsync();
        return _mapper.Map<IEnumerable<ClientDto>>(clients);
    }
}

public class GetClientByMacAddressQueryHandler : IRequestHandler<GetClientByMacAddressQuery, ClientDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IClientService _clientService;

    public GetClientByMacAddressQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, IClientService clientService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _clientService = clientService;
    }

    public async Task<ClientDto?> Handle(GetClientByMacAddressQuery request, CancellationToken cancellationToken)
    {
        var client = await _clientService.GetClientByMacAddressAsync(request.MacAddress);
        return client != null ? _mapper.Map<ClientDto>(client) : null;
    }
}

public class GetSessionByIdQueryHandler : IRequestHandler<GetSessionByIdQuery, SessionDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetSessionByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<SessionDto?> Handle(GetSessionByIdQuery request, CancellationToken cancellationToken)
    {
        var session = await _unitOfWork.Sessions.GetByIdAsync(request.Id);
        return session != null ? _mapper.Map<SessionDto>(session) : null;
    }
}

public class GetActiveSessionsQueryHandler : IRequestHandler<GetActiveSessionsQuery, IEnumerable<SessionDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetActiveSessionsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<SessionDto>> Handle(GetActiveSessionsQuery request, CancellationToken cancellationToken)
    {
        var sessions = await _unitOfWork.Sessions.FindAsync(s => s.Status == SessionStatus.Active);
        return _mapper.Map<IEnumerable<SessionDto>>(sessions);
    }
}