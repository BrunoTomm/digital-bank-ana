namespace BankMore.Fees.Application.Messages;

public record TransferCompletedMessage(string TransferId, string OriginAccountId, string DestinationAccountId, decimal Value, string MovementDate);
