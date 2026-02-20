using System.Text;
using System.Text.Json;
using BankMore.CurrentAccount.Application.Interfaces;
using BankMore.CurrentAccount.Domain.Entities;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BankMore.CurrentAccount.Infrastructure.Kafka;

public class FeesCompletedConsumerService : BackgroundService
{
    private readonly KafkaOptions _options;
    private readonly IIdempotencyKafkaRepository _idempotency;
    private readonly ICurrentAccountRepository _accountRepository;
    private readonly IMovementRepository _movementRepository;
    private readonly ITarifasRepository _tarifasRepository;
    private readonly ILogger<FeesCompletedConsumerService> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public FeesCompletedConsumerService(
        IOptions<KafkaOptions> options,
        IIdempotencyKafkaRepository idempotency,
        ICurrentAccountRepository accountRepository,
        IMovementRepository movementRepository,
        ITarifasRepository tarifasRepository,
        ILogger<FeesCompletedConsumerService> logger)
    {
        _options = options.Value;
        _idempotency = idempotency;
        _accountRepository = accountRepository;
        _movementRepository = movementRepository;
        _tarifasRepository = tarifasRepository;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _options.BootstrapServers,
            GroupId = _options.ConsumerGroupId + "-fees",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe(_options.TopicFeesCompleted);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(TimeSpan.FromSeconds(5));
                if (result?.Message?.Value == null) continue;

                var messageId = result.Message.Key ?? result.Offset.Value.ToString();
                if (await _idempotency.ExistsAsync(messageId, stoppingToken))
                {
                    _logger.LogDebug("Fee message {MessageId} already processed, skipping.", messageId);
                    consumer.Commit(result);
                    continue;
                }

                try
                {
                    var payload = JsonSerializer.Deserialize<FeeCompletedPayload>(result.Message.Value, JsonOptions);
                    if (payload?.FeeId == null || payload.AccountIdToDebit == null || payload.Amount <= 0)
                    {
                        _logger.LogWarning("Invalid fee message, skipping.");
                        consumer.Commit(result);
                        continue;
                    }

                    var account = await _accountRepository.GetByIdAsync(payload.AccountIdToDebit, stoppingToken);
                    if (account == null || !account.Active)
                    {
                        _logger.LogWarning("Account {AccountId} not found or inactive for fee {FeeId}, sending to DLQ.", payload.AccountIdToDebit, payload.FeeId);
                        await SendToDlqAsync(result.Message.Key, result.Message.Value, result.Message.Headers, stoppingToken);
                        consumer.Commit(result);
                        continue;
                    }

                    var movementDate = payload.ProcessedAt ?? DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss");
                    var movement = new Movement
                    {
                        Id = payload.FeeId,
                        CurrentAccountId = payload.AccountIdToDebit,
                        MovementDate = movementDate,
                        Type = 'D',
                        Value = payload.Amount
                    };
                    await _movementRepository.CreateAsync(movement, stoppingToken);
                    await _tarifasRepository.CreateAsync(payload.FeeId, payload.AccountIdToDebit, movementDate, payload.Amount, stoppingToken);
                    await _idempotency.CreateAsync(messageId, stoppingToken);
                    consumer.Commit(result);

                    _logger.LogInformation("Fee {FeeId} debited from account {AccountId}, amount {Amount}.", payload.FeeId, payload.AccountIdToDebit, payload.Amount);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process fee message {MessageId}. Sending to DLQ.", messageId);
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

    private class FeeCompletedPayload
    {
        public string? FeeId { get; set; }
        public string? TransferId { get; set; }
        public string? AccountIdToDebit { get; set; }
        public decimal Amount { get; set; }
        public string? ProcessedAt { get; set; }
    }
}
