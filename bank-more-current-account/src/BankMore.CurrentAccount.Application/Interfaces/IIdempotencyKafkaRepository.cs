namespace BankMore.CurrentAccount.Application.Interfaces;

public interface IIdempotencyKafkaRepository
{
    Task<bool> ExistsAsync(string messageId, CancellationToken cancellationToken = default);
    Task CreateAsync(string messageId, CancellationToken cancellationToken = default);
}
