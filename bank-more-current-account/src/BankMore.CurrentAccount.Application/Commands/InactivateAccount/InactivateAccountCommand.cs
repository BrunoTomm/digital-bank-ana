using MediatR;

namespace BankMore.CurrentAccount.Application.Commands.InactivateAccount;

public record InactivateAccountCommand(string AccountId, string Password) : IRequest<Unit>;
