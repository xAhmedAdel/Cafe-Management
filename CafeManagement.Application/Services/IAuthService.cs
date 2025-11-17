using CafeManagement.Application.DTOs;

namespace CafeManagement.Application.Services;

public interface IAuthService
{
    Task<UserLoginResponse> AuthenticateUserAsync(UserLoginRequest request);
    Task<UserDto?> CreateUserAsync(CreateUserRequest request);
    Task<bool> AddTimeToUserAsync(AddTimeRequest request);
    Task<IEnumerable<UserDto>> GetUsersAsync();
    Task<UserDto?> GetUserByIdAsync(int userId);
}