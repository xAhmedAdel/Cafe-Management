using CafeManagement.Application.DTOs;
using MediatR;

namespace CafeManagement.Application.Commands;

public record CreateSessionCommand(CreateSessionDto SessionDto) : IRequest<SessionDto>;

public record EndSessionCommand(int Id) : IRequest<SessionDto>;

public record ExtendSessionCommand(int Id, ExtendSessionDto ExtendDto) : IRequest<SessionDto>;

public record UpdateSessionStatusCommand(int Id, Core.Enums.SessionStatus Status) : IRequest<SessionDto>;