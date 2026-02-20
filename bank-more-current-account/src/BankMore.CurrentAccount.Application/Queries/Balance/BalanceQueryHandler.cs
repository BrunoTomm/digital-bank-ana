using BankMore.CurrentAccount.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BankMore.CurrentAccount.Application.Queries.Balance;

public class BalanceQueryHandler : IRequestHandler<BalanceQuery, BalanceResult>
{
    private readonly ICurrentAccountRepository _accountRepository;
    private readonly IMovementRepository _movementRepository;
    private readonly ILogger<BalanceQueryHandler> _logger;

    public BalanceQueryHandler(
        ICurrentAccountRepository accountRepository,
        IMovementRepository movementRepository,
        ILogger<BalanceQueryHandler> logger)
    {
        _accountRepository = accountRepository;
        _movementRepository = movementRepository;
        _logger = logger;
    }

    public async Task<BalanceResult> Handle(BalanceQuery request, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByIdAsync(request.AccountId, cancellationToken);
        if (account == null)
            throw new ApplicationException("INVALID_ACCOUNT|Account not found.");
        if (!account.Active)
            throw new ApplicationException("INACTIVE_ACCOUNT|Account is inactive.");

        var balance = await _movementRepository.GetBalanceAsync(request.AccountId, cancellationToken);
        _logger.LogInformation("Balance queried for account {AccountId}: {Balance}", request.AccountId, balance);
        return new BalanceResult(account.Number, account.Name, DateTime.UtcNow, balance);
    }
}
