using MediatR;
using TheBooker.Application.Common.Behaviors;
using TheBooker.Application.Common.Interfaces;
using TheBooker.Application.Services.Availability;
using TheBooker.Domain.Common.Results;

namespace TheBooker.Application.Features.Availability;

/// <summary>
/// Query to get available slots for a provider on a specific date.
/// </summary>
public sealed record GetAvailabilityQuery(
    Guid TenantId,
    Guid ProviderId,
    Guid ServiceId,
    DateOnly Date,
    int SlotIntervalMinutes = 15) : IQuery<AvailabilityResponse>;

/// <summary>
/// Handler for GetAvailabilityQuery.
/// </summary>
public sealed class GetAvailabilityQueryHandler
    : IRequestHandler<GetAvailabilityQuery, Result<AvailabilityResponse>>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IServiceProviderRepository _providerRepository;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IScheduleOverrideRepository _overrideRepository;

    public GetAvailabilityQueryHandler(
        ITenantRepository tenantRepository,
        IServiceProviderRepository providerRepository,
        IAppointmentRepository appointmentRepository,
        IScheduleOverrideRepository overrideRepository)
    {
        _tenantRepository = tenantRepository;
        _providerRepository = providerRepository;
        _appointmentRepository = appointmentRepository;
        _overrideRepository = overrideRepository;
    }

    public async Task<Result<AvailabilityResponse>> Handle(
        GetAvailabilityQuery request,
        CancellationToken cancellationToken)
    {
        // Fetch tenant
        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant is null)
            return Error.NotFound("Tenant", request.TenantId);

        // Fetch provider with services
        var provider = await _providerRepository.GetWithServicesAsync(request.ProviderId, cancellationToken);
        if (provider is null)
            return Error.NotFound("ServiceProvider", request.ProviderId);

        // Get service duration
        var service = provider.Services.FirstOrDefault(s => s.Id == request.ServiceId);
        if (service is null)
            return Error.NotFound("Service", request.ServiceId);

        // Fetch existing appointments for the date
        var appointments = await _appointmentRepository
            .GetActiveByProviderAndDateAsync(request.ProviderId, request.Date, cancellationToken);

        // Fetch schedule overrides
        var overrides = await _overrideRepository
            .GetByDateRangeAsync(request.TenantId, request.Date, request.Date, request.ProviderId, cancellationToken);

        // Calculate availability using the engine
        var engine = new AvailabilityEngine();
        var availability = engine.CalculateAvailability(
            tenant,
            provider,
            appointments,
            overrides,
            service.DurationMinutes,
            request.Date,
            request.SlotIntervalMinutes);

        return availability;
    }
}
