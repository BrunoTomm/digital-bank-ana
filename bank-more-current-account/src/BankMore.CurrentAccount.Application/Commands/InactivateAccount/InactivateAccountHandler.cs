using BankMore.CurrentAccount.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace BankMore.CurrentAccount.Application.Commands.InactivateAccount;

public class InactivateAccountHandler : IRequestHandler<InactivateAccountCommand, Unit>
{
    private readonly ICurrentAccountRepository _repository;
    private readonly ILogger<InactivateAccountHandler> _logger;

    public InactivateAccountHandler(ICurrentAccountRepository repository, ILogger<InactivateAccountHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Unit> Handle(InactivateAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await _repository.GetByIdAsync(request.AccountId, cancellationToken);
        if (account == null)
            throw new ApplicationException("INVALID_ACCOUNT|Account not found.");
        var hash = HashPassword(request.Password, account.Salt);
        if (hash != account.PasswordHash)
            throw new UnauthorizedAccessException("USER_UNAUTHORIZED|Invalid password.");
        account.Active = false;
        await _repository.UpdateAsync(account, cancellationToken);
        _logger.LogInformation("Account {AccountId} inactivated.", request.AccountId);
        return Unit.Value;
    }

    private static string HashPassword(string password, string salt)
    {
        var salted = Encoding.UTF8.GetBytes(password + salt);
        var hash = SHA256.HashData(salted);
        return Convert.ToBase64String(hash);
    }
}
