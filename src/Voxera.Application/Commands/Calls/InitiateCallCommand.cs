using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Voxera.Application.DTOs;
using Voxera.Application.Interfaces;
using Voxera.Domain.Entities;
using Voxera.Domain.Enums;

namespace Voxera.Application.Commands.Calls;

public record InitiateCallCommand(
    Guid CompanyId,
    string FromExtension,
    string ToNumber,
    Guid? RequestedByUserId = null
) : IRequest<CallInitiatedDto>;

public class InitiateCallCommandHandler : IRequestHandler<InitiateCallCommand, CallInitiatedDto>
{
    private readonly IApplicationDbContext _db;
    private readonly IFreeSwitchService _freeSwitchService;
    private readonly IWebhookService _webhookService;
    private readonly ILogger<InitiateCallCommandHandler> _logger;

    public InitiateCallCommandHandler(IApplicationDbContext db, IFreeSwitchService freeSwitchService, IWebhookService webhookService, ILogger<InitiateCallCommandHandler> logger)
    {
        _db = db;
        _freeSwitchService = freeSwitchService;
        _webhookService = webhookService;
        _logger = logger;
    }

    public async Task<CallInitiatedDto> Handle(InitiateCallCommand request, CancellationToken cancellationToken)
    {
        var company = await _db.Companies.FindAsync(new object[] { request.CompanyId }, cancellationToken)
            ?? throw new KeyNotFoundException("Company not found.");

        var extension = await _db.Extensions
            .FirstOrDefaultAsync(e => e.Number == request.FromExtension && e.CompanyId == request.CompanyId, cancellationToken)
            ?? throw new KeyNotFoundException("Extension not found.");

        var callId = Guid.NewGuid().ToString();
        var direction = IsInternalNumber(request.ToNumber) ? CallDirection.Internal : CallDirection.Outbound;

        var callLog = CallLog.Create(request.CompanyId, callId, request.FromExtension, request.ToNumber, direction);
        callLog.MarkAsUpdated();
        await _db.CallLogs.AddAsync(callLog, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        var success = await _freeSwitchService.OriginateCallAsync(request.FromExtension, request.ToNumber, company.SipDomain!, cancellationToken);

        if (!success)
        {
            callLog.MarkEnded(CallEndReason.Failed);
            await _db.SaveChangesAsync(cancellationToken);
            throw new InvalidOperationException("Failed to initiate call.");
        }

        await _webhookService.SendWebhookAsync(request.CompanyId, WebhookEventType.IncomingCall, new
        {
            callId,
            from = request.FromExtension,
            to = request.ToNumber,
            direction = direction.ToString()
        }, cancellationToken);

        _logger.LogInformation("Call initiated: {CallId} from {From} to {To}", callId, request.FromExtension, request.ToNumber);

        return new CallInitiatedDto(callId, request.FromExtension, request.ToNumber, direction.ToString(), DateTime.UtcNow);
    }

    private static bool IsInternalNumber(string number) => number.Length <= 5 && number.All(char.IsDigit);
}
