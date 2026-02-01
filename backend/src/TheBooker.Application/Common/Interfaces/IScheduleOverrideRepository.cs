using TheBooker.Domain.Entities;

namespace TheBooker.Application.Common.Interfaces;

/// <summary>
/// Repository interface for ScheduleOverride entity.
/// </summary>
public interface IScheduleOverrideRepository : IRepository<ScheduleOverride>
{
    /// <summary>
    /// Gets overrides that apply to a specific date for a tenant.
    /// </summary>
    Task<IReadOnlyList<ScheduleOverride>> GetByTenantAndDateAsync(
        Guid tenantId,
        DateOnly date,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets overrides that apply to a specific date for a provider.
    /// </summary>
    Task<IReadOnlyList<ScheduleOverride>> GetByProviderAndDateAsync(
        Guid providerId,
        DateOnly date,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets overrides for a date range.
    /// </summary>
    Task<IReadOnlyList<ScheduleOverride>> GetByDateRangeAsync(
        Guid tenantId,
        DateOnly startDate,
        DateOnly endDate,
        Guid? providerId = null,
        CancellationToken cancellationToken = default);
}
