using Voxera.Domain.Enums;

namespace Voxera.Domain.Entities;

public class CallQueue : BaseEntity
{
    public Guid CompanyId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string Extension { get; private set; } = string.Empty;
    public QueueStrategy Strategy { get; private set; } = QueueStrategy.RoundRobin;
    public int MaxWaitTime { get; private set; } = 300;  // seconds
    public int MaxQueueSize { get; private set; } = 50;
    public string? MusicOnHoldPath { get; private set; }
    public string? WelcomeAudioPath { get; private set; }
    public int AnnounceInterval { get; private set; } = 60;  // seconds
    public bool AnnouncePosition { get; private set; } = true;
    public bool AnnounceWaitTime { get; private set; } = true;
    public string? OverflowTarget { get; private set; }
    public QueueOverflowAction OverflowAction { get; private set; } = QueueOverflowAction.Voicemail;
    public bool IsActive { get; private set; } = true;

    // Navigation
    public Company? Company { get; private set; }
    public ICollection<QueueAgent> Agents { get; private set; } = new List<QueueAgent>();
    public ICollection<CallLog> CallLogs { get; private set; } = new List<CallLog>();

    protected CallQueue() { }

    public static CallQueue Create(Guid companyId, string name, string extension)
    {
        return new CallQueue { CompanyId = companyId, Name = name, Extension = extension };
    }
}

public class QueueAgent : BaseEntity
{
    public Guid QueueId { get; private set; }
    public Guid ExtensionId { get; private set; }
    public int Priority { get; private set; } = 1;
    public int Penalty { get; private set; } = 0;
    public bool IsActive { get; private set; } = true;
    public int WrapUpTime { get; private set; } = 10;  // seconds after call

    public CallQueue? Queue { get; private set; }
    public Extension? Extension { get; private set; }

    protected QueueAgent() { }

    public static QueueAgent Create(Guid queueId, Guid extensionId, int priority = 1)
    {
        return new QueueAgent { QueueId = queueId, ExtensionId = extensionId, Priority = priority };
    }
}
