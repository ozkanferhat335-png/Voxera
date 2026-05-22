using Microsoft.EntityFrameworkCore;
using Quartz;
using Serilog;
using Voxera.Application.Interfaces;
using Voxera.Infrastructure.AI;
using Voxera.Infrastructure.FreeSWITCH;
using Voxera.Infrastructure.Persistence;
using Voxera.Infrastructure.Services;
using Voxera.Infrastructure.Settings;
using Voxera.Worker.Jobs;

var builder = Host.CreateApplicationBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .CreateLogger();

builder.Services.AddSerilog();

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

// Settings
builder.Services.Configure<AiSettings>(builder.Configuration.GetSection("AI"));
builder.Services.Configure<FreeSwitchSettings>(builder.Configuration.GetSection("FreeSWITCH"));
builder.Services.Configure<RedisSettings>(builder.Configuration.GetSection("Redis"));
builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection("RabbitMQ"));

// Services
builder.Services.AddSingleton<ICacheService, RedisCacheService>();
builder.Services.AddSingleton<IFreeSwitchService, FreeSwitchService>();
builder.Services.AddScoped<IAiService, AiService>();
builder.Services.AddScoped<IWebhookService, WebhookService>();
builder.Services.AddHttpClient("openai");
builder.Services.AddHttpClient("webhook");

// Quartz scheduler
builder.Services.AddQuartz(q =>
{
    // AI processing job - runs every 5 minutes
    var aiJobKey = new JobKey("AiProcessingJob");
    q.AddJob<AiProcessingJob>(opts => opts.WithIdentity(aiJobKey));
    q.AddTrigger(opts => opts
        .ForJob(aiJobKey)
        .WithIdentity("AiProcessingJob-trigger")
        .WithCronSchedule("0 */5 * * * ?"));

    // Invoice generation job - runs daily at midnight
    var invoiceJobKey = new JobKey("InvoiceGenerationJob");
    q.AddJob<InvoiceGenerationJob>(opts => opts.WithIdentity(invoiceJobKey));
    q.AddTrigger(opts => opts
        .ForJob(invoiceJobKey)
        .WithIdentity("InvoiceGenerationJob-trigger")
        .WithCronSchedule("0 0 0 * * ?"));

    // Cleanup old recordings - runs weekly
    var cleanupJobKey = new JobKey("RecordingCleanupJob");
    q.AddJob<RecordingCleanupJob>(opts => opts.WithIdentity(cleanupJobKey));
    q.AddTrigger(opts => opts
        .ForJob(cleanupJobKey)
        .WithIdentity("RecordingCleanupJob-trigger")
        .WithCronSchedule("0 0 2 ? * SUN"));
});

builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

// RabbitMQ consumer
builder.Services.AddHostedService<RabbitMqConsumerService>();

var host = builder.Build();
Log.Information("Voxera Worker starting...");
await host.RunAsync();
