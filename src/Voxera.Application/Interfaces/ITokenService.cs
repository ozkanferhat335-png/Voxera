using Voxera.Domain.Entities;

namespace Voxera.Application.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    Guid? ValidateAccessToken(string token);
    Task<(string AccessToken, string RefreshToken)> RefreshTokensAsync(string refreshToken, CancellationToken ct = default);
}
