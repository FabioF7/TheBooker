using Microsoft.EntityFrameworkCore;
using TheBooker.Application.Common.Interfaces;
using TheBooker.Domain.Entities;

namespace TheBooker.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for ScheduleOverride entity.
/// </summary>
public class ScheduleOverrideRepository : IScheduleOverrideRepository
{
    private readonly ApplicationDbContext _context;

    public ScheduleOverrideRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ScheduleOverride?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ScheduleOverrides
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<ScheduleOverride>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ScheduleOverrides
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ScheduleOverride>> GetByTenantAndDateAsync(
        Guid tenantId,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        return await _context.ScheduleOverrides
            .Where(o => o.TenantId == tenantId &&
                        o.ProviderId == null &&
                        o.StartDate <= date &&
                        o.EndDate >= date)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ScheduleOverride>> GetByProviderAndDateAsync(
        Guid providerId,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        return await _context.ScheduleOverrides
            .Where(o => o.ProviderId == providerId &&
                        o.StartDate <= date &&
                        o.EndDate >= date)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ScheduleOverride>> GetByDateRangeAsync(
        Guid tenantId,
        DateOnly startDate,
        DateOnly endDate,
        Guid? providerId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.ScheduleOverrides
            .Where(o => o.TenantId == tenantId &&
                        o.StartDate <= endDate &&
                        o.EndDate >= startDate);

        if (providerId.HasValue)
        {
            query = query.Where(o => o.ProviderId == null || o.ProviderId == providerId);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task AddAsync(ScheduleOverride entity, CancellationToken cancellationToken = default)
    {
        await _context.ScheduleOverrides.AddAsync(entity, cancellationToken);
    }

    public void Update(ScheduleOverride entity)
    {
        _context.ScheduleOverrides.Update(entity);
    }

    public void Remove(ScheduleOverride entity)
    {
        _context.ScheduleOverrides.Remove(entity);
    }
}
