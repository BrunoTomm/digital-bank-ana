namespace BankMore.Transfer.Infrastructure.Options;

public class CurrentAccountApiOptions
{
    public const string SectionName = "CurrentAccountApi";
    public string BaseUrl { get; set; } = "";
    public string InternalApiKey { get; set; } = "";
}
