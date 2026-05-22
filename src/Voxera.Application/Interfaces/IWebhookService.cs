using Voxera.Domain.Enums;

namespace Voxera.Application.Interfaces;

public interface IWebhookService
{
    Task SendWebhookAsync(Guid companyId, WebhookEventType eventType, object payload, CancellationToken ct = default);
    Task<bool> ValidateWebhookSignatureAsync(string payload, string signature, string secret);
}
