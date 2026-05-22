using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Voxera.Application.DTOs;
using Voxera.Application.Interfaces;
using Voxera.Domain.Entities;
using Voxera.Domain.Enums;

namespace Voxera.Application.Commands.Auth;

public record LoginCommand(string Email, string Password, string? IpAddress, string? UserAgent) : IRequest<AuthResponseDto>;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ITokenService _tokenService;
    private readonly ICacheService _cache;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(IApplicationDbContext db, ITokenService tokenService, ICacheService cache, ILogger<LoginCommandHandler> logger)
    {
        _db = db;
        _tokenService = tokenService;
        _cache = cache;
        _logger = logger;
    }

    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _db.Users
            .Include(u => u.Company)
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLowerInvariant() && !u.IsDeleted, cancellationToken)
            ?? throw new UnauthorizedAccessException("Invalid email or password.");

        if (user.IsLocked)
            throw new UnauthorizedAccessException("Account is temporarily locked due to multiple failed login attempts.");

        if (user.Status != UserStatus.Active)
            throw new UnauthorizedAccessException("Account is not active.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            user.RecordFailedLogin();
            await _db.SaveChangesAsync(cancellationToken);
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();

        user.SetRefreshToken(refreshToken, DateTime.UtcNow.AddDays(30));
        user.RecordLogin(request.IpAddress ?? "unknown");
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {Email} logged in from {Ip}", user.Email, request.IpAddress);

        return new AuthResponseDto(
            accessToken,
            refreshToken,
            user.Id,
            user.FullName,
            user.Email,
            user.Role.ToString(),
            user.CompanyId,
            user.Company?.Name ?? string.Empty
        );
    }
}
