using Microsoft.EntityFrameworkCore;
using TheBooker.Application.Common.Interfaces;
using TheBooker.Domain.Entities;

namespace TheBooker.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Tenant aggregate.
/// </summary>
public class TenantRepository : ITenantRepository
{
    private readonly ApplicationDbContext _context;

    public TenantRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Tenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Tenants
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Tenant>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Tenants
            .ToListAsync(cancellationToken);
    }

    public async Task<Tenant?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _context.Tenants
            .FirstOrDefaultAsync(t => t.Slug.Value == slug.ToLowerInvariant(), cancellationToken);
    }

    public async Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _context.Tenants
            .AnyAsync(t => t.Slug.Value == slug.ToLowerInvariant(), cancellationToken);
    }

    public async Task AddAsync(Tenant entity, CancellationToken cancellationToken = default)
    {
        await _context.Tenants.AddAsync(entity, cancellationToken);
    }

    public void Update(Tenant entity)
    {
        _context.Tenants.Update(entity);
    }

    public void Remove(Tenant entity)
    {
        _context.Tenants.Remove(entity);
    }
}
