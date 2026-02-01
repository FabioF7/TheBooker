using TheBooker.Application.Common.Interfaces;
using TheBooker.Domain.Common.Results;

namespace TheBooker.Application.Services.Availability;

/// <summary>
/// Service interface for availability operations.
/// </summary>
public interface IAvailabilityService
{
    /// <summary>
    /// Gets available slots for a provider on a specific date.
    /// </summary>
    Task<Result<AvailabilityResponse>> GetAvailabilityAsync(
        AvailabilityRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets availability for multiple dates (calendar view).
    /// </summary>
    Task<Result<IReadOnlyList<AvailabilityResponse>>> GetAvailabilityRangeAsync(
        Guid tenantId,
        Guid providerId,
        Guid serviceId,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementation of availability service that orchestrates data fetching and calculation.
/// </summary>
public sealed class AvailabilityService : IAvailabilityService
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IServiceProviderRepository _providerRepository;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IScheduleOverrideRepository _overrideRepository;
    private readonly AvailabilityEngine _engine;

    public AvailabilityService(
        ITenantRepository tenantRepository,
        IServiceProviderRepository providerRepository,
        IAppointmentRepository appointmentRepository,
        IScheduleOverrideRepository overrideRepository)
    {
        _tenantRepository = tenantRepository;
        _providerRepository = providerRepository;
        _appointmentRepository = appointmentRepository;
        _overrideRepository = overrideRepository;
        _engine = new AvailabilityEngine();
    }

    public async Task<Result<AvailabilityResponse>> GetAvailabilityAsync(
        AvailabilityRequest request,
        CancellationToken cancellationToken = default)
    {
        // Fetch tenant
        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant is null)
            return Error.NotFound("Tenant", request.TenantId);

        // Fetch provider
        var provider = await _providerRepository.GetByIdAsync(request.ProviderId, cancellationToken);
        if (provider is null)
            return Error.NotFound("ServiceProvider", request.ProviderId);

        // Fetch existing appointments for the date
        var appointments = await _appointmentRepository
            .GetActiveByProviderAndDateAsync(request.ProviderId, request.Date, cancellationToken);

        // Fetch schedule overrides
        var overrides = await _overrideRepository
            .GetByDateRangeAsync(request.TenantId, request.Date, request.Date, request.ProviderId, cancellationToken);

        // Calculate availability
        var availability = _engine.CalculateAvailability(
            tenant,
            provider,
            appointments,
            overrides,
            request.ServiceDurationMinutes,
            request.Date,
            request.SlotIntervalMinutes);

        return availability;
    }

    public async Task<Result<IReadOnlyList<AvailabilityResponse>>> GetAvailabilityRangeAsync(
        Guid tenantId,
        Guid providerId,
        Guid serviceId,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default)
    {
        // Fetch tenant
        var tenant = await _tenantRepository.GetByIdAsync(tenantId, cancellationToken);
        if (tenant is null)
            return Error.NotFound("Tenant", tenantId);

        // Fetch provider with services
        var provider = await _providerRepository.GetWithServicesAsync(providerId, cancellationToken);
        if (provider is null)
            return Error.NotFound("ServiceProvider", providerId);

        // Get service duration
        var service = provider.Services.FirstOrDefault(s => s.Id == serviceId);
        if (service is null)
            return Error.NotFound("Service", serviceId);

        // Fetch all overrides for the date range
        var overrides = await _overrideRepository
            .GetByDateRangeAsync(tenantId, startDate, endDate, providerId, cancellationToken);

        var results = new List<AvailabilityResponse>();

        // Process each date
        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            // Fetch appointments for this specific date
            var appointments = await _appointmentRepository
                .GetActiveByProviderAndDateAsync(providerId, date, cancellationToken);

            // Filter overrides for this date
            var dateOverrides = overrides
                .Where(o => o.AppliesToDate(date))
                .ToList();

            var availability = _engine.CalculateAvailability(
                tenant,
                provider,
                appointments,
                dateOverrides,
                service.DurationMinutes,
                date);

            results.Add(availability);
        }

        return results;
    }
}
