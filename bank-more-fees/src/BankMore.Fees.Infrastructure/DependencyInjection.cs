using BankMore.Fees.Application.Interfaces;
using BankMore.Fees.Infrastructure.Idempotency;
using BankMore.Fees.Infrastructure.Kafka;
using BankMore.Fees.Infrastructure.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BankMore.Fees.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<KafkaOptions>(configuration.GetSection(KafkaOptions.SectionName));
        services.Configure<FeeOptions>(configuration.GetSection(FeeOptions.SectionName));
        services.AddSingleton<IIdempotencyRepository, InMemoryIdempotencyRepository>();
        services.AddSingleton<IFeeCompletedPublisher, FeeCompletedPublisher>();
        services.AddHostedService<TransfersCompletedConsumerService>();
        return services;
    }
}
