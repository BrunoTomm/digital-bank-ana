using BankMore.Transfer.Application.Interfaces;
using BankMore.Transfer.Domain.Configurations;
using MediatR;
using Microsoft.Extensions.Logging;
using Polly;

namespace BankMore.Transfer.Application.Commands.Transfer;

public class TransferHandler : IRequestHandler<TransferCommand, TransferResult>
{
    private readonly ITransferRepository _transferRepository;
    private readonly ICurrentAccountApiClient _currentAccountClient;
    private readonly ITransferEventPublisher _eventPublisher;
    private readonly IAsyncPolicy _compensationRetryPolicy;
    private readonly ILogger<TransferHandler> _logger;

    public TransferHandler(
        ITransferRepository transferRepository,
        ICurrentAccountApiClient currentAccountClient,
        ITransferEventPublisher eventPublisher,
        IAsyncPolicy compensationRetryPolicy,
        ILogger<TransferHandler> logger)
    {
        _transferRepository = transferRepository;
        _currentAccountClient = currentAccountClient;
        _eventPublisher = eventPublisher;
        _compensationRetryPolicy = compensationRetryPolicy;
        _logger = logger;
    }

    public async Task<TransferResult> Handle(TransferCommand request, CancellationToken cancellationToken)
    {
        var destinationAccountId = await _currentAccountClient.GetAccountIdByNumberAsync(request.DestinationAccountNumber, cancellationToken);
        if (string.IsNullOrEmpty(destinationAccountId))
            throw new ApplicationException("INVALID_ACCOUNT|Destination account not found.");

        if (request.OriginAccountId == destinationAccountId)
            throw new ApplicationException("INVALID_ACCOUNT|Origin and destination must be different.");

        if (request.Amount <= 0)
            throw new ApplicationException("INVALID_VALUE|Amount must be positive.");
        if (request.OriginAccountId.Length > TransferConfiguration.OriginAccountIdLength || destinationAccountId.Length > TransferConfiguration.DestinationAccountIdLength)
            throw new ApplicationException("INVALID_ACCOUNT|Account id exceeds maximum length.");

        var transferId = Guid.NewGuid().ToString();
        var movementDate = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss");

        try
        {
            await _currentAccountClient.DebitAsync(request.OriginAccountId, request.Amount, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Debit failed for transfer {TransferId}.", transferId);
            throw new ApplicationException("TRANSFER_FAILED|Debit failed: " + ex.Message);
        }

        try
        {
            await _currentAccountClient.CreditAsync(destinationAccountId, request.Amount, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Credit failed for transfer {TransferId}. Performing compensation.", transferId);
            try
            {
                await _compensationRetryPolicy.ExecuteAsync(async () =>
                    await _currentAccountClient.CreditAsync(request.OriginAccountId, request.Amount, cancellationToken));
            }
            catch (Exception compensationEx)
            {
                _logger.LogCritical(compensationEx,
                    "Compensation failed for transfer {TransferId}. Manual intervention required. OriginAccountId={OriginAccountId}, DestinationAccountId={DestinationAccountId}, Amount={Amount}, CorrelationId={CorrelationId}",
                    transferId, request.OriginAccountId, destinationAccountId, request.Amount, request.CorrelationId ?? "(none)");
                throw new ApplicationException("TRANSFER_FAILED|Credit failed. Compensation failed after retries. Manual intervention required.");
            }
            throw new ApplicationException("TRANSFER_FAILED|Credit failed. Compensation attempted.");
        }

        var transfer = new BankMore.Transfer.Domain.Entities.Transfer
        {
            Id = transferId,
            OriginAccountId = request.OriginAccountId,
            DestinationAccountId = destinationAccountId,
            MovementDate = movementDate,
            Value = request.Amount
        };
        await _transferRepository.CreateAsync(transfer, cancellationToken);

        await _eventPublisher.PublishAsync(
            new TransferCompletedMessage(transferId, request.OriginAccountId, destinationAccountId, request.Amount, movementDate),
            request.CorrelationId,
            cancellationToken);

        _logger.LogInformation("Transfer {TransferId} completed from {Origin} to {Destination}, amount {Amount}.",
            transferId, request.OriginAccountId, destinationAccountId, request.Amount);
        return new TransferResult(transferId);
    }
}
