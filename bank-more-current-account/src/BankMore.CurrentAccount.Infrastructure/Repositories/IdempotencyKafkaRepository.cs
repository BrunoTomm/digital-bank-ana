using BankMore.CurrentAccount.Application.Interfaces;
using BankMore.CurrentAccount.Infrastructure.Sql;
using Dapper;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace BankMore.CurrentAccount.Infrastructure.Repositories;

public class IdempotencyKafkaRepository : IIdempotencyKafkaRepository
{
    private readonly string _connectionString;

    public IdempotencyKafkaRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    private IDbConnection CreateConnection() => new OracleConnection(_connectionString);

    public async Task<bool> ExistsAsync(string messageId, CancellationToken cancellationToken = default)
    {
        using var conn = CreateConnection();
        var exists = await conn.ExecuteScalarAsync<int?>(new CommandDefinition(IdempotencyKafkaSql.Exists, new { messageId }, cancellationToken: cancellationToken));
        return exists == 1;
    }

    public async Task CreateAsync(string messageId, CancellationToken cancellationToken = default)
    {
        using var conn = CreateConnection();
        await conn.ExecuteAsync(new CommandDefinition(IdempotencyKafkaSql.Insert, new { messageId }, cancellationToken: cancellationToken));
    }
}
