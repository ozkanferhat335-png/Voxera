namespace Voxera.Domain.Enums;

public enum UserRole
{
    SuperAdmin = 1,
    CompanyAdmin = 2,
    Operator = 3,
    Accounting = 4,
    TechnicalSupport = 5
}

public enum UserStatus
{
    Active = 1,
    Inactive = 2,
    Suspended = 3,
    PendingVerification = 4
}

public enum CompanyStatus
{
    Active = 1,
    Suspended = 2,
    Cancelled = 3,
    PendingSetup = 4
}

public enum SubscriptionPlan
{
    Trial = 0,
    Starter = 1,
    Business = 2,
    Enterprise = 3
}

public enum ExtensionType
{
    User = 1,
    Queue = 2,
    IVR = 3,
    Conference = 4,
    Voicemail = 5,
    Trunk = 6
}

public enum ExtensionStatus
{
    Active = 1,
    Inactive = 2,
    Busy = 3
}

public enum ForwardCondition
{
    None = 0,
    Always = 1,
    Busy = 2,
    NoAnswer = 3,
    Offline = 4
}

public enum SipAccountStatus
{
    Active = 1,
    Inactive = 2,
    Suspended = 3
}

public enum AgentStatus
{
    Offline = 0,
    Available = 1,
    Busy = 2,
    Away = 3,
    DoNotDisturb = 4,
    WrapUp = 5
}

public enum CallDirection
{
    Inbound = 1,
    Outbound = 2,
    Internal = 3
}

public enum CallStatus
{
    Initiated = 1,
    Ringing = 2,
    Active = 3,
    OnHold = 4,
    Completed = 5,
    Missed = 6,
    Failed = 7,
    Busy = 8,
    Cancelled = 9
}

public enum CallEndReason
{
    Normal = 1,
    NoAnswer = 2,
    Busy = 3,
    Failed = 4,
    Cancelled = 5,
    Transferred = 6,
    Timeout = 7
}

public enum ApiKeyStatus
{
    Active = 1,
    Revoked = 2,
    Expired = 3
}

public enum IvrActionType
{
    Extension = 1,
    Queue = 2,
    IvrMenu = 3,
    Voicemail = 4,
    Hangup = 5,
    ExternalNumber = 6,
    Announcement = 7
}

public enum QueueStrategy
{
    RoundRobin = 1,
    LeastRecent = 2,
    FewestCalls = 3,
    Random = 4,
    Priority = 5
}

public enum QueueOverflowAction
{
    Voicemail = 1,
    Extension = 2,
    IvrMenu = 3,
    Hangup = 4,
    ExternalNumber = 5
}

public enum InvoiceType
{
    Subscription = 1,
    Usage = 2,
    OneTime = 3,
    Credit = 4
}

public enum InvoiceStatus
{
    Draft = 1,
    Sent = 2,
    Paid = 3,
    Overdue = 4,
    Cancelled = 5
}

public enum SentimentType
{
    Positive = 1,
    Neutral = 2,
    Negative = 3
}

public enum WebhookEventType
{
    IncomingCall = 1,
    CallAnswered = 2,
    CallEnded = 3,
    MissedCall = 4,
    RecordingReady = 5,
    AgentStatusChanged = 6,
    VoicemailReceived = 7
}
