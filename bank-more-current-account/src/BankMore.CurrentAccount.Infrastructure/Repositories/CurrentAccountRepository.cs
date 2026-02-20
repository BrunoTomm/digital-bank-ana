using BankMore.CurrentAccount.Application.Interfaces;
using BankMore.CurrentAccount.Infrastructure.Sql;
using Dapper;
using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using CurrentAccountEntity = BankMore.CurrentAccount.Domain.Entities.CurrentAccount;

namespace BankMore.CurrentAccount.Infrastructure.Repositories;

public class CurrentAccountRepository : ICurrentAccountRepository
{
    private readonly string _connectionString;
    private readonly ILogger<CurrentAccountRepository> _logger;

    public CurrentAccountRepository(string connectionString, ILogger<CurrentAccountRepository> logger)
    {
        _connectionString = connectionString;
        _logger = logger;
    }

    private IDbConnection CreateConnection() => new OracleConnection(_connectionString);

    public async Task<CurrentAccountEntity?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        using var conn = CreateConnection();
        var row = await conn.QuerySingleOrDefaultAsync<CurrentAccountRow>(new CommandDefinition(CurrentAccountSql.SelectById, new { id }, cancellationToken: cancellationToken));
        return row == null ? null : Map(row);
    }

    public async Task<CurrentAccountEntity?> GetByNumberAsync(int number, CancellationToken cancellationToken = default)
    {
        using var conn = CreateConnection();
        var row = await conn.QuerySingleOrDefaultAsync<CurrentAccountRow>(new CommandDefinition(CurrentAccountSql.SelectByNumber, new { accountNumber = number }, cancellationToken: cancellationToken));
        return row == null ? null : Map(row);
    }

    public async Task<CurrentAccountEntity?> GetByCpfAsync(string cpf, CancellationToken cancellationToken = default)
    {
        using var conn = CreateConnection();
        var row = await conn.QuerySingleOrDefaultAsync<CurrentAccountRow>(new CommandDefinition(CurrentAccountSql.SelectByCpf, new { cpf }, cancellationToken: cancellationToken));
        return row == null ? null : Map(row);
    }

    public async Task<int> GetNextAccountNumberAsync(CancellationToken cancellationToken = default)
    {
        using var conn = CreateConnection();
        var max = await conn.ExecuteScalarAsync<decimal?>(new CommandDefinition(CurrentAccountSql.NextAccountNumber, cancellationToken: cancellationToken));
        return (int)(max ?? 1);
    }

    public async Task CreateAsync(CurrentAccountEntity account, CancellationToken cancellationToken = default)
    {
        using var conn = CreateConnection();
        await conn.ExecuteAsync(new CommandDefinition(CurrentAccountSql.Insert, new
        {
            Id = account.Id,
            Numero = account.Number,
            Nome = account.Name,
            ActiveNum = account.Active ? 1 : 0,
            PasswordHash = account.PasswordHash,
            Salt = account.Salt,
            Cpf = account.Cpf
        }, cancellationToken: cancellationToken));
        _logger.LogDebug("Created current account {Id}", account.Id);
    }

    public async Task UpdateAsync(CurrentAccountEntity account, CancellationToken cancellationToken = default)
    {
        using var conn = CreateConnection();
        await conn.ExecuteAsync(new CommandDefinition(CurrentAccountSql.UpdateActive, new { Id = account.Id, ActiveNum = account.Active ? 1 : 0 }, cancellationToken: cancellationToken));
    }

    private static CurrentAccountEntity Map(CurrentAccountRow row) => new()
    {
        Id = row.Id,
        Number = row.Number,
        Name = row.Name,
        Active = row.Active == 1,
        PasswordHash = row.PasswordHash,
        Salt = row.Salt,
        Cpf = row.Cpf
    };

    private class CurrentAccountRow
    {
        public string Id { get; set; } = "";
        public int Number { get; set; }
        public string Name { get; set; } = "";
        public int Active { get; set; }
        public string PasswordHash { get; set; } = "";
        public string Salt { get; set; } = "";
        public string Cpf { get; set; } = "";
    }
}
