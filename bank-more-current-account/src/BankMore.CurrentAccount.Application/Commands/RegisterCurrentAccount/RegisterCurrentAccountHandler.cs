using BankMore.CurrentAccount.Application.Interfaces;
using BankMore.CurrentAccount.Application.Validators;
using BankMore.CurrentAccount.Domain.Configurations;
using BankMore.CurrentAccount.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace BankMore.CurrentAccount.Application.Commands.RegisterCurrentAccount;

public class RegisterCurrentAccountHandler : IRequestHandler<RegisterCurrentAccountCommand, RegisterCurrentAccountResult>
{
    private readonly ICurrentAccountRepository _repository;
    private readonly ILogger<RegisterCurrentAccountHandler> _logger;

    public RegisterCurrentAccountHandler(ICurrentAccountRepository repository, ILogger<RegisterCurrentAccountHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<RegisterCurrentAccountResult> Handle(RegisterCurrentAccountCommand request, CancellationToken cancellationToken)
    {
        if (!CpfValidator.IsValid(request.Cpf))
        {
            _logger.LogWarning("Invalid CPF provided: {Cpf}", request.Cpf);
            throw new ApplicationException($"INVALID_DOCUMENT|Invalid CPF.");
        }

        var normalizedCpf = CpfValidator.Normalize(request.Cpf);
        if (normalizedCpf.Length > CurrentAccountConfiguration.CpfLength)
            throw new ApplicationException($"INVALID_DOCUMENT|CPF exceeds maximum length ({CurrentAccountConfiguration.CpfLength}).");
        var existing = await _repository.GetByCpfAsync(normalizedCpf, cancellationToken);
        if (existing != null)
            throw new ApplicationException($"INVALID_DOCUMENT|Account already exists for this CPF.");

        var salt = GenerateSalt();
        var hash = HashPassword(request.Password, salt);
        var number = await _repository.GetNextAccountNumberAsync(cancellationToken);
        var account = new BankMore.CurrentAccount.Domain.Entities.CurrentAccount
        {
            Id = Guid.NewGuid().ToString(),
            Number = number,
            Name = $"Account Holder {number}",
            Active = true,
            PasswordHash = hash,
            Salt = salt,
            Cpf = normalizedCpf
        };

        await _repository.CreateAsync(account, cancellationToken);
        _logger.LogInformation("Current account registered. Number: {Number}, Id: {Id}", number, account.Id);
        return new RegisterCurrentAccountResult(number);
    }

    private static string GenerateSalt()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }

    private static string HashPassword(string password, string salt)
    {
        var salted = Encoding.UTF8.GetBytes(password + salt);
        var hash = SHA256.HashData(salted);
        return Convert.ToBase64String(hash);
    }
}
