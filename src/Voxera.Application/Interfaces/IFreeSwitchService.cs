namespace Voxera.Application.Interfaces;

public interface IFreeSwitchService
{
    Task<bool> OriginateCallAsync(string fromExtension, string toNumber, string domain, CancellationToken ct = default);
    Task<bool> HangupCallAsync(string callId, CancellationToken ct = default);
    Task<bool> TransferCallAsync(string callId, string destination, CancellationToken ct = default);
    Task<bool> HoldCallAsync(string callId, CancellationToken ct = default);
    Task<bool> UnholdCallAsync(string callId, CancellationToken ct = default);
    Task<bool> StartRecordingAsync(string callId, string filePath, CancellationToken ct = default);
    Task<bool> StopRecordingAsync(string callId, CancellationToken ct = default);
    Task<bool> PlayAudioAsync(string callId, string audioPath, CancellationToken ct = default);
    Task<IEnumerable<ActiveCallDto>> GetActiveCallsAsync(string domain, CancellationToken ct = default);
    Task<bool> CreateSipUserAsync(string username, string password, string domain, CancellationToken ct = default);
    Task<bool> DeleteSipUserAsync(string username, string domain, CancellationToken ct = default);
    Task<bool> ReloadXmlAsync(CancellationToken ct = default);
}

public record ActiveCallDto(
    string CallId,
    string CallerNumber,
    string CalleeNumber,
    string State,
    int DurationSeconds
);
