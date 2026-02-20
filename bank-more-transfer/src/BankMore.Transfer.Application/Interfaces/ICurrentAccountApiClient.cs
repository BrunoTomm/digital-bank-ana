namespace BankMore.Transfer.Application.Interfaces;

public interface ICurrentAccountApiClient
{
    Task<string?> GetAccountIdByNumberAsync(int accountNumber, CancellationToken cancellationToken = default);
    Task DebitAsync(string accountId, decimal amount, CancellationToken cancellationToken = default);
    Task CreditAsync(string accountId, decimal amount, CancellationToken cancellationToken = default);
}
