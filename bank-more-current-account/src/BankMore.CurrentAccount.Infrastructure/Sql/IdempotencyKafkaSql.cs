namespace BankMore.CurrentAccount.Infrastructure.Sql;

public static class IdempotencyKafkaSql
{
    public const string Exists = "SELECT 1 FROM idempotencia_kafka WHERE message_id = :messageId AND ROWNUM = 1";
    public const string Insert = "INSERT INTO idempotencia_kafka (message_id) VALUES (:messageId)";
}
