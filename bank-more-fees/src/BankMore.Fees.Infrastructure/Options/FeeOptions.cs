namespace BankMore.Fees.Infrastructure.Options;

public class FeeOptions
{
    public const string SectionName = "Fee";
    public decimal FixedAmountPerTransfer { get; set; }
}
