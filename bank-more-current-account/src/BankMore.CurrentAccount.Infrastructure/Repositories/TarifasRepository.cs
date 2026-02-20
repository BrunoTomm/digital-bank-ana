using BankMore.CurrentAccount.Application.Interfaces;
using BankMore.CurrentAccount.Infrastructure.Sql;
using Dapper;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace BankMore.CurrentAccount.Infrastructure.Repositories;

public class TarifasRepository : ITarifasRepository
{
    private readonly string _connectionString;

    public TarifasRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    private IDbConnection CreateConnection() => new OracleConnection(_connectionString);

    public async Task CreateAsync(string idTarifa, string idContaCorrente, string dataMovimento, decimal valor, CancellationToken cancellationToken = default)
    {
        using var conn = CreateConnection();
        await conn.ExecuteAsync(new CommandDefinition(TarifasSql.Insert, new
        {
            IdTarifa = idTarifa,
            IdContaCorrente = idContaCorrente,
            DataMovimento = dataMovimento,
            Valor = valor
        }, cancellationToken: cancellationToken));
    }
}
