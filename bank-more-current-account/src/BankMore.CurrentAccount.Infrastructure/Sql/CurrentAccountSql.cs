namespace BankMore.CurrentAccount.Infrastructure.Sql;

public static class CurrentAccountSql
{
    public const string SelectById = "SELECT idcontacorrente AS \"Id\", numero AS \"Number\", nome AS \"Name\", ativo AS \"Active\", senha AS \"PasswordHash\", salt AS \"Salt\", cpf AS \"Cpf\" FROM contacorrente WHERE idcontacorrente = :id";
    public const string SelectByNumber = "SELECT idcontacorrente AS \"Id\", numero AS \"Number\", nome AS \"Name\", ativo AS \"Active\", senha AS \"PasswordHash\", salt AS \"Salt\", cpf AS \"Cpf\" FROM contacorrente WHERE numero = :accountNumber";
    public const string SelectByCpf = "SELECT idcontacorrente AS \"Id\", numero AS \"Number\", nome AS \"Name\", ativo AS \"Active\", senha AS \"PasswordHash\", salt AS \"Salt\", cpf AS \"Cpf\" FROM contacorrente WHERE cpf = :cpf";
    public const string NextAccountNumber = "SELECT NVL(MAX(numero), 0) + 1 FROM contacorrente";
    public const string Insert = "INSERT INTO contacorrente (idcontacorrente, numero, nome, ativo, senha, salt, cpf) VALUES (:Id, :Numero, :Nome, :ActiveNum, :PasswordHash, :Salt, :Cpf)";
    public const string UpdateActive = "UPDATE contacorrente SET ativo = :ActiveNum WHERE idcontacorrente = :Id";
}
