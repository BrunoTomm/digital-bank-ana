namespace BankMore.CurrentAccount.Domain.Configurations;

public static class CurrentAccountConfiguration
{
    public const int IdLength = 37;
    public const int NumberMaxDigits = 10;
    public const int NameLength = 100;
    public const int PasswordHashLength = 100;
    public const int SaltLength = 100;
    public const int CpfLength = 14;
}
