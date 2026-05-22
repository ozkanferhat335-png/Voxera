using Voxera.Domain.Enums;

namespace Voxera.Application.Interfaces;

public interface IAiService
{
    Task<string?> TranscribeAudioAsync(string audioFilePath, string language = "tr", CancellationToken ct = default);
    Task<string?> SummarizeCallAsync(string transcript, CancellationToken ct = default);
    Task<SentimentType> AnalyzeSentimentAsync(string transcript, CancellationToken ct = default);
    Task<string?> CreateTicketFromCallAsync(string summary, string callerNumber, string companyName, CancellationToken ct = default);
}
