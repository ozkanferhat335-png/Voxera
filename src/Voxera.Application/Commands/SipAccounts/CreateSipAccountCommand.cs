using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Voxera.Application.DTOs;
using Voxera.Application.Interfaces;
using Voxera.Domain.Entities;

namespace Voxera.Application.Commands.SipAccounts;

public record CreateSipAccountCommand(
    Guid CompanyId,
    Guid ExtensionId,
    string? CustomPassword = null,
    bool EnableWebRtc = false
) : IRequest<SipAccountDto>;

public class CreateSipAccountCommandHandler : IRequestHandler<CreateSipAccountCommand, SipAccountDto>
{
    private readonly IApplicationDbContext _db;
    private readonly IFreeSwitchService _freeSwitchService;
    private readonly ILogger<CreateSipAccountCommandHandler> _logger;

    public CreateSipAccountCommandHandler(IApplicationDbContext db, IFreeSwitchService freeSwitchService, ILogger<CreateSipAccountCommandHandler> logger)
    {
        _db = db;
        _freeSwitchService = freeSwitchService;
        _logger = logger;
    }

    public async Task<SipAccountDto> Handle(CreateSipAccountCommand request, CancellationToken cancellationToken)
    {
        var extension = await _db.Extensions
            .Include(e => e.Company)
            .FirstOrDefaultAsync(e => e.Id == request.ExtensionId && e.CompanyId == request.CompanyId, cancellationToken)
            ?? throw new KeyNotFoundException("Extension not found.");

        var existingAccount = await _db.SipAccounts.AnyAsync(s => s.ExtensionId == request.ExtensionId, cancellationToken);
        if (existingAccount)
            throw new InvalidOperationException("SIP account already exists for this extension.");

        var password = request.CustomPassword ?? GenerateSecurePassword();
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password, 10);
        var domain = extension.Company!.SipDomain ?? $"{extension.Company.Slug}.sip.voxera.io";

        var sipAccount = SipAccount.Create(request.CompanyId, request.ExtensionId, extension.Number, passwordHash, domain);
        if (request.EnableWebRtc) sipAccount.EnableWebRtc();

        await _db.SipAccounts.AddAsync(sipAccount, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        // Register in FreeSWITCH
        await _freeSwitchService.CreateSipUserAsync(extension.Number, password, domain, cancellationToken);
        await _freeSwitchService.ReloadXmlAsync(cancellationToken);

        _logger.LogInformation("SIP account created for extension {Number} in company {CompanyId}", extension.Number, request.CompanyId);

        return new SipAccountDto(sipAccount.Id, extension.Number, domain, sipAccount.Status.ToString(), sipAccount.AgentStatus.ToString(), password);
    }

    private static string GenerateSecurePassword()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789!@#$";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 16).Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
