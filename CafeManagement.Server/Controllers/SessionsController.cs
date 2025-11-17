using CafeManagement.Application.Commands;
using CafeManagement.Application.DTOs;
using CafeManagement.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeManagement.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SessionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SessionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SessionDto>>> GetAll()
    {
        var result = await _mediator.Send(new GetAllSessionsQuery());
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SessionDto>> GetById(int id)
    {
        var result = await _mediator.Send(new GetSessionByIdQuery(id));

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<SessionDto>>> GetActive()
    {
        var result = await _mediator.Send(new GetActiveSessionsQuery());
        return Ok(result);
    }

    [HttpGet("client/{clientId}")]
    public async Task<ActionResult<IEnumerable<SessionDto>>> GetByClient(int clientId)
    {
        var result = await _mediator.Send(new GetSessionsByClientQuery(clientId));
        return Ok(result);
    }

    [HttpGet("active/{clientId}")]
    [AllowAnonymous]
    public async Task<ActionResult<SessionDto?>> GetActiveByClient(int clientId)
    {
        var result = await _mediator.Send(new GetActiveSessionByClientQuery(clientId));

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<SessionDto>>> GetByUser(int userId)
    {
        var result = await _mediator.Send(new GetSessionsByUserQuery(userId));
        return Ok(result);
    }

    [HttpGet("date-range")]
    public async Task<ActionResult<IEnumerable<SessionDto>>> GetByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var result = await _mediator.Send(new GetSessionsByDateRangeQuery(startDate, endDate));
        return Ok(result);
    }

    [HttpGet("summary")]
    public async Task<ActionResult<SessionSummaryDto>> GetSummary()
    {
        var result = await _mediator.Send(new GetSessionSummaryQuery());
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<SessionDto>> Create([FromBody] CreateSessionDto createSessionDto)
    {
        try
        {
            var result = await _mediator.Send(new CreateSessionCommand(createSessionDto));
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/end")]
    [AllowAnonymous]
    public async Task<ActionResult<SessionDto>> EndSession(int id)
    {
        try
        {
            var result = await _mediator.Send(new EndSessionCommand(id));
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/extend")]
    public async Task<ActionResult<SessionDto>> ExtendSession(int id, [FromBody] ExtendSessionDto extendSessionDto)
    {
        try
        {
            var result = await _mediator.Send(new ExtendSessionCommand(id, extendSessionDto));
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}/status")]
    public async Task<ActionResult<SessionDto>> UpdateStatus(int id, [FromBody] int status)
    {
        try
        {
            var sessionStatus = (Core.Enums.SessionStatus)status;
            var result = await _mediator.Send(new UpdateSessionStatusCommand(id, sessionStatus));
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}