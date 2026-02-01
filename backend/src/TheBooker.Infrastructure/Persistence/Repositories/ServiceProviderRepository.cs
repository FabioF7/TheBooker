using Microsoft.EntityFrameworkCore;
using TheBooker.Application.Common.Interfaces;
using TheBooker.Domain.Entities;

namespace TheBooker.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for ServiceProvider entity.
/// </summary>
public class ServiceProviderRepository : IServiceProviderRepository
{
    private readonly ApplicationDbContext _context;

    public ServiceProviderRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ServiceProvider?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ServiceProviders
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<ServiceProvider>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ServiceProviders
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ServiceProvider>> GetActiveByTenantAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await _context.ServiceProviders
            .Where(p => p.TenantId == tenantId && p.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ServiceProvider>> GetByServiceAsync(
        Guid serviceId,
        CancellationToken cancellationToken = default)
    {
        return await _context.ServiceProviders
            .Where(p => p.Services.Any(s => s.Id == serviceId) && p.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<ServiceProvider?> GetWithServicesAsync(
        Guid providerId,
        CancellationToken cancellationToken = default)
    {
        return await _context.ServiceProviders
            .Include(p => p.Services)
            .FirstOrDefaultAsync(p => p.Id == providerId, cancellationToken);
    }

    public async Task AddAsync(ServiceProvider entity, CancellationToken cancellationToken = default)
    {
        await _context.ServiceProviders.AddAsync(entity, cancellationToken);
    }

    public void Update(ServiceProvider entity)
    {
        _context.ServiceProviders.Update(entity);
    }

    public void Remove(ServiceProvider entity)
    {
        _context.ServiceProviders.Remove(entity);
    }
}
