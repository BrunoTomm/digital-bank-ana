using System.Text.Json;
using BankMore.CurrentAccount.Application.Interfaces;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BankMore.CurrentAccount.Infrastructure.Kafka;

public class TransfersCompletedConsumerService : BackgroundService
{
    private readonly KafkaOptions _options;
    private readonly IIdempotencyKafkaRepository _idempotency;
    private readonly ILogger<TransfersCompletedConsumerService> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public TransfersCompletedConsumerService(
        IOptions<KafkaOptions> options,
        IIdempotencyKafkaRepository idempotency,
        ILogger<TransfersCompletedConsumerService> logger)
    {
        _options = options.Value;
        _idempotency = idempotency;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (string.IsNullOrEmpty(_options.BootstrapServers))
        {
            _logger.LogError("Kafka BootstrapServers is not configured. CurrentAccount Transfers consumer will not start.");
            return;
        }

        var config = new ConsumerConfig
        {
            BootstrapServers = _options.BootstrapServers,
            GroupId = _options.ConsumerGroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        _logger.LogInformation("CurrentAccount Kafka consumer (transfers) starting. BootstrapServers={BootstrapServers}, Topic={Topic}, GroupId={GroupId}",
            _options.BootstrapServers, _options.TopicTransfersCompleted, _options.ConsumerGroupId);

        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe(_options.TopicTransfersCompleted);
        _logger.LogInformation("CurrentAccount subscribed to {Topic}. Waiting for messages...", _options.TopicTransfersCompleted);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(TimeSpan.FromSeconds(5));
                if (result?.Message?.Value == null) continue;

                var messageId = result.Message.Key ?? result.Offset.Value.ToString();
                if (await _idempotency.ExistsAsync(messageId, stoppingToken))
                {
                    _logger.LogDebug("Kafka message {MessageId} already processed, skipping.", messageId);
                    consumer.Commit(result);
                    continue;
                }

                string? correlationId = null;
                if (result.Message.Headers != null)
                {
                    var corrHeader = result.Message.Headers.FirstOrDefault(h => string.Equals(h.Key, "Correlation-Id", StringComparison.OrdinalIgnoreCase));
                    if (corrHeader != null && corrHeader.GetValueBytes() != null)
                        correlationId = System.Text.Encoding.UTF8.GetString(corrHeader.GetValueBytes());
                }

                try
                {
                    var payload = JsonSerializer.Deserialize<TransferCompletedPayload>(result.Message.Value, JsonOptions);
                    _logger.LogInformation("Processed transfer completed: {TransferId} from {Origin} to {Destination}, amount {Amount}. CorrelationId: {CorrelationId}",
                        payload?.TransferId, payload?.OriginAccountId, payload?.DestinationAccountId, payload?.Value, correlationId ?? "(none)");
                    await _idempotency.CreateAsync(messageId, stoppingToken);
                    consumer.Commit(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process Kafka message {MessageId}. Sending to DLQ.", messageId);
                    await SendToDlqAsync(result.Message.Key, result.Message.Value, result.Message.Headers, stoppingToken);
                    consumer.Commit(result);
                }
            }
            catch (ConsumeException ex) when (ex.Error.IsError && ex.Error.Code == ErrorCode.Local_TimedOut)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kafka consumer error.");
            }

            await Task.Delay(100, stoppingToken);
        }
    }

    private async Task SendToDlqAsync(string? key, string value, Headers? headers, CancellationToken ct)
    {
        try
        {
            var config = new ProducerConfig { BootstrapServers = _options.BootstrapServers };
            using var producer = new ProducerBuilder<string, string>(config).Build();
            var msg = new Message<string, string> { Key = key, Value = value, Headers = headers ?? new Headers() };
            await producer.ProduceAsync(_options.TopicDlq, msg, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send message to DLQ {Topic}.", _options.TopicDlq);
        }
    }

    private class TransferCompletedPayload
    {
        public string? TransferId { get; set; }
        public string? OriginAccountId { get; set; }
        public string? DestinationAccountId { get; set; }
        public decimal Value { get; set; }
        public string? MovementDate { get; set; }
    }
}
