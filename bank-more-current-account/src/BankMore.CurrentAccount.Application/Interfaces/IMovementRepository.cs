using BankMore.CurrentAccount.Domain.Entities;

namespace BankMore.CurrentAccount.Application.Interfaces;

public interface IMovementRepository
{
    Task CreateAsync(Movement movement, CancellationToken cancellationToken = default);
    Task<decimal> GetBalanceAsync(string currentAccountId, CancellationToken cancellationToken = default);
}
