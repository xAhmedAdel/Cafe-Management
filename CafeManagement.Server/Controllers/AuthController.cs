using Microsoft.AspNetCore.Mvc;
using CafeManagement.Application.DTOs;
using CafeManagement.Application.Services;

namespace CafeManagement.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserLoginResponse>> Login([FromBody] UserLoginRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new UserLoginResponse
                {
                    Success = false,
                    Message = "Username and password are required"
                });
            }

            if (request.ClientId <= 0)
            {
                return BadRequest(new UserLoginResponse
                {
                    Success = false,
                    Message = "Valid client ID is required"
                });
            }

            var result = await _authService.AuthenticateUserAsync(request);

            if (result.Success)
            {
                _logger.LogInformation($"User {request.Username} logged in successfully");
                return Ok(result);
            }
            else
            {
                _logger.LogWarning($"Login failed for user {request.Username}: {result.Message}");
                return Unauthorized(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(500, new UserLoginResponse
            {
                Success = false,
                Message = "An error occurred during login"
            });
        }
    }

    [HttpPost("users")]
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Username and password are required");
            }

            if (request.Password.Length < 4)
            {
                return BadRequest("Password must be at least 4 characters long");
            }

            if (request.InitialMinutes < 0)
            {
                return BadRequest("Initial minutes cannot be negative");
            }

            var user = await _authService.CreateUserAsync(request);
            if (user == null)
            {
                return Conflict("Username already exists");
            }

            _logger.LogInformation($"Created new user: {user.Username}");
            return CreatedAtAction(nameof(GetUser), new { userId = user.Id }, user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return StatusCode(500, "An error occurred while creating the user");
        }
    }

    [HttpPost("users/{userId}/add-time")]
    public async Task<ActionResult> AddTimeToUser(int userId, [FromBody] AddTimeRequest request)
    {
        try
        {
            if (request.MinutesToAdd <= 0)
            {
                return BadRequest("Minutes to add must be greater than 0");
            }

            request.UserId = userId;
            var success = await _authService.AddTimeToUserAsync(request);

            if (!success)
            {
                return NotFound("User not found or inactive");
            }

            _logger.LogInformation($"Added {request.MinutesToAdd} minutes to user {userId}");
            return Ok("Time added successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error adding time to user {userId}");
            return StatusCode(500, "An error occurred while adding time");
        }
    }

    [HttpGet("users")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
    {
        try
        {
            var users = await _authService.GetUsersAsync();
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users");
            return StatusCode(500, "An error occurred while retrieving users");
        }
    }

    [HttpGet("users/{userId}")]
    public async Task<ActionResult<UserDto>> GetUser(int userId)
    {
        try
        {
            var user = await _authService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving user {userId}");
            return StatusCode(500, "An error occurred while retrieving the user");
        }
    }
}