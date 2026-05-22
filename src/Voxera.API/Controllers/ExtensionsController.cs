using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Voxera.Application.DTOs;
using Voxera.Application.Interfaces;
using Voxera.Domain.Entities;
using Voxera.Domain.Enums;

namespace Voxera.API.Controllers;

[ApiController]
[Route("api/v1/extensions")]
[Authorize]
[Produces("application/json")]
public class ExtensionsController : ControllerBase
{
    private readonly IApplicationDbContext _db;

    public ExtensionsController(IApplicationDbContext db) => _db = db;

    private Guid CompanyId => Guid.Parse(User.FindFirst("company_id")!.Value);

    /// <summary>List all extensions for the company</summary>
    [HttpGet]
    public async Task<IActionResult> GetExtensions(CancellationToken ct)
    {
        var extensions = await _db.Extensions
            .Where(e => e.CompanyId == CompanyId)
            .Include(e => e.SipAccount)
            .Select(e => new ExtensionDto(
                e.Id, e.Number, e.DisplayName, e.Type.ToString(), e.Status.ToString(),
                e.VoicemailEnabled, e.RecordCalls, e.DoNotDisturb, e.ForwardTo,
                e.SipAccount != null ? e.SipAccount.AgentStatus.ToString() : null))
            .ToListAsync(ct);
        return Ok(extensions);
    }

    /// <summary>Get a specific extension</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetExtension(Guid id, CancellationToken ct)
    {
        var extension = await _db.Extensions
            .Where(e => e.Id == id && e.CompanyId == CompanyId)
            .Include(e => e.SipAccount)
            .FirstOrDefaultAsync(ct);
        if (extension is null) return NotFound();
        return Ok(new ExtensionDto(
            extension.Id, extension.Number, extension.DisplayName, extension.Type.ToString(),
            extension.Status.ToString(), extension.VoicemailEnabled, extension.RecordCalls,
            extension.DoNotDisturb, extension.ForwardTo,
            extension.SipAccount?.AgentStatus.ToString()));
    }

    /// <summary>Create a new extension</summary>
    [HttpPost]
    [Authorize(Policy = "CompanyAdmin")]
    public async Task<IActionResult> CreateExtension([FromBody] CreateExtensionRequest request, CancellationToken ct)
    {
        var company = await _db.Companies.FindAsync(new object[] { CompanyId }, ct);
        var extensionCount = await _db.Extensions.CountAsync(e => e.CompanyId == CompanyId, ct);
        if (extensionCount >= company!.MaxExtensions)
            return BadRequest(new { error = "Extension limit reached for your plan." });

        var numberExists = await _db.Extensions.AnyAsync(e => e.CompanyId == CompanyId && e.Number == request.Number, ct);
        if (numberExists) return BadRequest(new { error = "Extension number already exists." });

        var extension = Extension.Create(CompanyId, request.Number, request.DisplayName, request.Type);
        await _db.Extensions.AddAsync(extension, ct);
        await _db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetExtension), new { id = extension.Id }, new { id = extension.Id, number = extension.Number });
    }

    /// <summary>Update extension settings</summary>
    [HttpPut("{id}")]
    [Authorize(Policy = "CompanyAdmin")]
    public async Task<IActionResult> UpdateExtension(Guid id, [FromBody] UpdateExtensionRequest request, CancellationToken ct)
    {
        var extension = await _db.Extensions.FirstOrDefaultAsync(e => e.Id == id && e.CompanyId == CompanyId, ct);
        if (extension is null) return NotFound();

        if (request.ForwardTo is not null)
            extension.SetForward(request.ForwardTo, request.ForwardCondition ?? ForwardCondition.Always);
        if (request.DoNotDisturb.HasValue)
            extension.SetDoNotDisturb(request.DoNotDisturb.Value);
        if (request.VoicemailEnabled.HasValue)
            extension.SetVoicemail(request.VoicemailEnabled.Value, request.VoicemailPin);
        if (request.RecordCalls.HasValue)
            extension.SetCallRecording(request.RecordCalls.Value);

        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    /// <summary>Delete an extension</summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = "CompanyAdmin")]
    public async Task<IActionResult> DeleteExtension(Guid id, CancellationToken ct)
    {
        var extension = await _db.Extensions.FirstOrDefaultAsync(e => e.Id == id && e.CompanyId == CompanyId, ct);
        if (extension is null) return NotFound();
        extension.SoftDelete();
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}

public record CreateExtensionRequest(string Number, string DisplayName, ExtensionType Type = ExtensionType.User);
public record UpdateExtensionRequest(
    string? ForwardTo = null,
    ForwardCondition? ForwardCondition = null,
    bool? DoNotDisturb = null,
    bool? VoicemailEnabled = null,
    string? VoicemailPin = null,
    bool? RecordCalls = null
);
