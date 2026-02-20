namespace BankMore.CurrentAccount.Domain.Entities;

public class Movement
{
    public string Id { get; set; } = string.Empty;
    public string CurrentAccountId { get; set; } = string.Empty;
    public string MovementDate { get; set; } = string.Empty;
    public char Type { get; set; }
    public decimal Value { get; set; }
}
