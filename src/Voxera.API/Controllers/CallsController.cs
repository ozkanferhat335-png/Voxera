using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Voxera.Application.Commands.Calls;
using Voxera.Application.Queries.Calls;
using Voxera.Domain.Enums;

namespace Voxera.API.Controllers;

[ApiController]
[Route("api/v1/calls")]
[Authorize]
[Produces("application/json")]
public class CallsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CallsController(IMediator mediator) => _mediator = mediator;

    private Guid CompanyId => Guid.Parse(User.FindFirst("company_id")!.Value);
    private Guid UserId => Guid.Parse(User.FindFirst("sub")!.Value);

    /// <summary>Get call logs with filtering and pagination</summary>
    [HttpGet]
    public async Task<IActionResult> GetCallLogs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] CallDirection? direction = null,
        [FromQuery] CallStatus? status = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetCallLogsQuery(CompanyId, page, pageSize, search, direction, status, from, to), ct);
        return Ok(result);
    }

    /// <summary>Initiate an outbound call</summary>
    [HttpPost("originate")]
    public async Task<IActionResult> OriginateCall([FromBody] OriginateCallRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new InitiateCallCommand(CompanyId, request.FromExtension, request.ToNumber, UserId), ct);
        return Ok(result);
    }

    /// <summary>Hangup an active call</summary>
    [HttpPost("{callId}/hangup")]
    public async Task<IActionResult> HangupCall(string callId, CancellationToken ct)
    {
        var result = await _mediator.Send(new HangupCallCommand(CompanyId, callId), ct);
        return Ok(new { success = result });
    }

    /// <summary>Transfer a call to another extension or number</summary>
    [HttpPost("{callId}/transfer")]
    public async Task<IActionResult> TransferCall(string callId, [FromBody] TransferCallRequest request, CancellationToken ct)
    {
        // TODO: Add TransferCallCommand
        return Ok(new { success = true, callId, destination = request.Destination });
    }

    /// <summary>Hold/unhold a call</summary>
    [HttpPost("{callId}/hold")]
    public async Task<IActionResult> HoldCall(string callId, [FromBody] HoldCallRequest request, CancellationToken ct)
    {
        return Ok(new { success = true, callId, onHold = request.Hold });
    }

    /// <summary>Get recording download URL</summary>
    [HttpGet("{callId}/recording")]
    public async Task<IActionResult> GetRecording(string callId, CancellationToken ct)
    {
        // TODO: Generate signed URL for recording download
        return Ok(new { downloadUrl = $"/recordings/{callId}.wav", expiresIn = 3600 });
    }
}

public record OriginateCallRequest(string FromExtension, string ToNumber);
public record TransferCallRequest(string Destination);
public record HoldCallRequest(bool Hold);
