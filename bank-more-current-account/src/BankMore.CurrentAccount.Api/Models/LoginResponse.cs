namespace BankMore.CurrentAccount.Api.Models;

public record LoginResponse(string Token, string AccountId, int AccountNumber);
