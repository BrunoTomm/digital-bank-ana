namespace BankMore.Transfer.Application.Interfaces;

public interface ITransferRepository
{
    Task CreateAsync(BankMore.Transfer.Domain.Entities.Transfer transfer, CancellationToken cancellationToken = default);
}
