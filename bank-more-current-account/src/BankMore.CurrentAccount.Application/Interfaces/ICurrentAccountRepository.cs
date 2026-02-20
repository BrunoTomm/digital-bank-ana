namespace BankMore.CurrentAccount.Application.Interfaces;

public interface ICurrentAccountRepository
{
    Task<BankMore.CurrentAccount.Domain.Entities.CurrentAccount?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<BankMore.CurrentAccount.Domain.Entities.CurrentAccount?> GetByNumberAsync(int number, CancellationToken cancellationToken = default);
    Task<BankMore.CurrentAccount.Domain.Entities.CurrentAccount?> GetByCpfAsync(string cpf, CancellationToken cancellationToken = default);
    Task<int> GetNextAccountNumberAsync(CancellationToken cancellationToken = default);
    Task CreateAsync(BankMore.CurrentAccount.Domain.Entities.CurrentAccount account, CancellationToken cancellationToken = default);
    Task UpdateAsync(BankMore.CurrentAccount.Domain.Entities.CurrentAccount account, CancellationToken cancellationToken = default);
}
