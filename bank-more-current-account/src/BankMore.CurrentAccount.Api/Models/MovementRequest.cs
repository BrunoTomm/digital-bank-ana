namespace BankMore.CurrentAccount.Api.Models;

public record MovementRequest(int? AccountNumber, decimal Value, char Type);
