namespace BankMore.CurrentAccount.Api.Models;

public record InternalMovementRequest(string AccountId, decimal Amount, char Type);
