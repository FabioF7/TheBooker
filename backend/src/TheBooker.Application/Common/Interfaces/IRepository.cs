using TheBooker.Domain.Common.Primitives;

namespace TheBooker.Application.Common.Interfaces;

/// <summary>
/// Generic repository interface for basic CRUD operations.
/// </summary>
public interface IRepository<T> where T : Entity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    void Update(T entity);
    void Remove(T entity);
}

/// <summary>
/// Unit of Work pattern interface.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
