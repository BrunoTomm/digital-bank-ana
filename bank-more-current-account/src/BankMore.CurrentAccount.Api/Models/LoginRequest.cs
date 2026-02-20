namespace BankMore.CurrentAccount.Api.Models;

public record LoginRequest(string? AccountNumberOrCpf, string Password);
