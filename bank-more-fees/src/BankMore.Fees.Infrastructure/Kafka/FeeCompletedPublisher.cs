using System.Text;
using System.Text.Json;
using BankMore.Fees.Application.Interfaces;
using BankMore.Fees.Application.Messages;
using BankMore.Fees.Infrastructure.Options;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BankMore.Fees.Infrastructure.Kafka;

public class FeeCompletedPublisher : IFeeCompletedPublisher
{
    private readonly KafkaOptions _options;
    private readonly ILogger<FeeCompletedPublisher> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public FeeCompletedPublisher(IOptions<KafkaOptions> options, ILogger<FeeCompletedPublisher> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task PublishAsync(FeeCompletedMessage message, string? correlationId, CancellationToken cancellationToken = default)
    {
        var config = new ProducerConfig { BootstrapServers = _options.BootstrapServers };
        using var producer = new ProducerBuilder<string, string>(config).Build();
        var payload = JsonSerializer.Serialize(message, JsonOptions);
        var kafkaMessage = new Message<string, string>
        {
            Key = message.FeeId,
            Value = payload,
            Headers = new Headers()
        };
        if (!string.IsNullOrEmpty(correlationId))
            kafkaMessage.Headers.Add("Correlation-Id", Encoding.UTF8.GetBytes(correlationId));

        await producer.ProduceAsync(_options.TopicFeesCompleted, kafkaMessage, cancellationToken);
        _logger.LogInformation("Published fee completed {FeeId} for transfer {TransferId}, amount {Amount}. CorrelationId: {CorrelationId}",
            message.FeeId, message.TransferId, message.Amount, correlationId ?? "(none)");
    }
}
