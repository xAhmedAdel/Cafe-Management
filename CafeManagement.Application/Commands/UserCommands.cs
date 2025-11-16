using CafeManagement.Application.DTOs;
using MediatR;

namespace CafeManagement.Application.Commands;

public record CreateUserCommand(CreateUserDto UserDto) : IRequest<UserDto>;

public record UpdateUserCommand(int Id, UpdateUserDto UserDto) : IRequest<UserDto>;

public record DeleteUserCommand(int Id) : IRequest<bool>;

public record AuthenticateUserCommand(UserLoginDto LoginDto) : IRequest<UserLoginResponseDto?>;

public record UpdateUserBalanceCommand(int UserId, decimal Amount) : IRequest<UserDto>;