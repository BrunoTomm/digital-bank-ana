namespace BankMore.CurrentAccount.Application.Interfaces;

public interface ITarifasRepository
{
    Task CreateAsync(string idTarifa, string idContaCorrente, string dataMovimento, decimal valor, CancellationToken cancellationToken = default);
}
