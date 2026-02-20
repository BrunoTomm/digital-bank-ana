using MediatR;

namespace BankMore.CurrentAccount.Application.Commands.RegisterCurrentAccount;

public record RegisterCurrentAccountCommand(string Cpf, string Password) : IRequest<RegisterCurrentAccountResult>;
