using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Voxera.Infrastructure.Settings;

namespace Voxera.Worker.Consumers;

/// <summary>
/// RabbitMQ consumer service that processes async messages:
/// - Call events from FreeSWITCH
/// - Webhook delivery retries
/// - AI processing queue
/// </summary>
public class RabbitMqConsumerService : BackgroundService
{
    private readonly RabbitMqSettings _settings;
    private readonly ILogger<RabbitMqConsumerService> _logger;
    private IConnection? _connection;
    private IModel? _channel;

    public RabbitMqConsumerService(IOptions<RabbitMqSettings> settings, ILogger<RabbitMqConsumerService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _settings.Host,
                Port = _settings.Port,
                UserName = _settings.Username,
                Password = _settings.Password,
                VirtualHost = _settings.VirtualHost,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            _connection = factory.CreateConnection("voxera-worker");
            _channel = _connection.CreateModel();

            // Declare queues
            _channel.QueueDeclare("voxera.call.events", durable: true, exclusive: false, autoDelete: false);
            _channel.QueueDeclare("voxera.webhook.retry", durable: true, exclusive: false, autoDelete: false);
            _channel.QueueDeclare("voxera.ai.processing", durable: true, exclusive: false, autoDelete: false);

            _channel.BasicQos(0, 10, false);

            // Call events consumer
            var callConsumer = new EventingBasicConsumer(_channel);
            callConsumer.Received += async (_, ea) =>
            {
                try
                {
                    var body = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var message = JsonSerializer.Deserialize<Dictionary<string, object>>(body);
                    _logger.LogInformation("Processing call event: {EventType}", message?["event_type"]);
                    _channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process call event");
                    _channel.BasicNack(ea.DeliveryTag, false, true);
                }
            };

            _channel.BasicConsume("voxera.call.events", false, callConsumer);

            _logger.LogInformation("RabbitMQ consumer started, listening on queues");

            while (!stoppingToken.IsCancellationRequested)
                await Task.Delay(1000, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RabbitMQ consumer failed to start");
        }
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }
}
