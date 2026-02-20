namespace BankMore.CurrentAccount.Domain.Entities;

public class CurrentAccount
{
    public string Id { get; set; } = string.Empty;
    public int Number { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool Active { get; set; } = true;
    public string PasswordHash { get; set; } = string.Empty;
    public string Salt { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
}
