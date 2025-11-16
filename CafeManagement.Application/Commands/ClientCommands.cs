using CafeManagement.Application.DTOs;
using MediatR;

namespace CafeManagement.Application.Commands;

public record CreateClientCommand(CreateClientDto ClientDto) : IRequest<ClientDto>;

public record UpdateClientStatusCommand(int Id, UpdateClientStatusDto StatusDto) : IRequest<ClientDto>;

public record DeleteClientCommand(int Id) : IRequest<bool>;

public record UpdateClientConfigurationCommand(int Id, string Configuration) : IRequest<ClientDto>;