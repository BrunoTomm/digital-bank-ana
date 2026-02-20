using MediatR;

namespace BankMore.CurrentAccount.Application.Commands.Login;

public record LoginCommand(string? AccountNumberOrCpf, string Password) : IRequest<LoginResult>;
