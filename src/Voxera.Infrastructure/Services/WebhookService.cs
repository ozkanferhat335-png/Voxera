using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Voxera.Application.Interfaces;
using Voxera.Domain.Enums;

namespace Voxera.Infrastructure.Services;

public class WebhookService : IWebhookService
{
    private readonly IApplicationDbContext _db;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<WebhookService> _logger;

    public WebhookService(IApplicationDbContext db, IHttpClientFactory httpClientFactory, ILogger<WebhookService> logger)
    {
        _db = db;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task SendWebhookAsync(Guid companyId, WebhookEventType eventType, object payload, CancellationToken ct = default)
    {
        var company = await _db.Companies.FindAsync(new object[] { companyId }, ct);
        if (company?.WebhookUrl is null) return;

        var webhookPayload = new
        {
            event_type = eventType.ToString(),
            company_id = companyId,
            timestamp = DateTime.UtcNow,
            data = payload
        };

        var json = JsonSerializer.Serialize(webhookPayload, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower });
        var signature = GenerateSignature(json, company.WebhookSecret ?? string.Empty);

        try
        {
            var client = _httpClientFactory.CreateClient("webhook");
            var request = new HttpRequestMessage(HttpMethod.Post, company.WebhookUrl)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            request.Headers.Add("X-Voxera-Signature", signature);
            request.Headers.Add("X-Voxera-Event", eventType.ToString());
            request.Headers.Add("X-Voxera-Timestamp", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());

            var response = await client.SendAsync(request, ct);
            _logger.LogInformation("Webhook sent to {Url}: {Status}", company.WebhookUrl, response.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send webhook to {Url}", company.WebhookUrl);
        }
    }

    public Task<bool> ValidateWebhookSignatureAsync(string payload, string signature, string secret)
    {
        var expected = GenerateSignature(payload, secret);
        return Task.FromResult(string.Equals(expected, signature, StringComparison.OrdinalIgnoreCase));
    }

    private static string GenerateSignature(string payload, string secret)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);
        using var hmac = new HMACSHA256(keyBytes);
        var hash = hmac.ComputeHash(payloadBytes);
        return $"sha256={Convert.ToHexString(hash).ToLowerInvariant()}";
    }
}
