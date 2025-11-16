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
public class ClientsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ClientsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClientDto>>> GetAll()
    {
        var result = await _mediator.Send(new GetAllClientsQuery());
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ClientDto>> GetById(int id)
    {
        var result = await _mediator.Send(new GetClientByIdQuery(id));

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpGet("online")]
    public async Task<ActionResult<IEnumerable<ClientDto>>> GetOnline()
    {
        var result = await _mediator.Send(new GetOnlineClientsQuery());
        return Ok(result);
    }

    [HttpGet("status/{status}")]
    public async Task<ActionResult<IEnumerable<ClientDto>>> GetByStatus(int status)
    {
        var clientStatus = (Core.Enums.ClientStatus)status;
        var result = await _mediator.Send(new GetClientsByStatusQuery(clientStatus));
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ClientDto>> Create([FromBody] CreateClientDto createClientDto)
    {
        try
        {
            var result = await _mediator.Send(new CreateClientCommand(createClientDto));
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}/status")]
    public async Task<ActionResult<ClientDto>> UpdateStatus(int id, [FromBody] UpdateClientStatusDto statusDto)
    {
        try
        {
            var result = await _mediator.Send(new UpdateClientStatusCommand(id, statusDto));
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

    [HttpGet("mac/{macAddress}")]
    public async Task<ActionResult<ClientDto>> GetByMacAddress(string macAddress)
    {
        var result = await _mediator.Send(new GetClientByMacAddressQuery(macAddress));

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpGet("ip/{ipAddress}")]
    public async Task<ActionResult<ClientDto>> GetByIpAddress(string ipAddress)
    {
        var result = await _mediator.Send(new GetClientByIpAddressQuery(ipAddress));

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var result = await _mediator.Send(new DeleteClientCommand(id));

        if (!result)
            return NotFound();

        return NoContent();
    }
}