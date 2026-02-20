namespace BankMore.Transfer.Domain.Configurations;

public static class TransferConfiguration
{
    public const int IdLength = 37;
    public const int OriginAccountIdLength = 37;
    public const int DestinationAccountIdLength = 37;
    public const int MovementDateLength = 25;
    public const int ValuePrecision = 18;
    public const int ValueScale = 2;
}
