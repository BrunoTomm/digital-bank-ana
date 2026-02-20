namespace BankMore.Transfer.Infrastructure.Models;

public record InternalMovementRequest(string AccountId, decimal Amount, char Type);
