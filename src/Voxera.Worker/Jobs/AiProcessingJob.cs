using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;
using Voxera.Application.Interfaces;
using Voxera.Domain.Enums;

namespace Voxera.Worker.Jobs;

/// <summary>
/// Background job that processes completed call recordings with AI:
/// - Transcribes audio using Whisper
/// - Generates call summary using GPT
/// - Analyzes sentiment
/// </summary>
[DisallowConcurrentExecution]
public class AiProcessingJob : IJob
{
    private readonly IApplicationDbContext _db;
    private readonly IAiService _aiService;
    private readonly ILogger<AiProcessingJob> _logger;

    public AiProcessingJob(IApplicationDbContext db, IAiService aiService, ILogger<AiProcessingJob> logger)
    {
        _db = db;
        _aiService = aiService;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        // Find recorded calls that haven't been processed by AI yet
        var unprocessedCalls = await _db.CallLogs
            .Where(c => c.IsRecorded && c.RecordingPath != null && c.AiSummary == null && c.Status == CallStatus.Completed)
            .Take(10)  // Process in batches
            .ToListAsync(context.CancellationToken);

        if (!unprocessedCalls.Any())
        {
            _logger.LogDebug("No unprocessed recordings found");
            return;
        }

        _logger.LogInformation("Processing {Count} recordings with AI", unprocessedCalls.Count);

        foreach (var call in unprocessedCalls)
        {
            try
            {
                if (!File.Exists(call.RecordingPath)) continue;

                // Transcribe
                var transcript = await _aiService.TranscribeAudioAsync(call.RecordingPath!, "tr", context.CancellationToken);
                if (transcript is null) continue;

                // Summarize
                var summary = await _aiService.SummarizeCallAsync(transcript, context.CancellationToken);

                // Sentiment analysis
                var sentiment = await _aiService.AnalyzeSentimentAsync(transcript, context.CancellationToken);

                call.SetAiAnalysis(summary, transcript, sentiment);
                _logger.LogInformation("AI processed call {CallId}: sentiment={Sentiment}", call.CallId, sentiment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to AI-process call {CallId}", call.CallId);
            }
        }

        await _db.SaveChangesAsync(context.CancellationToken);
    }
}

public class InvoiceGenerationJob : IJob
{
    private readonly IApplicationDbContext _db;
    private readonly ILogger<InvoiceGenerationJob> _logger;

    public InvoiceGenerationJob(IApplicationDbContext db, ILogger<InvoiceGenerationJob> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Running invoice generation job");
        // TODO: Generate monthly invoices for active subscriptions
        await Task.CompletedTask;
    }
}

public class RecordingCleanupJob : IJob
{
    private readonly IApplicationDbContext _db;
    private readonly ILogger<RecordingCleanupJob> _logger;

    public RecordingCleanupJob(IApplicationDbContext db, ILogger<RecordingCleanupJob> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        // Delete recordings older than 90 days (configurable per company plan)
        var cutoffDate = DateTime.UtcNow.AddDays(-90);
        var oldRecordings = await _db.CallLogs
            .Where(c => c.IsRecorded && c.EndedAt < cutoffDate && c.RecordingPath != null)
            .ToListAsync(context.CancellationToken);

        foreach (var call in oldRecordings)
        {
            try
            {
                if (File.Exists(call.RecordingPath))
                    File.Delete(call.RecordingPath!);
                _logger.LogInformation("Deleted old recording: {Path}", call.RecordingPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete recording: {Path}", call.RecordingPath);
            }
        }
    }
}
