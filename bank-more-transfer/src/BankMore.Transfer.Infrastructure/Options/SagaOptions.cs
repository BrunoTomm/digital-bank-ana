namespace BankMore.Transfer.Infrastructure.Options;

public class SagaOptions
{
    public const string SectionName = "Saga";
    public int CompensationRetryCount { get; set; } = 3;
    public int CompensationRetryDelaySecondsBase { get; set; } = 2;
}
