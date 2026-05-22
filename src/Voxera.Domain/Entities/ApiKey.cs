using Voxera.Domain.Enums;

namespace Voxera.Domain.Entities;

public class ApiKey : BaseEntity
{
    public Guid CompanyId { get; private set; }
    public Guid? UserId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string KeyHash { get; private set; } = string.Empty;
    public string KeyPrefix { get; private set; } = string.Empty;  // First 8 chars for display
    public ApiKeyStatus Status { get; private set; } = ApiKeyStatus.Active;
    public string[] Permissions { get; private set; } = Array.Empty<string>();
    public string? IpWhitelist { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public DateTime? LastUsedAt { get; private set; }
    public string? LastUsedIp { get; private set; }
    public long RequestCount { get; private set; } = 0;
    public int RateLimitPerMinute { get; private set; } = 60;
    public int RateLimitPerDay { get; private set; } = 10000;

    // Navigation
    public Company? Company { get; private set; }
    public User? User { get; private set; }

    protected ApiKey() { }

    public static ApiKey Create(Guid companyId, Guid? userId, string name, string keyHash, string keyPrefix, string[] permissions)
    {
        return new ApiKey
        {
            CompanyId = companyId,
            UserId = userId,
            Name = name,
            KeyHash = keyHash,
            KeyPrefix = keyPrefix,
            Permissions = permissions
        };
    }

    public void RecordUsage(string ip)
    {
        LastUsedAt = DateTime.UtcNow;
        LastUsedIp = ip;
        RequestCount++;
        MarkAsUpdated();
    }

    public void Revoke() { Status = ApiKeyStatus.Revoked; MarkAsUpdated(); }
    public void SetIpWhitelist(string? ips) { IpWhitelist = ips; MarkAsUpdated(); }
    public void SetExpiry(DateTime? expiresAt) { ExpiresAt = expiresAt; MarkAsUpdated(); }
    public bool IsExpired => ExpiresAt.HasValue && ExpiresAt < DateTime.UtcNow;
    public bool IsActive => Status == ApiKeyStatus.Active && !IsExpired;
}
