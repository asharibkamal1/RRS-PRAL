using System.Linq.Expressions;
using PralPer.Domain.Common;

namespace PralPer.Application.Abstractions;

/// <summary>Generic repository abstraction over an aggregate/entity type.</summary>
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<T>> ListAsync(CancellationToken ct = default);
    Task<IReadOnlyList<T>> ListAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);

    /// <summary>Composable query root for read scenarios (projections, includes, paging).</summary>
    IQueryable<T> Query();

    Task AddAsync(T entity, CancellationToken ct = default);
    void Update(T entity);
    void Remove(T entity);
}
