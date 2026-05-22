using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Voxera.Infrastructure.Persistence;

namespace Voxera.API.Middleware;

/// <summary>
/// Middleware that validates X-API-Key header for API key authentication.
/// Allows requests to proceed if they have a valid JWT OR a valid API key.
/// </summary>
public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private const string ApiKeyHeader = "X-API-Key";

    public ApiKeyMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, ApplicationDbContext db)
    {
        // Skip if already authenticated via JWT
        if (context.User.Identity?.IsAuthenticated == true)
        {
            await _next(context);
            return;
        }

        // Check for API key
        if (!context.Request.Headers.TryGetValue(ApiKeyHeader, out var apiKeyValue))
        {
            await _next(context);
            return;
        }

        var rawKey = apiKeyValue.ToString();
        var keyHash = HashKey(rawKey);

        var apiKey = await db.ApiKeys
            .Include(k => k.Company)
            .FirstOrDefaultAsync(k => k.KeyHash == keyHash && !k.IsDeleted);

        if (apiKey is null || !apiKey.IsActive)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "Invalid or revoked API key." });
            return;
        }

        // Check IP whitelist
        if (!string.IsNullOrEmpty(apiKey.IpWhitelist))
        {
            var clientIp = context.Connection.RemoteIpAddress?.ToString();
            var allowedIps = apiKey.IpWhitelist.Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (!allowedIps.Contains(clientIp))
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsJsonAsync(new { error = "IP address not whitelisted." });
                return;
            }
        }

        // Record usage
        apiKey.RecordUsage(context.Connection.RemoteIpAddress?.ToString() ?? "unknown");
        await db.SaveChangesAsync();

        // Add company context to request
        context.Items["CompanyId"] = apiKey.CompanyId;
        context.Items["ApiKeyId"] = apiKey.Id;

        await _next(context);
    }

    private static string HashKey(string key)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(key));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
