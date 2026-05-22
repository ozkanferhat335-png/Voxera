using Voxera.Domain.Enums;

namespace Voxera.Domain.Entities;

public class SipAccount : BaseEntity
{
    public Guid CompanyId { get; private set; }
    public Guid ExtensionId { get; private set; }
    public string Username { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string Domain { get; private set; } = string.Empty;
    public string? ContactUri { get; private set; }
    public string? UserAgent { get; private set; }
    public SipAccountStatus Status { get; private set; } = SipAccountStatus.Active;
    public AgentStatus AgentStatus { get; private set; } = AgentStatus.Offline;
    public DateTime? LastRegisteredAt { get; private set; }
    public string? LastRegisteredIp { get; private set; }
    public int? LastRegisteredPort { get; private set; }
    public string? Transport { get; private set; } = "UDP";
    public bool WebRtcEnabled { get; private set; } = false;
    public string? NatIp { get; private set; }
    public int MaxSessions { get; private set; } = 2;
    public int CurrentSessions { get; private set; } = 0;
    public string? Codec { get; private set; } = "PCMU,PCMA,G729,opus";

    // Navigation
    public Company? Company { get; private set; }
    public Extension? Extension { get; private set; }

    protected SipAccount() { }

    public static SipAccount Create(Guid companyId, Guid extensionId, string username, string passwordHash, string domain)
    {
        return new SipAccount
        {
            CompanyId = companyId,
            ExtensionId = extensionId,
            Username = username,
            PasswordHash = passwordHash,
            Domain = domain
        };
    }

    public void UpdateRegistration(string contactUri, string ip, int port, string? userAgent)
    {
        ContactUri = contactUri;
        LastRegisteredAt = DateTime.UtcNow;
        LastRegisteredIp = ip;
        LastRegisteredPort = port;
        UserAgent = userAgent;
        AgentStatus = AgentStatus.Available;
        MarkAsUpdated();
    }

    public void SetAgentStatus(AgentStatus status) { AgentStatus = status; MarkAsUpdated(); }
    public void IncrementSessions() { CurrentSessions++; MarkAsUpdated(); }
    public void DecrementSessions() { if (CurrentSessions > 0) CurrentSessions--; MarkAsUpdated(); }
    public void Deactivate() { Status = SipAccountStatus.Inactive; AgentStatus = AgentStatus.Offline; MarkAsUpdated(); }
    public void EnableWebRtc() { WebRtcEnabled = true; Transport = "WSS"; MarkAsUpdated(); }
}
