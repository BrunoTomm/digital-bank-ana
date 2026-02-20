namespace BankMore.Fees.Application.Messages;

public record FeeCompletedMessage(string FeeId, string TransferId, string AccountIdToDebit, decimal Amount, string ProcessedAt);
