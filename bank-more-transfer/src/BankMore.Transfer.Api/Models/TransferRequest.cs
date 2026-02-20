namespace BankMore.Transfer.Api.Models;

public record TransferRequest(int DestinationAccountNumber, decimal Amount);
