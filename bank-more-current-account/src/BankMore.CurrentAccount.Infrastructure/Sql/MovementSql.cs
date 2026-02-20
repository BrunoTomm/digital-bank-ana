namespace BankMore.CurrentAccount.Infrastructure.Sql;

public static class MovementSql
{
    public const string Insert = "INSERT INTO movimento (idmovimento, idcontacorrente, datamovimento, tipomovimento, valor) VALUES (:Id, :CurrentAccountId, :MovementDate, :Type, :Value)";
    public const string Balance = "SELECT NVL(SUM(CASE WHEN tipomovimento = 'C' THEN valor ELSE -valor END), 0) FROM movimento WHERE idcontacorrente = :currentAccountId";
}
