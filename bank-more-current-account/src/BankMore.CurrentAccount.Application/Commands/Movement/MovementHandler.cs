using BankMore.CurrentAccount.Application.Interfaces;
using BankMore.CurrentAccount.Domain.Configurations;
using BankMore.CurrentAccount.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BankMore.CurrentAccount.Application.Commands.Movement;

public class MovementHandler : IRequestHandler<MovementCommand, Unit>
{
    private readonly ICurrentAccountRepository _accountRepository;
    private readonly IMovementRepository _movementRepository;
    private readonly ILogger<MovementHandler> _logger;

    public MovementHandler(
        ICurrentAccountRepository accountRepository,
        IMovementRepository movementRepository,
        ILogger<MovementHandler> logger)
    {
        _accountRepository = accountRepository;
        _movementRepository = movementRepository;
        _logger = logger;
    }

    public async Task<Unit> Handle(MovementCommand request, CancellationToken cancellationToken)
    {
        var accountNumber = request.AccountNumber != null && int.TryParse(request.AccountNumber, out var num)
            ? num
            : (int?)null;
        var accountId = accountNumber.HasValue
            ? (await _accountRepository.GetByNumberAsync(accountNumber!.Value, cancellationToken))?.Id
            : request.AccountIdFromToken;
        if (string.IsNullOrEmpty(accountId))
            accountId = request.AccountIdFromToken;

        var account = await _accountRepository.GetByIdAsync(accountId, cancellationToken);
        if (account == null)
            throw new ApplicationException("INVALID_ACCOUNT|Account not found.");
        if (!account.Active)
            throw new ApplicationException("INACTIVE_ACCOUNT|Account is inactive.");
        if (request.Value <= 0)
            throw new ApplicationException("INVALID_VALUE|Value must be positive.");
        if (accountId.Length > MovementConfiguration.CurrentAccountIdLength)
            throw new ApplicationException("INVALID_ACCOUNT|Account id exceeds maximum length.");
        if (request.Type != 'C' && request.Type != 'D')
            throw new ApplicationException("INVALID_TYPE|Type must be C (Credit) or D (Debit).");

        bool isOtherAccount = accountId != request.AccountIdFromToken;
        if (isOtherAccount && request.Type != 'C')
            throw new ApplicationException("INVALID_TYPE|Only credit is allowed when moving to another account.");

        var movementDate = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss");
        var movement = new BankMore.CurrentAccount.Domain.Entities.Movement
        {
            Id = Guid.NewGuid().ToString(),
            CurrentAccountId = accountId,
            MovementDate = movementDate,
            Type = request.Type,
            Value = request.Value
        };
        await _movementRepository.CreateAsync(movement, cancellationToken);
        _logger.LogInformation("Movement {MovementId} created for account {AccountId}, type {Type}, value {Value}.",
            movement.Id, accountId, request.Type, request.Value);
        return Unit.Value;
    }
}
