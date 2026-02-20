namespace BankMore.CurrentAccount.Application.Queries.Balance;

public record BalanceResult(int AccountNumber, string HolderName, DateTime QueriedAt, decimal Balance);
