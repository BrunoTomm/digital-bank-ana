using BankMore.CurrentAccount.Application.Interfaces;
using BankMore.CurrentAccount.Infrastructure.Hosted;
using BankMore.CurrentAccount.Infrastructure.Kafka;
using BankMore.CurrentAccount.Infrastructure.Options;
using BankMore.CurrentAccount.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BankMore.CurrentAccount.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString, IConfiguration? configuration = null)
    {
        services.AddSingleton<ICurrentAccountRepository>(sp => new CurrentAccountRepository(connectionString, sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<CurrentAccountRepository>>()));
        services.AddSingleton<IMovementRepository>(sp => new MovementRepository(connectionString, sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<MovementRepository>>()));
        services.AddSingleton<ITarifasRepository>(_ => new TarifasRepository(connectionString));
        services.AddSingleton<IIdempotencyKafkaRepository>(_ => new IdempotencyKafkaRepository(connectionString));

        if (configuration != null)
            services.Configure<SeedDataOptions>(configuration.GetSection(SeedDataOptions.SectionName));
        services.AddHostedService<SeedDataService>();

        if (configuration != null)
        {
            services.Configure<KafkaOptions>(configuration.GetSection(KafkaOptions.SectionName));
            services.AddHostedService<TransfersCompletedConsumerService>();
            services.AddHostedService<FeesCompletedConsumerService>();
        }

        return services;
    }
}
