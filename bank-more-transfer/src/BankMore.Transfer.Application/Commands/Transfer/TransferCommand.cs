using MediatR;

namespace BankMore.Transfer.Application.Commands.Transfer;

public record TransferCommand(
    string OriginAccountId,
    int DestinationAccountNumber,
    decimal Amount,
    string? CorrelationId = null) : IRequest<TransferResult>;
