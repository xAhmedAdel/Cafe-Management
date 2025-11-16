using AutoMapper;
using CafeManagement.Application.Commands;
using CafeManagement.Application.DTOs;
using CafeManagement.Core.Entities;
using CafeManagement.Core.Enums;
using CafeManagement.Core.Interfaces;
using MediatR;

namespace CafeManagement.Application.Handlers;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IPasswordHasher<User> _passwordHasher;

    public CreateUserCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IPasswordHasher<User> passwordHasher)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _passwordHasher = passwordHasher;
    }

    public async Task<UserDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _unitOfWork.Users.FindAsync(u => u.Username == request.UserDto.Username);
        if (existingUser.Any())
            throw new InvalidOperationException("Username already exists");

        var user = new User
        {
            Username = request.UserDto.Username,
            Email = request.UserDto.Email,
            Role = request.UserDto.Role,
            Balance = request.UserDto.Balance,
            PasswordHash = _passwordHasher.HashPassword(null!, request.UserDto.Password)
        };

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<UserDto>(user);
    }
}

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UserDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateUserCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<UserDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(request.Id);
        if (user == null)
            throw new KeyNotFoundException("User not found");

        if (request.UserDto.Email != null)
            user.Email = request.UserDto.Email;

        if (request.UserDto.Balance.HasValue)
            user.Balance = request.UserDto.Balance.Value;

        user.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<UserDto>(user);
    }
}

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteUserCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(request.Id);
        if (user == null)
            return false;

        await _unitOfWork.Users.DeleteAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}

public class AuthenticateUserCommandHandler : IRequestHandler<AuthenticateUserCommand, UserLoginResponseDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthenticateUserCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IPasswordHasher<User> passwordHasher, IJwtTokenGenerator jwtTokenGenerator)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<UserLoginResponseDto?> Handle(AuthenticateUserCommand request, CancellationToken cancellationToken)
    {
        var user = (await _unitOfWork.Users.FindAsync(u => u.Username == request.LoginDto.Username)).FirstOrDefault();
        if (user == null)
            return null;

        var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.LoginDto.Password);
        if (verificationResult == PasswordVerificationResult.Failed)
            return null;

        var token = _jwtTokenGenerator.GenerateToken(user);
        var userDto = _mapper.Map<UserDto>(user);

        return new UserLoginResponseDto
        {
            Token = token,
            User = userDto,
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        };
    }
}