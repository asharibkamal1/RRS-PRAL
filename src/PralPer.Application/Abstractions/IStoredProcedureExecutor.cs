namespace PralPer.Application.Abstractions;

/// <summary>
/// Data-access abstraction for invoking SQL Server stored procedures supplied by the Database team.
/// Lives alongside EF Core: use EF for entity CRUD, this for set-based / reporting SPs.
/// Parameters are passed as a plain object whose properties map to @parameters (Dapper convention),
/// keeping the Application layer free of any specific data library.
/// </summary>
public interface IStoredProcedureExecutor
{
    /// <summary>Executes a result-set stored procedure and maps rows to <typeparamref name="T"/>.</summary>
    Task<IReadOnlyList<T>> QueryAsync<T>(string storedProcedure, object? parameters = null, CancellationToken ct = default);

    /// <summary>Executes a stored procedure expected to return a single row (or null).</summary>
    Task<T?> QuerySingleOrDefaultAsync<T>(string storedProcedure, object? parameters = null, CancellationToken ct = default);

    /// <summary>Executes a non-query stored procedure; returns affected rows.</summary>
    Task<int> ExecuteAsync(string storedProcedure, object? parameters = null, CancellationToken ct = default);

    /// <summary>Executes a stored procedure returning a single scalar value.</summary>
    Task<T?> ExecuteScalarAsync<T>(string storedProcedure, object? parameters = null, CancellationToken ct = default);
}
