namespace BankMore.CurrentAccount.Infrastructure.Options;

public class SeedDataOptions
{
    public const string SectionName = "SeedData";
    public bool Enabled { get; set; } = true;
    public string AdminAccountId { get; set; } = "";
    public int AdminNumber { get; set; } = 1;
    public string AdminName { get; set; } = "";
    public string AdminPassword { get; set; } = "";
    public string AdminCpf { get; set; } = "";
}
