using MediatR;

namespace BankMore.CurrentAccount.Application.Queries.Balance;

public record BalanceQuery(string AccountId) : IRequest<BalanceResult>;
