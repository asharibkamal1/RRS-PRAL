using PralPer.Domain.Common;

namespace PralPer.Application.Abstractions;

/// <summary>Coordinates repositories over a single transactional boundary (DbContext).</summary>
public interface IUnitOfWork : IAsyncDisposable
{
    IRepository<T> Repository<T>() where T : BaseEntity;
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
