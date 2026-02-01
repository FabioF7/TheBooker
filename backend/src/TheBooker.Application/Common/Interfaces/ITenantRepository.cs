using TheBooker.Domain.Entities;

namespace TheBooker.Application.Common.Interfaces;

/// <summary>
/// Repository interface for Tenant aggregate.
/// </summary>
public interface ITenantRepository : IRepository<Tenant>
{
    Task<Tenant?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default);
}
