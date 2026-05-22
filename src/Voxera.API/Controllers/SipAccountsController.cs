using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Voxera.Application.Commands.SipAccounts;
using Voxera.Application.DTOs;
using Voxera.Application.Interfaces;
using Voxera.Domain.Enums;

namespace Voxera.API.Controllers;

[ApiController]
[Route("api/v1/sip-accounts")]
[Authorize]
[Produces("application/json")]
public class SipAccountsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IApplicationDbContext _db;

    public SipAccountsController(IMediator mediator, IApplicationDbContext db)
    {
        _mediator = mediator;
        _db = db;
    }

    private Guid CompanyId => Guid.Parse(User.FindFirst("company_id")!.Value);

    /// <summary>List all SIP accounts for the company</summary>
    [HttpGet]
    public async Task<IActionResult> GetSipAccounts(CancellationToken ct)
    {
        var accounts = await _db.SipAccounts
            .Where(s => s.CompanyId == CompanyId)
            .Select(s => new SipAccountDto(s.Id, s.Username, s.Domain, s.Status.ToString(), s.AgentStatus.ToString()))
            .ToListAsync(ct);
        return Ok(accounts);
    }

    /// <summary>Create a SIP account for an extension</summary>
    [HttpPost]
    [Authorize(Policy = "CompanyAdmin")]
    public async Task<IActionResult> CreateSipAccount([FromBody] CreateSipAccountRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateSipAccountCommand(CompanyId, request.ExtensionId, request.Password, request.EnableWebRtc), ct);
        return CreatedAtAction(nameof(GetSipAccounts), result);
    }

    /// <summary>Update agent status (Available, Busy, Away, DND)</summary>
    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateAgentStatus(Guid id, [FromBody] UpdateAgentStatusRequest request, CancellationToken ct)
    {
        var account = await _db.SipAccounts.FirstOrDefaultAsync(s => s.Id == id && s.CompanyId == CompanyId, ct);
        if (account is null) return NotFound();
        account.SetAgentStatus(request.Status);
        await _db.SaveChangesAsync(ct);
        return Ok(new { id, status = request.Status.ToString() });
    }

    /// <summary>Delete a SIP account</summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = "CompanyAdmin")]
    public async Task<IActionResult> DeleteSipAccount(Guid id, CancellationToken ct)
    {
        var account = await _db.SipAccounts.FirstOrDefaultAsync(s => s.Id == id && s.CompanyId == CompanyId, ct);
        if (account is null) return NotFound();
        account.Deactivate();
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}

public record CreateSipAccountRequest(Guid ExtensionId, string? Password = null, bool EnableWebRtc = false);
public record UpdateAgentStatusRequest(AgentStatus Status);
