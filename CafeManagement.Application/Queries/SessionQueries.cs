using CafeManagement.Application.DTOs;
using MediatR;

namespace CafeManagement.Application.Queries;

public record GetSessionByIdQuery(int Id) : IRequest<SessionDto?>;

public record GetAllSessionsQuery() : IRequest<IEnumerable<SessionDto>>;

public record GetSessionsByClientQuery(int ClientId) : IRequest<IEnumerable<SessionDto>>;

public record GetActiveSessionByClientQuery(int ClientId) : IRequest<SessionDto?>;

public record GetSessionsByUserQuery(int UserId) : IRequest<IEnumerable<SessionDto>>;

public record GetActiveSessionsQuery() : IRequest<IEnumerable<SessionDto>>;

public record GetSessionsByDateRangeQuery(DateTime StartDate, DateTime EndDate) : IRequest<IEnumerable<SessionDto>>;

public record GetSessionSummaryQuery() : IRequest<SessionSummaryDto>;