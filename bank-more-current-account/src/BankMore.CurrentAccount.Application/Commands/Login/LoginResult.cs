namespace BankMore.CurrentAccount.Application.Commands.Login;

public record LoginResult(string Token, string AccountId, int AccountNumber);
