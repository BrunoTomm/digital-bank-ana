using BankMore.Transfer.Application.Interfaces;
using BankMore.Transfer.Infrastructure.Clients;
using BankMore.Transfer.Infrastructure.Kafka;
using BankMore.Transfer.Infrastructure.Options;
using BankMore.Transfer.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;

namespace BankMore.Transfer.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException("ConnectionStrings:DefaultConnection is required. Configure in appsettings.json or environment.");
        services.AddScoped<ITransferRepository>(_ => new TransferRepository(connectionString));

        services.Configure<CurrentAccountApiOptions>(configuration.GetSection(CurrentAccountApiOptions.SectionName));
        services.AddHttpClient<ICurrentAccountApiClient, CurrentAccountApiClient>();

        services.Configure<KafkaOptions>(configuration.GetSection(KafkaOptions.SectionName));
        services.AddSingleton<ITransferEventPublisher, TransferEventPublisher>();

        services.Configure<SagaOptions>(configuration.GetSection(SagaOptions.SectionName));
        services.AddSingleton<Polly.IAsyncPolicy>(sp =>
        {
            var opts = sp.GetRequiredService<IOptions<SagaOptions>>().Value;
            var count = opts.CompensationRetryCount <= 0 ? 3 : opts.CompensationRetryCount;
            var baseSeconds = opts.CompensationRetryDelaySecondsBase <= 0 ? 2 : opts.CompensationRetryDelaySecondsBase;
            return Policy.Handle<Exception>()
                .WaitAndRetryAsync(count, retryAttempt => TimeSpan.FromSeconds(Math.Pow(baseSeconds, retryAttempt)));
        });

        return services;
    }
}
