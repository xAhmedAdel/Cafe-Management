using CafeManagement.Application.Commands;
using CafeManagement.Application.DTOs;
using CafeManagement.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CafeManagement.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserLoginResponseDto>> Login([FromBody] UserLoginDto loginDto)
    {
        var result = await _mediator.Send(new AuthenticateUserCommand(loginDto));

        if (result == null)
            return Unauthorized(new { message = "Invalid username or password" });

        return Ok(result);
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register([FromBody] CreateUserDto createUserDto)
    {
        try
        {
            var result = await _mediator.Send(new CreateUserCommand(createUserDto));
            return CreatedAtAction(nameof(GetUser), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUser(int id)
    {
        var result = await _mediator.Send(new GetUserByIdQuery(id));

        if (result == null)
            return NotFound();

        return Ok(result);
    }
}