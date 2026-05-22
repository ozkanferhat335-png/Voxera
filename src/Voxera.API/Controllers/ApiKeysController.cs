using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Voxera.Application.DTOs;
using Voxera.Application.Interfaces;
using Voxera.Domain.Entities;

namespace Voxera.API.Controllers;

[ApiController]
[Route("api/v1/api-keys")]
[Authorize(Policy = "CompanyAdmin")]
[Produces("application/json")]
public class ApiKeysController : ControllerBase
{
    private readonly IApplicationDbContext _db;

    public ApiKeysController(IApplicationDbContext db) => _db = db;

    private Guid CompanyId => Guid.Parse(User.FindFirst("company_id")!.Value);
    private Guid UserId => Guid.Parse(User.FindFirst("sub")!.Value);

    /// <summary>List all API keys for the company</summary>
    [HttpGet]
    public async Task<IActionResult> GetApiKeys(CancellationToken ct)
    {
        var keys = await _db.ApiKeys
            .Where(k => k.CompanyId == CompanyId)
            .Select(k => new ApiKeyDto(k.Id, k.Name, k.KeyPrefix, k.Status.ToString(), k.Permissions, k.ExpiresAt, k.LastUsedAt, k.RequestCount))
            .ToListAsync(ct);
        return Ok(keys);
    }

    /// <summary>Create a new API key</summary>
    [HttpPost]
    public async Task<IActionResult> CreateApiKey([FromBody] CreateApiKeyRequest request, CancellationToken ct)
    {
        // Generate a secure random key
        var rawKey = $"vxr_{GenerateSecureKey(32)}";
        var keyHash = HashKey(rawKey);
        var keyPrefix = rawKey[..12];

        var apiKey = ApiKey.Create(CompanyId, UserId, request.Name, keyHash, keyPrefix, request.Permissions ?? new[] { "calls:read", "calls:write" });
        if (request.ExpiresAt.HasValue) apiKey.SetExpiry(request.ExpiresAt);
        if (!string.IsNullOrEmpty(request.IpWhitelist)) apiKey.SetIpWhitelist(request.IpWhitelist);

        await _db.ApiKeys.AddAsync(apiKey, ct);
        await _db.SaveChangesAsync(ct);

        // Return the raw key ONCE - it won't be shown again
        return Ok(new { id = apiKey.Id, name = apiKey.Name, key = rawKey, prefix = keyPrefix, message = "Store this key securely. It will not be shown again." });
    }

    /// <summary>Revoke an API key</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> RevokeApiKey(Guid id, CancellationToken ct)
    {
        var key = await _db.ApiKeys.FirstOrDefaultAsync(k => k.Id == id && k.CompanyId == CompanyId, ct);
        if (key is null) return NotFound();
        key.Revoke();
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    private static string GenerateSecureKey(int length)
    {
        var bytes = new byte[length];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes).Replace("+", "").Replace("/", "").Replace("=", "")[..length];
    }

    private static string HashKey(string key)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(key));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}

public record CreateApiKeyRequest(string Name, string[]? Permissions = null, DateTime? ExpiresAt = null, string? IpWhitelist = null);
