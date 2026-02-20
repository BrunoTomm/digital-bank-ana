using System.Text;
using System.Text.Json;
using BankMore.Fees.Application.Interfaces;
using BankMore.Fees.Application.Messages;
using BankMore.Fees.Infrastructure.Options;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BankMore.Fees.Infrastructure.Kafka;

public class TransfersCompletedConsumerService : BackgroundService
{
    private readonly KafkaOptions _kafkaOptions;
    private readonly FeeOptions _feeOptions;
    private readonly IIdempotencyRepository _idempotency;
    private readonly IFeeCompletedPublisher _feePublisher;
    private readonly ILogger<TransfersCompletedConsumerService> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public TransfersCompletedConsumerService(
        IOptions<KafkaOptions> kafkaOptions,
        IOptions<FeeOptions> feeOptions,
        IIdempotencyRepository idempotency,
        IFeeCompletedPublisher feePublisher,
        ILogger<TransfersCompletedConsumerService> logger)
    {
        _kafkaOptions = kafkaOptions.Value;
        _feeOptions = feeOptions.Value;
        _idempotency = idempotency;
        _feePublisher = feePublisher;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (string.IsNullOrEmpty(_kafkaOptions.BootstrapServers))
        {
            _logger.LogError("Kafka BootstrapServers is not configured. Consumer will not start.");
            return;
        }

        var config = new ConsumerConfig
        {
            BootstrapServers = _kafkaOptions.BootstrapServers,
            GroupId = _kafkaOptions.ConsumerGroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        _logger.LogInformation("Fees Kafka consumer starting. BootstrapServers={BootstrapServers}, Topic={Topic}, GroupId={GroupId}",
            _kafkaOptions.BootstrapServers, _kafkaOptions.TopicTransfersCompleted, _kafkaOptions.ConsumerGroupId);

        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe(_kafkaOptions.TopicTransfersCompleted);
        _logger.LogInformation("Fees subscribed to {Topic}. Waiting for messages...", _kafkaOptions.TopicTransfersCompleted);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(TimeSpan.FromSeconds(5));
                if (result?.Message?.Value == null) continue;

                var transferId = result.Message.Key ?? result.Offset.Value.ToString();
                if (await _idempotency.ExistsAsync(transferId, stoppingToken))
                {
                    _logger.LogDebug("Transfer {TransferId} already processed for fee, skipping.", transferId);
                    consumer.Commit(result);
                    continue;
                }

                string? correlationId = null;
                if (result.Message.Headers != null)
                {
                    var corrHeader = result.Message.Headers.FirstOrDefault(h => string.Equals(h.Key, "Correlation-Id", StringComparison.OrdinalIgnoreCase));
                    if (corrHeader != null && corrHeader.GetValueBytes() != null)
                        correlationId = Encoding.UTF8.GetString(corrHeader.GetValueBytes());
                }

                try
                {
                    var payload = JsonSerializer.Deserialize<TransferCompletedPayload>(result.Message.Value, JsonOptions);
                    if (payload?.TransferId == null || payload.OriginAccountId == null)
                    {
                        _logger.LogWarning("Invalid transfer message, skipping.");
                        consumer.Commit(result);
                        continue;
                    }

                    decimal feeAmount = _feeOptions.FixedAmountPerTransfer;
                    var feeId = Guid.NewGuid().ToString();
                    var processedAt = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss");

                    var feeMessage = new FeeCompletedMessage(
                        FeeId: feeId,
                        TransferId: payload.TransferId,
                        AccountIdToDebit: payload.OriginAccountId,
                        Amount: feeAmount,
                        ProcessedAt: processedAt);

                    await _feePublisher.PublishAsync(feeMessage, correlationId, stoppingToken);
                    await _idempotency.MarkProcessedAsync(transferId, stoppingToken);
                    consumer.Commit(result);

                    _logger.LogInformation("Fee {FeeId} for transfer {TransferId}, amount {Amount} from account {AccountId}. CorrelationId: {CorrelationId}",
                        feeId, payload.TransferId, feeAmount, payload.OriginAccountId, correlationId ?? "(none)");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process transfer {TransferId} for fee.", transferId);
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

    private class TransferCompletedPayload
    {
        public string? TransferId { get; set; }
        public string? OriginAccountId { get; set; }
        public string? DestinationAccountId { get; set; }
        public decimal Value { get; set; }
        public string? MovementDate { get; set; }
    }
}
