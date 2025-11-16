using CafeManagement.Application.DTOs;
using MediatR;

namespace CafeManagement.Application.Queries;

public record GetUserByIdQuery(int Id) : IRequest<UserDto?>;

public record GetAllUsersQuery() : IRequest<IEnumerable<UserDto>>;

public record GetUsersByRoleQuery(Core.Enums.UserRole Role) : IRequest<IEnumerable<UserDto>>;

public record SearchUsersQuery(string SearchTerm) : IRequest<IEnumerable<UserDto>>;