namespace Voxera.Domain.Entities;

public class AuditLog : BaseEntity
{
    public Guid? CompanyId { get; private set; }
    public Guid? UserId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string EntityType { get; private set; } = string.Empty;
    public string? EntityId { get; private set; }
    public string? OldValues { get; private set; }
    public string? NewValues { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public bool IsSuccess { get; private set; } = true;
    public string? ErrorMessage { get; private set; }
    public string? AdditionalData { get; private set; }

    // Navigation
    public User? User { get; private set; }

    protected AuditLog() { }

    public static AuditLog Create(Guid? companyId, Guid? userId, string action, string entityType, string? entityId = null)
    {
        return new AuditLog
        {
            CompanyId = companyId,
            UserId = userId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId
        };
    }

    public void SetChanges(string? oldValues, string? newValues) { OldValues = oldValues; NewValues = newValues; }
    public void SetRequest(string? ip, string? userAgent) { IpAddress = ip; UserAgent = userAgent; }
    public void MarkFailed(string error) { IsSuccess = false; ErrorMessage = error; }
}
