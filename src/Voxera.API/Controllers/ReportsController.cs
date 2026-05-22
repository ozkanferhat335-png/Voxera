using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Voxera.Application.Interfaces;
using Voxera.Domain.Enums;

namespace Voxera.API.Controllers;

[ApiController]
[Route("api/v1/reports")]
[Authorize]
[Produces("application/json")]
public class ReportsController : ControllerBase
{
    private readonly IApplicationDbContext _db;

    public ReportsController(IApplicationDbContext db) => _db = db;

    private Guid CompanyId => Guid.Parse(User.FindFirst("company_id")!.Value);

    /// <summary>Get daily call report</summary>
    [HttpGet("daily")]
    public async Task<IActionResult> GetDailyReport([FromQuery] DateTime? date = null, CancellationToken ct = default)
    {
        var reportDate = date?.Date ?? DateTime.UtcNow.Date;
        var nextDay = reportDate.AddDays(1);

        var calls = await _db.CallLogs
            .Where(c => c.CompanyId == CompanyId && c.StartedAt >= reportDate && c.StartedAt < nextDay)
            .ToListAsync(ct);

        return Ok(new
        {
            date = reportDate.ToString("yyyy-MM-dd"),
            totalCalls = calls.Count,
            answeredCalls = calls.Count(c => c.Status == CallStatus.Completed),
            missedCalls = calls.Count(c => c.Status == CallStatus.Missed),
            inboundCalls = calls.Count(c => c.Direction == CallDirection.Inbound),
            outboundCalls = calls.Count(c => c.Direction == CallDirection.Outbound),
            internalCalls = calls.Count(c => c.Direction == CallDirection.Internal),
            averageDuration = calls.Where(c => c.DurationSeconds.HasValue).Select(c => c.DurationSeconds!.Value).DefaultIfEmpty(0).Average(),
            totalDuration = calls.Sum(c => c.DurationSeconds ?? 0),
            recordedCalls = calls.Count(c => c.IsRecorded)
        });
    }

    /// <summary>Get operator performance report</summary>
    [HttpGet("operator-performance")]
    public async Task<IActionResult> GetOperatorPerformance([FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null, CancellationToken ct = default)
    {
        var startDate = from ?? DateTime.UtcNow.AddDays(-30);
        var endDate = to ?? DateTime.UtcNow;

        var extensions = await _db.Extensions
            .Where(e => e.CompanyId == CompanyId && e.Type == ExtensionType.User)
            .Include(e => e.User)
            .ToListAsync(ct);

        var callLogs = await _db.CallLogs
            .Where(c => c.CompanyId == CompanyId && c.StartedAt >= startDate && c.StartedAt <= endDate)
            .ToListAsync(ct);

        var performance = extensions.Select(ext =>
        {
            var extCalls = callLogs.Where(c => c.ToExtensionId == ext.Id || c.FromExtensionId == ext.Id).ToList();
            return new
            {
                extensionId = ext.Id,
                extensionNumber = ext.Number,
                operatorName = ext.User?.FullName ?? ext.DisplayName,
                totalCalls = extCalls.Count,
                answeredCalls = extCalls.Count(c => c.Status == CallStatus.Completed),
                missedCalls = extCalls.Count(c => c.Status == CallStatus.Missed),
                averageDuration = extCalls.Where(c => c.DurationSeconds.HasValue).Select(c => c.DurationSeconds!.Value).DefaultIfEmpty(0).Average(),
                totalTalkTime = extCalls.Sum(c => c.DurationSeconds ?? 0)
            };
        });

        return Ok(performance);
    }

    /// <summary>Get missed calls report</summary>
    [HttpGet("missed-calls")]
    public async Task<IActionResult> GetMissedCalls([FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null, CancellationToken ct = default)
    {
        var startDate = from ?? DateTime.UtcNow.Date;
        var endDate = to ?? DateTime.UtcNow;

        var missedCalls = await _db.CallLogs
            .Where(c => c.CompanyId == CompanyId && c.Status == CallStatus.Missed && c.StartedAt >= startDate && c.StartedAt <= endDate)
            .OrderByDescending(c => c.StartedAt)
            .Select(c => new { c.CallerNumber, c.CallerName, c.StartedAt, c.RingDurationSeconds })
            .ToListAsync(ct);

        return Ok(missedCalls);
    }
}
