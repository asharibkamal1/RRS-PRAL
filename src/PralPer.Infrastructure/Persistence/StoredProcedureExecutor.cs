using System.Data;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using PralPer.Application.Abstractions;

namespace PralPer.Infrastructure.Persistence;

/// <summary>
/// Dapper-based stored-procedure executor. Reuses the EF Core <see cref="AppDbContext"/> connection
/// so SP calls participate in the same connection (and any active transaction) as entity work.
/// </summary>
public sealed class StoredProcedureExecutor : IStoredProcedureExecutor
{
    private readonly AppDbContext _db;

    public StoredProcedureExecutor(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<T>> QueryAsync<T>(string storedProcedure, object? parameters = null, CancellationToken ct = default)
    {
        var connection = _db.Database.GetDbConnection();
        var command = new CommandDefinition(
            storedProcedure, parameters,
            transaction: _db.Database.CurrentTransaction?.GetDbTransaction(),
            commandType: CommandType.StoredProcedure,
            cancellationToken: ct);

        var rows = await connection.QueryAsync<T>(command);
        return rows.AsList();
    }

    public async Task<T?> QuerySingleOrDefaultAsync<T>(string storedProcedure, object? parameters = null, CancellationToken ct = default)
    {
        var connection = _db.Database.GetDbConnection();
        var command = new CommandDefinition(
            storedProcedure, parameters,
            transaction: _db.Database.CurrentTransaction?.GetDbTransaction(),
            commandType: CommandType.StoredProcedure,
            cancellationToken: ct);

        return await connection.QuerySingleOrDefaultAsync<T?>(command);
    }

    public async Task<int> ExecuteAsync(string storedProcedure, object? parameters = null, CancellationToken ct = default)
    {
        var connection = _db.Database.GetDbConnection();
        var command = new CommandDefinition(
            storedProcedure, parameters,
            transaction: _db.Database.CurrentTransaction?.GetDbTransaction(),
            commandType: CommandType.StoredProcedure,
            cancellationToken: ct);

        return await connection.ExecuteAsync(command);
    }

    public async Task<T?> ExecuteScalarAsync<T>(string storedProcedure, object? parameters = null, CancellationToken ct = default)
    {
        var connection = _db.Database.GetDbConnection();
        var command = new CommandDefinition(
            storedProcedure, parameters,
            transaction: _db.Database.CurrentTransaction?.GetDbTransaction(),
            commandType: CommandType.StoredProcedure,
            cancellationToken: ct);

        return await connection.ExecuteScalarAsync<T?>(command);
    }
}
