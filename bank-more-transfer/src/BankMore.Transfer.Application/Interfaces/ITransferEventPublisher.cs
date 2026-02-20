namespace BankMore.Transfer.Application.Interfaces;

public interface ITransferEventPublisher
{
    Task PublishAsync(TransferCompletedMessage message, string? correlationId, CancellationToken cancellationToken = default);
}

public record TransferCompletedMessage(string TransferId, string OriginAccountId, string DestinationAccountId, decimal Value, string MovementDate);
