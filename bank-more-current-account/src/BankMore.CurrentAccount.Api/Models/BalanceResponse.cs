namespace BankMore.CurrentAccount.Api.Models;

public record BalanceResponse(int AccountNumber, string HolderName, DateTime QueriedAt, decimal Balance);
