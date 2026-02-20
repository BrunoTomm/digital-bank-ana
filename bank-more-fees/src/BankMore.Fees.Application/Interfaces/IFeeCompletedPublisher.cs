using BankMore.Fees.Application.Messages;

namespace BankMore.Fees.Application.Interfaces;

public interface IFeeCompletedPublisher
{
    Task PublishAsync(FeeCompletedMessage message, string? correlationId, CancellationToken cancellationToken = default);
}
