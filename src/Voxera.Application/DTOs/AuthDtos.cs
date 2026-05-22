namespace Voxera.Application.DTOs;

public record AuthResponseDto(
    string AccessToken,
    string RefreshToken,
    Guid UserId,
    string FullName,
    string Email,
    string Role,
    Guid CompanyId,
    string CompanyName
);

public record SipAccountDto(
    Guid Id,
    string Username,
    string Domain,
    string Status,
    string AgentStatus,
    string? PlainPassword = null
);

public record CallInitiatedDto(
    string CallId,
    string From,
    string To,
    string Direction,
    DateTime StartedAt
);

public record CallLogDto(
    Guid Id,
    string CallId,
    string? CallerNumber,
    string? CallerName,
    string? CalleeNumber,
    string Direction,
    string Status,
    DateTime StartedAt,
    DateTime? AnsweredAt,
    DateTime? EndedAt,
    int? DurationSeconds,
    bool IsRecorded,
    string? RecordingPath,
    string? AiSummary,
    string? Sentiment
);

public record DashboardStatsDto(
    int TotalCallsToday,
    int ActiveCalls,
    int MissedCallsToday,
    int TotalAgents,
    int AvailableAgents,
    int BusyAgents,
    double AverageCallDuration,
    int TotalExtensions,
    List<HourlyCallStatDto> HourlyStats
);

public record HourlyCallStatDto(int Hour, int TotalCalls, int AnsweredCalls, int MissedCalls);

public record ExtensionDto(
    Guid Id,
    string Number,
    string DisplayName,
    string Type,
    string Status,
    bool VoicemailEnabled,
    bool RecordCalls,
    bool DoNotDisturb,
    string? ForwardTo,
    string? AgentStatus
);

public record CompanyDto(
    Guid Id,
    string Name,
    string Slug,
    string Status,
    string Plan,
    DateTime? PlanExpiresAt,
    int MaxExtensions,
    int MaxConcurrentCalls,
    string? SipDomain,
    string? WebhookUrl
);

public record UserDto(
    Guid Id,
    string FullName,
    string Email,
    string Role,
    string Status,
    DateTime? LastLoginAt,
    Guid CompanyId
);

public record ApiKeyDto(
    Guid Id,
    string Name,
    string KeyPrefix,
    string Status,
    string[] Permissions,
    DateTime? ExpiresAt,
    DateTime? LastUsedAt,
    long RequestCount
);

public record WebhookPayloadDto(
    string EventType,
    Guid CompanyId,
    DateTime Timestamp,
    object Data
);

public record PagedResultDto<T>(
    IEnumerable<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);
