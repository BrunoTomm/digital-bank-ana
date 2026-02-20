using BankMore.CurrentAccount.Application.Interfaces;
using BankMore.CurrentAccount.Infrastructure.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using CurrentAccountEntity = BankMore.CurrentAccount.Domain.Entities.CurrentAccount;

namespace BankMore.CurrentAccount.Infrastructure.Hosted;

public class SeedDataService : IHostedService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<SeedDataService> _logger;
    private readonly SeedDataOptions _options;

    public SeedDataService(IServiceProvider services, ILogger<SeedDataService> logger, IOptions<SeedDataOptions> options)
    {
        _services = services;
        _logger = logger;
        _options = options?.Value ?? new SeedDataOptions();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_options.Enabled || string.IsNullOrWhiteSpace(_options.AdminPassword) || string.IsNullOrWhiteSpace(_options.AdminAccountId))
        {
            _logger.LogDebug("Seed: desabilitado ou configuração incompleta.");
            return;
        }
        await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
        try
        {
            using var scope = _services.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<ICurrentAccountRepository>();

            var existing = await repo.GetByNumberAsync(_options.AdminNumber, cancellationToken);
            if (existing != null)
            {
                _logger.LogDebug("Seed: conta admin já existe.");
                return;
            }

            var salt = GenerateSalt();
            var hash = HashPassword(_options.AdminPassword, salt);
            var account = new CurrentAccountEntity
            {
                Id = _options.AdminAccountId,
                Number = _options.AdminNumber,
                Name = string.IsNullOrWhiteSpace(_options.AdminName) ? "Admin" : _options.AdminName,
                Active = true,
                PasswordHash = hash,
                Salt = salt,
                Cpf = string.IsNullOrWhiteSpace(_options.AdminCpf) ? "52998224725" : _options.AdminCpf
            };

            await repo.CreateAsync(account, cancellationToken);
            _logger.LogInformation("Seed: conta admin criada. Número {Number}.", _options.AdminNumber);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Seed: não foi possível criar conta admin (banco pode ainda não estar pronto).");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

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
