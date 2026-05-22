using Voxera.Domain.Enums;

namespace Voxera.Domain.Entities;

public class Extension : BaseEntity
{
    public Guid CompanyId { get; private set; }
    public Guid? UserId { get; private set; }
    public string Number { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public ExtensionType Type { get; private set; } = ExtensionType.User;
    public ExtensionStatus Status { get; private set; } = ExtensionStatus.Active;
    public bool VoicemailEnabled { get; private set; } = true;
    public string? VoicemailPin { get; private set; }
    public string? ForwardTo { get; private set; }
    public ForwardCondition ForwardCondition { get; private set; } = ForwardCondition.None;
    public int? MaxCallDuration { get; private set; }
    public bool RecordCalls { get; private set; } = false;
    public string? CallerIdName { get; private set; }
    public string? CallerIdNumber { get; private set; }
    public int RingTimeout { get; private set; } = 30;
    public bool DoNotDisturb { get; private set; } = false;

    // Navigation
    public Company? Company { get; private set; }
    public User? User { get; private set; }
    public SipAccount? SipAccount { get; private set; }
    public ICollection<CallLog> InboundCalls { get; private set; } = new List<CallLog>();
    public ICollection<CallLog> OutboundCalls { get; private set; } = new List<CallLog>();

    protected Extension() { }

    public static Extension Create(Guid companyId, string number, string displayName, ExtensionType type = ExtensionType.User)
    {
        return new Extension
        {
            CompanyId = companyId,
            Number = number,
            DisplayName = displayName,
            Type = type
        };
    }

    public void AssignUser(Guid userId) { UserId = userId; MarkAsUpdated(); }
    public void SetForward(string? forwardTo, ForwardCondition condition) { ForwardTo = forwardTo; ForwardCondition = condition; MarkAsUpdated(); }
    public void SetDoNotDisturb(bool enabled) { DoNotDisturb = enabled; MarkAsUpdated(); }
    public void SetVoicemail(bool enabled, string? pin) { VoicemailEnabled = enabled; VoicemailPin = pin; MarkAsUpdated(); }
    public void SetCallRecording(bool enabled) { RecordCalls = enabled; MarkAsUpdated(); }
    public void Deactivate() { Status = ExtensionStatus.Inactive; MarkAsUpdated(); }
    public void Activate() { Status = ExtensionStatus.Active; MarkAsUpdated(); }
}
