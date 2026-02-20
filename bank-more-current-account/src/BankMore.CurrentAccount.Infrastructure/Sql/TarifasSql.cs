namespace BankMore.CurrentAccount.Infrastructure.Sql;

public static class TarifasSql
{
    public const string Insert = "INSERT INTO tarifas (idtarifa, idcontacorrente, datamovimento, valor) VALUES (:IdTarifa, :IdContaCorrente, :DataMovimento, :Valor)";
}
