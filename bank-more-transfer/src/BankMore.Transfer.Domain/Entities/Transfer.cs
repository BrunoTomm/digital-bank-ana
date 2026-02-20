namespace BankMore.Transfer.Domain.Entities;

public class Transfer
{
    public string Id { get; set; } = "";
    public string OriginAccountId { get; set; } = "";
    public string DestinationAccountId { get; set; } = "";
    public string MovementDate { get; set; } = "";
    public decimal Value { get; set; }
}
