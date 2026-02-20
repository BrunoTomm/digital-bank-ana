using System.Text.Json;
using BankMore.Transfer.Application.Interfaces;
using BankMore.Transfer.Infrastructure.Options;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BankMore.Transfer.Infrastructure.Kafka;

public class TransferEventPublisher : ITransferEventPublisher
{
    private readonly KafkaOptions _options;
    private readonly ILogger<TransferEventPublisher> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public TransferEventPublisher(IOptions<KafkaOptions> options, ILogger<TransferEventPublisher> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task PublishAsync(TransferCompletedMessage message, string? correlationId, CancellationToken cancellationToken = default)
    {
        try
        {
            var config = new ProducerConfig
            {
                BootstrapServers = _options.BootstrapServers,
                MessageTimeoutMs = 5000,
                RequestTimeoutMs = 5000
            };
            using var producer = new ProducerBuilder<string, string>(config).Build();
            var payload = JsonSerializer.Serialize(message, JsonOptions);
            var kafkaMessage = new Message<string, string>
            {
                Key = message.TransferId,
                Value = payload,
                Headers = new Headers()
            };
            if (!string.IsNullOrEmpty(correlationId))
                kafkaMessage.Headers.Add("Correlation-Id", System.Text.Encoding.UTF8.GetBytes(correlationId));

            await producer.ProduceAsync(_options.TopicTransfersCompleted, kafkaMessage, cancellationToken);
            _logger.LogInformation("Published transfer completed {TransferId} to {Topic}. CorrelationId: {CorrelationId}",
                message.TransferId, _options.TopicTransfersCompleted, correlationId ?? "(none)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish transfer completed {TransferId} to Kafka.", message.TransferId);
        }
    }
}
