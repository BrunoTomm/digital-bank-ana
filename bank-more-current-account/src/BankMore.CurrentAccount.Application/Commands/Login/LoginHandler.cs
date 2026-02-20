using System.Security.Claims;
using BankMore.CurrentAccount.Application.Helpers;
using BankMore.CurrentAccount.Application.Interfaces;
using BankMore.CurrentAccount.Application.Validators;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;

namespace BankMore.CurrentAccount.Application.Commands.Login;

public class LoginHandler : IRequestHandler<LoginCommand, LoginResult>
{
    private readonly ICurrentAccountRepository _repository;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<LoginHandler> _logger;

    public LoginHandler(ICurrentAccountRepository repository, IOptions<JwtSettings> jwtSettings, ILogger<LoginHandler> logger)
    {
        _repository = repository;
        _jwtSettings = jwtSettings.Value;
        _logger = logger;
    }

    public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.AccountNumberOrCpf) || string.IsNullOrWhiteSpace(request.Password))
        {
            _logger.LogWarning("Login failed: missing credentials");
            throw new UnauthorizedAccessException("USER_UNAUTHORIZED|Invalid credentials.");
        }

        var input = request.AccountNumberOrCpf.Trim();
        BankMore.CurrentAccount.Domain.Entities.CurrentAccount? account = null;
        if (int.TryParse(input, out var number))
            account = await _repository.GetByNumberAsync(number, cancellationToken);
        else
            account = await _repository.GetByCpfAsync(CpfValidator.Normalize(input), cancellationToken);

        if (account == null)
        {
            _logger.LogWarning("Login failed: account not found for {Input}", input);
            throw new UnauthorizedAccessException("USER_UNAUTHORIZED|Invalid credentials.");
        }

        var hash = HashPassword(request.Password, account.Salt);
        if (hash != account.PasswordHash)
        {
            _logger.LogWarning("Login failed: invalid password for account {AccountId}", account.Id);
            throw new UnauthorizedAccessException("USER_UNAUTHORIZED|Invalid credentials.");
        }

        var token = JwtHelper.BuildToken(account.Id, account.Number.ToString(), _jwtSettings);
        _logger.LogInformation("Login successful for account {AccountNumber}", account.Number);
        return new LoginResult(token, account.Id, account.Number);
    }

    private static string HashPassword(string password, string salt)
    {
        var salted = Encoding.UTF8.GetBytes(password + salt);
        var hash = SHA256.HashData(salted);
        return Convert.ToBase64String(hash);
    }
}
