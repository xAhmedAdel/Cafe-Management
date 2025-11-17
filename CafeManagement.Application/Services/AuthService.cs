using CafeManagement.Application.DTOs;
using CafeManagement.Core.Entities;
using CafeManagement.Core.Enums;
using CafeManagement.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;

namespace CafeManagement.Application.Services;

public class AuthService : IAuthService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IApplicationDbContext context, ILogger<AuthService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<UserLoginResponse> AuthenticateUserAsync(UserLoginRequest request)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == request.Username.ToLower() && u.IsActive);

            if (user == null)
            {
                return new UserLoginResponse
                {
                    Success = false,
                    Message = "Invalid username or password"
                };
            }

            if (!VerifyPassword(request.Password, user.PasswordHash))
            {
                return new UserLoginResponse
                {
                    Success = false,
                    Message = "Invalid username or password"
                };
            }

            if (user.AvailableMinutes <= 0)
            {
                return new UserLoginResponse
                {
                    Success = false,
                    Message = "No time credits available. Please contact admin."
                };
            }

            // Check if user already has an active session
            var activeSession = await _context.Sessions
                .FirstOrDefaultAsync(s => s.UserId == user.Id && s.Status == SessionStatus.Active);

            if (activeSession != null)
            {
                return new UserLoginResponse
                {
                    Success = false,
                    Message = "User already has an active session on another computer"
                };
            }

            // Create new session
            var session = new Session
            {
                ClientId = request.ClientId,
                UserId = user.Id,
                StartTime = DateTime.UtcNow,
                Status = SessionStatus.Active,
                DurationMinutes = user.AvailableMinutes,
                HourlyRate = 0
            };

            _context.Sessions.Add(session);

            // Update user last login
            user.LastLoginTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation($"User {user.Username} authenticated successfully on client {request.ClientId}");

            return new UserLoginResponse
            {
                Success = true,
                Message = "Login successful",
                User = MapToUserDto(user),
                Session = MapToSessionDto(session)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user authentication");
            return new UserLoginResponse
            {
                Success = false,
                Message = "An error occurred during authentication"
            };
        }
    }

    public async Task<UserDto?> CreateUserAsync(CreateUserRequest request)
    {
        try
        {
            // Check if username already exists
            if (await _context.Users.AnyAsync(u => u.Username.ToLower() == request.Username.ToLower()))
            {
                return null;
            }

            var user = new User
            {
                Username = request.Username,
                PasswordHash = HashPassword(request.Password),
                Email = request.Email,
                Role = UserRole.Customer,
                AvailableMinutes = request.InitialMinutes,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Created new user: {user.Username}");
            return MapToUserDto(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return null;
        }
    }

    public async Task<bool> AddTimeToUserAsync(AddTimeRequest request)
    {
        try
        {
            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null || !user.IsActive)
            {
                return false;
            }

            user.AvailableMinutes += request.MinutesToAdd;
            user.UpdatedAt = DateTime.UtcNow;

            // Create usage log entry
            var usageLog = new UsageLog
            {
                ClientId = 0, // This is an admin action (0 indicates no specific client)
                UserId = user.Id,
                Action = "Time Added",
                Details = $"Added {request.MinutesToAdd} minutes. Reason: {request.Reason ?? "Admin credit"}",
                Timestamp = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.UsageLogs.Add(usageLog);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Added {request.MinutesToAdd} minutes to user {user.Username}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding time to user");
            return false;
        }
    }

    public async Task<IEnumerable<UserDto>> GetUsersAsync()
    {
        var users = await _context.Users
            .Where(u => u.Role == UserRole.Customer)
            .OrderByDescending(u => u.LastLoginTime)
            .ToListAsync();

        return users.Select(MapToUserDto);
    }

    public async Task<UserDto?> GetUserByIdAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        return user != null ? MapToUserDto(user) : null;
    }

    private static string HashPassword(string password)
    {
        const string salt = "cafe_management_salt_2024";

        string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password,
            salt: System.Text.Encoding.UTF8.GetBytes(salt),
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 10000,
            numBytesRequested: 256 / 8));

        return hashed;
    }

    private static bool VerifyPassword(string password, string hash)
    {
        return HashPassword(password) == hash;
    }

    private static UserDto MapToUserDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = (int)user.Role,
            AvailableMinutes = user.AvailableMinutes,
            Balance = user.Balance,
            IsActive = user.IsActive
        };
    }

    private static SessionDto MapToSessionDto(Session session)
    {
        return new SessionDto
        {
            Id = session.Id,
            ClientId = session.ClientId,
            UserId = session.UserId ?? 0,
            StartTime = session.StartTime,
            EndTime = session.EndTime,
            Status = (int)session.Status,
            DurationMinutes = session.DurationMinutes,
            TotalAmount = session.TotalAmount,
            HourlyRate = session.HourlyRate
        };
    }
}