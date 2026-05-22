using Voxera.Domain.Enums;

namespace Voxera.Domain.Entities;

public class Company : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string? Domain { get; private set; }
    public string? LogoUrl { get; private set; }
    public string? Phone { get; private set; }
    public string? Email { get; private set; }
    public string? Address { get; private set; }
    public string? TaxNumber { get; private set; }
    public CompanyStatus Status { get; private set; } = CompanyStatus.Active;
    public SubscriptionPlan Plan { get; private set; } = SubscriptionPlan.Starter;
    public DateTime? PlanExpiresAt { get; private set; }
    public int MaxExtensions { get; private set; } = 10;
    public int MaxConcurrentCalls { get; private set; } = 5;
    public bool IsTrialActive { get; private set; } = true;
    public DateTime? TrialEndsAt { get; private set; }
    public string? SipDomain { get; private set; }
    public string? WebhookUrl { get; private set; }
    public string? WebhookSecret { get; private set; }
    public string? TimeZone { get; private set; } = "Europe/Istanbul";
    public string? Country { get; private set; } = "TR";
    public string? Currency { get; private set; } = "TRY";

    // Navigation
    public ICollection<User> Users { get; private set; } = new List<User>();
    public ICollection<Extension> Extensions { get; private set; } = new List<Extension>();
    public ICollection<SipAccount> SipAccounts { get; private set; } = new List<SipAccount>();
    public ICollection<CallLog> CallLogs { get; private set; } = new List<CallLog>();
    public ICollection<ApiKey> ApiKeys { get; private set; } = new List<ApiKey>();
    public ICollection<IvrMenu> IvrMenus { get; private set; } = new List<IvrMenu>();
    public ICollection<CallQueue> CallQueues { get; private set; } = new List<CallQueue>();
    public ICollection<Invoice> Invoices { get; private set; } = new List<Invoice>();

    protected Company() { }

    public static Company Create(string name, string slug, SubscriptionPlan plan = SubscriptionPlan.Starter)
    {
        var company = new Company
        {
            Name = name,
            Slug = slug.ToLowerInvariant(),
            Plan = plan,
            TrialEndsAt = DateTime.UtcNow.AddDays(14),
            SipDomain = $"{slug.ToLowerInvariant()}.sip.voxera.io"
        };
        company.AddDomainEvent(new CompanyCreatedEvent(company.Id, name));
        return company;
    }

    public void UpdatePlan(SubscriptionPlan plan, DateTime expiresAt)
    {
        Plan = plan;
        PlanExpiresAt = expiresAt;
        IsTrialActive = false;
        MaxExtensions = plan switch
        {
            SubscriptionPlan.Starter => 10,
            SubscriptionPlan.Business => 50,
            SubscriptionPlan.Enterprise => int.MaxValue,
            _ => 10
        };
        MaxConcurrentCalls = plan switch
        {
            SubscriptionPlan.Starter => 5,
            SubscriptionPlan.Business => 25,
            SubscriptionPlan.Enterprise => int.MaxValue,
            _ => 5
        };
        MarkAsUpdated();
    }

    public void UpdateWebhook(string? url, string? secret)
    {
        WebhookUrl = url;
        WebhookSecret = secret;
        MarkAsUpdated();
    }

    public void Suspend() { Status = CompanyStatus.Suspended; MarkAsUpdated(); }
    public void Activate() { Status = CompanyStatus.Active; MarkAsUpdated(); }
}

public record CompanyCreatedEvent(Guid CompanyId, string Name) : DomainEvent;
