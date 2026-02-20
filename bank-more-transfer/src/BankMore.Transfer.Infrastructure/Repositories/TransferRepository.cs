using BankMore.Transfer.Application.Interfaces;
using BankMore.Transfer.Infrastructure.Sql;
using Dapper;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using TransferEntity = BankMore.Transfer.Domain.Entities.Transfer;

namespace BankMore.Transfer.Infrastructure.Repositories;

public class TransferRepository : ITransferRepository
{
    private readonly string _connectionString;

    public TransferRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    private IDbConnection CreateConnection() => new OracleConnection(_connectionString);

    public async Task CreateAsync(TransferEntity transfer, CancellationToken cancellationToken = default)
    {
        using var conn = CreateConnection();
        await conn.ExecuteAsync(new CommandDefinition(TransferSql.Insert, new
        {
            transfer.Id,
            transfer.OriginAccountId,
            transfer.DestinationAccountId,
            transfer.MovementDate,
            transfer.Value
        }, cancellationToken: cancellationToken));
    }
}
