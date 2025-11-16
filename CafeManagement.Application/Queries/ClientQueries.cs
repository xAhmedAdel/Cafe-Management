using CafeManagement.Application.DTOs;
using MediatR;

namespace CafeManagement.Application.Queries;

public record GetClientByIdQuery(int Id) : IRequest<ClientDto?>;

public record GetAllClientsQuery() : IRequest<IEnumerable<ClientDto>>;

public record GetClientsByStatusQuery(Core.Enums.ClientStatus Status) : IRequest<IEnumerable<ClientDto>>;

public record GetOnlineClientsQuery() : IRequest<IEnumerable<ClientDto>>;

public record GetClientByMacAddressQuery(string MacAddress) : IRequest<ClientDto?>;

public record GetClientByIpAddressQuery(string IpAddress) : IRequest<ClientDto?>;