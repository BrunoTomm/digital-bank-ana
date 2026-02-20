namespace BankMore.Fees.Application.Interfaces;

public interface IIdempotencyRepository
{
    Task<bool> ExistsAsync(string transferId, CancellationToken cancellationToken = default);
    Task MarkProcessedAsync(string transferId, CancellationToken cancellationToken = default);
}
