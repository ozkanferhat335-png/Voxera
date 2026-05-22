using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Voxera.Application.Interfaces;
using Voxera.Domain.Enums;
using Voxera.Infrastructure.Settings;

namespace Voxera.Infrastructure.AI;

public class AiService : IAiService
{
    private readonly AiSettings _settings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AiService> _logger;

    public AiService(IOptions<AiSettings> settings, IHttpClientFactory httpClientFactory, ILogger<AiService> logger)
    {
        _settings = settings.Value;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<string?> TranscribeAudioAsync(string audioFilePath, string language = "tr", CancellationToken ct = default)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("openai");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _settings.OpenAiApiKey);

            using var form = new MultipartFormDataContent();
            var fileBytes = await File.ReadAllBytesAsync(audioFilePath, ct);
            form.Add(new ByteArrayContent(fileBytes), "file", Path.GetFileName(audioFilePath));
            form.Add(new StringContent(_settings.WhisperModel), "model");
            form.Add(new StringContent(language), "language");
            form.Add(new StringContent("text"), "response_format");

            var response = await client.PostAsync("https://api.openai.com/v1/audio/transcriptions", form, ct);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to transcribe audio: {Path}", audioFilePath);
            return null;
        }
    }

    public async Task<string?> SummarizeCallAsync(string transcript, CancellationToken ct = default)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("openai");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _settings.OpenAiApiKey);

            var requestBody = new
            {
                model = _settings.GptModel,
                messages = new[]
                {
                    new { role = "system", content = "Sen bir çağrı merkezi asistanısın. Verilen çağrı transkriptini Türkçe olarak kısaca özetle. Müşterinin sorunu, çözüm ve sonuç hakkında bilgi ver." },
                    new { role = "user", content = transcript }
                },
                max_tokens = 500,
                temperature = 0.3
            };

            var json = JsonSerializer.Serialize(requestBody);
            var response = await client.PostAsync("https://api.openai.com/v1/chat/completions",
                new StringContent(json, Encoding.UTF8, "application/json"), ct);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync(ct);
            var doc = JsonDocument.Parse(result);
            return doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to summarize call");
            return null;
        }
    }

    public async Task<SentimentType> AnalyzeSentimentAsync(string transcript, CancellationToken ct = default)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("openai");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _settings.OpenAiApiKey);

            var requestBody = new
            {
                model = _settings.GptModel,
                messages = new[]
                {
                    new { role = "system", content = "Verilen metni analiz et ve duygu durumunu belirle. Sadece 'Positive', 'Neutral' veya 'Negative' yaz." },
                    new { role = "user", content = transcript }
                },
                max_tokens = 10,
                temperature = 0
            };

            var json = JsonSerializer.Serialize(requestBody);
            var response = await client.PostAsync("https://api.openai.com/v1/chat/completions",
                new StringContent(json, Encoding.UTF8, "application/json"), ct);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync(ct);
            var doc = JsonDocument.Parse(result);
            var sentiment = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString()?.Trim();

            return sentiment?.ToLower() switch
            {
                "positive" => SentimentType.Positive,
                "negative" => SentimentType.Negative,
                _ => SentimentType.Neutral
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to analyze sentiment");
            return SentimentType.Neutral;
        }
    }

    public async Task<string?> CreateTicketFromCallAsync(string summary, string callerNumber, string companyName, CancellationToken ct = default)
    {
        // In production: integrate with ticketing system (Jira, Zendesk, etc.)
        _logger.LogInformation("Creating ticket for call from {Caller} at {Company}", callerNumber, companyName);
        return await Task.FromResult($"TICKET-{DateTime.UtcNow:yyyyMMddHHmmss}");
    }
}
