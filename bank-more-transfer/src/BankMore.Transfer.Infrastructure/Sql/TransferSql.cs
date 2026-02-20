namespace BankMore.Transfer.Infrastructure.Sql;

public static class TransferSql
{
    public const string Insert = "INSERT INTO transferencia (idtransferencia, idcontacorrente_origem, idcontacorrente_destino, datamovimento, valor) VALUES (:Id, :OriginAccountId, :DestinationAccountId, :MovementDate, :Value)";
}
