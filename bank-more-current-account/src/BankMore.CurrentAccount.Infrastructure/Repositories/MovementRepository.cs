using BankMore.CurrentAccount.Application.Interfaces;
using BankMore.CurrentAccount.Domain.Entities;
using BankMore.CurrentAccount.Infrastructure.Sql;
using Dapper;
using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace BankMore.CurrentAccount.Infrastructure.Repositories;

public class MovementRepository : IMovementRepository
{
    private readonly string _connectionString;
    private readonly ILogger<MovementRepository> _logger;

    public MovementRepository(string connectionString, ILogger<MovementRepository> logger)
    {
        _connectionString = connectionString;
        _logger = logger;
    }

    private IDbConnection CreateConnection() => new OracleConnection(_connectionString);

    public async Task CreateAsync(Movement movement, CancellationToken cancellationToken = default)
    {
        using var conn = CreateConnection();
        await conn.ExecuteAsync(new CommandDefinition(MovementSql.Insert, new
        {
            movement.Id,
            movement.CurrentAccountId,
            movement.MovementDate,
            Type = movement.Type.ToString(),
            movement.Value
        }, cancellationToken: cancellationToken));
    }

    public async Task<decimal> GetBalanceAsync(string currentAccountId, CancellationToken cancellationToken = default)
    {
        using var conn = CreateConnection();
        var balance = await conn.ExecuteScalarAsync<decimal>(new CommandDefinition(MovementSql.Balance, new { currentAccountId }, cancellationToken: cancellationToken));
        return balance;
    }
}
