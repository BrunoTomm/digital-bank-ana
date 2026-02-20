using MediatR;

namespace BankMore.CurrentAccount.Application.Commands.Movement;

public record MovementCommand(
    string? AccountNumber,
    string AccountIdFromToken,
    decimal Value,
    char Type) : IRequest<Unit>;
