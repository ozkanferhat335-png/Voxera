using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Voxera.Application.DTOs;
using Voxera.Application.Interfaces;
using Voxera.Domain.Entities;
using Voxera.Domain.Enums;

namespace Voxera.Application.Commands.Auth;

public record RegisterCommand(
    string CompanyName,
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string? Phone = null
) : IRequest<AuthResponseDto>;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponseDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ITokenService _tokenService;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(IApplicationDbContext db, ITokenService tokenService, ILogger<RegisterCommandHandler> logger)
    {
        _db = db;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<AuthResponseDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var emailExists = await _db.Users.AnyAsync(u => u.Email == request.Email.ToLowerInvariant(), cancellationToken);
        if (emailExists)
            throw new InvalidOperationException("Email address is already registered.");

        var slug = GenerateSlug(request.CompanyName);
        var slugExists = await _db.Companies.AnyAsync(c => c.Slug == slug, cancellationToken);
        if (slugExists)
            slug = $"{slug}-{Guid.NewGuid().ToString()[..6]}";

        var company = Company.Create(request.CompanyName, slug);
        await _db.Companies.AddAsync(company, cancellationToken);

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, 12);
        var user = User.Create(company.Id, request.FirstName, request.LastName, request.Email, passwordHash, UserRole.CompanyAdmin);
        await _db.Users.AddAsync(user, cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();
        user.SetRefreshToken(refreshToken, DateTime.UtcNow.AddDays(30));
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("New company registered: {Company} by {Email}", request.CompanyName, request.Email);

        return new AuthResponseDto(accessToken, refreshToken, user.Id, user.FullName, user.Email, user.Role.ToString(), company.Id, company.Name);
    }

    private static string GenerateSlug(string name)
    {
        return name.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("ş", "s").Replace("ğ", "g").Replace("ü", "u")
            .Replace("ö", "o").Replace("ı", "i").Replace("ç", "c")
            .Where(c => char.IsLetterOrDigit(c) || c == '-')
            .Aggregate(string.Empty, (acc, c) => acc + c);
    }
}
