using MediatR;
using Microsoft.EntityFrameworkCore;
using Voxera.Application.Interfaces;
using Voxera.Domain.Enums;

namespace Voxera.Application.Commands.Calls;

public record HangupCallCommand(Guid CompanyId, string CallId) : IRequest<bool>;

public class HangupCallCommandHandler : IRequestHandler<HangupCallCommand, bool>
{
    private readonly IApplicationDbContext _db;
    private readonly IFreeSwitchService _freeSwitchService;
    private readonly IWebhookService _webhookService;

    public HangupCallCommandHandler(IApplicationDbContext db, IFreeSwitchService freeSwitchService, IWebhookService webhookService)
    {
        _db = db;
        _freeSwitchService = freeSwitchService;
        _webhookService = webhookService;
    }

    public async Task<bool> Handle(HangupCallCommand request, CancellationToken cancellationToken)
    {
        var callLog = await _db.CallLogs
            .FirstOrDefaultAsync(c => c.CallId == request.CallId && c.CompanyId == request.CompanyId, cancellationToken)
            ?? throw new KeyNotFoundException("Call not found.");

        var success = await _freeSwitchService.HangupCallAsync(request.CallId, cancellationToken);

        callLog.MarkEnded(CallEndReason.Normal);
        await _db.SaveChangesAsync(cancellationToken);

        await _webhookService.SendWebhookAsync(request.CompanyId, WebhookEventType.CallEnded, new
        {
            callId = request.CallId,
            duration = callLog.DurationSeconds,
            reason = CallEndReason.Normal.ToString()
        }, cancellationToken);

        return success;
    }
}
