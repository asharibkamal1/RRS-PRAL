using System.Collections.Concurrent;
using PralPer.Application.Abstractions;
using PralPer.Domain.Common;
using PralPer.Infrastructure.Persistence;

namespace PralPer.Infrastructure.Repositories;

/// <summary>Wraps the DbContext as a transactional boundary and caches generic repositories.</summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _db;
    private readonly ConcurrentDictionary<Type, object> _repositories = new();

    public UnitOfWork(AppDbContext db) => _db = db;

    public IRepository<T> Repository<T>() where T : BaseEntity
        => (IRepository<T>)_repositories.GetOrAdd(typeof(T), _ => new Repository<T>(_db));

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);

    public ValueTask DisposeAsync() => _db.DisposeAsync();
}
