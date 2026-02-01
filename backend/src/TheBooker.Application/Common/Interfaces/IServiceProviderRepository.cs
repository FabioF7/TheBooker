using TheBooker.Domain.Entities;

namespace TheBooker.Application.Common.Interfaces;

/// <summary>
/// Repository interface for ServiceProvider entity.
/// </summary>
public interface IServiceProviderRepository : IRepository<ServiceProvider>
{
    /// <summary>
    /// Gets all active providers for a tenant.
    /// </summary>
    Task<IReadOnlyList<ServiceProvider>> GetActiveByTenantAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets providers that offer a specific service.
    /// </summary>
    Task<IReadOnlyList<ServiceProvider>> GetByServiceAsync(
        Guid serviceId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a provider with their assigned services.
    /// </summary>
    Task<ServiceProvider?> GetWithServicesAsync(
        Guid providerId,
        CancellationToken cancellationToken = default);
}
