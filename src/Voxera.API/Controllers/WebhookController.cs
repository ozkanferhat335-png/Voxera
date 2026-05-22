using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Voxera.Application.Interfaces;
using Voxera.Domain.Enums;

namespace Voxera.API.Controllers;

[ApiController]
[Route("api/v1/webhooks")]
[Produces("application/json")]
public class WebhookController : ControllerBase
{
    private readonly IApplicationDbContext _db;
    private readonly IWebhookService _webhookService;

    public WebhookController(IApplicationDbContext db, IWebhookService webhookService)
    {
        _db = db;
        _webhookService = webhookService;
    }

    /// <summary>FreeSWITCH event receiver - called by FreeSWITCH on call events</summary>
    [HttpPost("freeswitch")]
    [AllowAnonymous]
    public async Task<IActionResult> FreeSwitchEvent([FromBody] FreeSwitchEventPayload payload, CancellationToken ct)
    {
        // Validate the event came from FreeSWITCH (IP whitelist in production)
        switch (payload.EventName)
        {
            case "CHANNEL_ANSWER":
                await HandleCallAnswered(payload, ct);
                break;
            case "CHANNEL_HANGUP_COMPLETE":
                await HandleCallEnded(payload, ct);
                break;
            case "CHANNEL_CREATE":
                await HandleCallCreated(payload, ct);
                break;
            case "RECORD_STOP":
                await HandleRecordingReady(payload, ct);
                break;
        }
        return Ok();
    }

    private async Task HandleCallAnswered(FreeSwitchEventPayload payload, CancellationToken ct)
    {
        var callLog = await _db.CallLogs.FirstOrDefaultAsync(c => c.CallId == payload.UniqueId, ct);
        if (callLog is null) return;
        callLog.MarkAnswered();
        await _db.SaveChangesAsync(ct);
    }

    private async Task HandleCallEnded(FreeSwitchEventPayload payload, CancellationToken ct)
    {
        var callLog = await _db.CallLogs.FirstOrDefaultAsync(c => c.CallId == payload.UniqueId, ct);
        if (callLog is null) return;

        var reason = payload.HangupCause switch
        {
            "NORMAL_CLEARING" => CallEndReason.Normal,
            "NO_ANSWER" => CallEndReason.NoAnswer,
            "USER_BUSY" => CallEndReason.Busy,
            "ORIGINATOR_CANCEL" => CallEndReason.Cancelled,
            _ => CallEndReason.Failed
        };

        callLog.MarkEnded(reason);
        await _db.SaveChangesAsync(ct);

        await _webhookService.SendWebhookAsync(callLog.CompanyId, WebhookEventType.CallEnded, new
        {
            callId = payload.UniqueId,
            duration = callLog.DurationSeconds,
            reason = reason.ToString()
        }, ct);
    }

    private async Task HandleCallCreated(FreeSwitchEventPayload payload, CancellationToken ct)
    {
        // Notify via SignalR for real-time popup
    }

    private async Task HandleRecordingReady(FreeSwitchEventPayload payload, CancellationToken ct)
    {
        var callLog = await _db.CallLogs.FirstOrDefaultAsync(c => c.CallId == payload.UniqueId, ct);
        if (callLog is null) return;

        var fileSize = payload.RecordingFilePath is not null && System.IO.File.Exists(payload.RecordingFilePath)
            ? new System.IO.FileInfo(payload.RecordingFilePath).Length : 0;

        callLog.SetRecording(payload.RecordingFilePath ?? string.Empty, fileSize);
        await _db.SaveChangesAsync(ct);

        await _webhookService.SendWebhookAsync(callLog.CompanyId, WebhookEventType.RecordingReady, new
        {
            callId = payload.UniqueId,
            recordingPath = payload.RecordingFilePath
        }, ct);
    }
}

public record FreeSwitchEventPayload(
    string EventName,
    string UniqueId,
    string? CallerNumber,
    string? CalleeNumber,
    string? HangupCause,
    string? RecordingFilePath,
    string? Domain
);
