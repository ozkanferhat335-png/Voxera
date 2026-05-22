using Voxera.Domain.Enums;

namespace Voxera.Domain.Entities;

public class CallLog : BaseEntity
{
    public Guid CompanyId { get; private set; }
    public string CallId { get; private set; } = string.Empty;  // FreeSWITCH UUID
    public string? CallerNumber { get; private set; }
    public string? CallerName { get; private set; }
    public string? CalleeNumber { get; private set; }
    public string? CalleeName { get; private set; }
    public Guid? FromExtensionId { get; private set; }
    public Guid? ToExtensionId { get; private set; }
    public CallDirection Direction { get; private set; }
    public CallStatus Status { get; private set; } = CallStatus.Initiated;
    public CallEndReason? EndReason { get; private set; }
    public DateTime StartedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? AnsweredAt { get; private set; }
    public DateTime? EndedAt { get; private set; }
    public int? DurationSeconds { get; private set; }
    public int? BillableSeconds { get; private set; }
    public int? RingDurationSeconds { get; private set; }
    public bool IsRecorded { get; private set; } = false;
    public string? RecordingPath { get; private set; }
    public long? RecordingSize { get; private set; }
    public string? SipTrunkId { get; private set; }
    public string? IvrPath { get; private set; }
    public Guid? QueueId { get; private set; }
    public string? Notes { get; private set; }
    public string? Tags { get; private set; }
    public bool IsTransferred { get; private set; } = false;
    public string? TransferredTo { get; private set; }
    public decimal? Cost { get; private set; }
    public string? AiSummary { get; private set; }
    public string? Transcript { get; private set; }
    public SentimentType? Sentiment { get; private set; }

    // Navigation
    public Company? Company { get; private set; }
    public Extension? FromExtension { get; private set; }
    public Extension? ToExtension { get; private set; }
    public CallQueue? Queue { get; private set; }

    protected CallLog() { }

    public static CallLog Create(Guid companyId, string callId, string? callerNumber, string? calleeNumber, CallDirection direction)
    {
        return new CallLog
        {
            CompanyId = companyId,
            CallId = callId,
            CallerNumber = callerNumber,
            CalleeNumber = calleeNumber,
            Direction = direction,
            StartedAt = DateTime.UtcNow
        };
    }

    public void MarkAnswered()
    {
        AnsweredAt = DateTime.UtcNow;
        Status = CallStatus.Active;
        RingDurationSeconds = (int)(AnsweredAt.Value - StartedAt).TotalSeconds;
        MarkAsUpdated();
    }

    public void MarkEnded(CallEndReason reason)
    {
        EndedAt = DateTime.UtcNow;
        EndReason = reason;
        Status = reason == CallEndReason.NoAnswer ? CallStatus.Missed : CallStatus.Completed;
        if (AnsweredAt.HasValue)
            DurationSeconds = (int)(EndedAt.Value - AnsweredAt.Value).TotalSeconds;
        MarkAsUpdated();
    }

    public void SetRecording(string path, long size)
    {
        IsRecorded = true;
        RecordingPath = path;
        RecordingSize = size;
        MarkAsUpdated();
    }

    public void SetAiAnalysis(string? summary, string? transcript, SentimentType? sentiment)
    {
        AiSummary = summary;
        Transcript = transcript;
        Sentiment = sentiment;
        MarkAsUpdated();
    }

    public void Transfer(string transferredTo)
    {
        IsTransferred = true;
        TransferredTo = transferredTo;
        MarkAsUpdated();
    }

    public void AddNote(string note) { Notes = note; MarkAsUpdated(); }
}
