using MediatR;
using TheBooker.Application.Common.Behaviors;
using TheBooker.Application.Common.Interfaces;
using TheBooker.Domain;
using TheBooker.Domain.Common.Results;
using TheBooker.Domain.Entities;

namespace TheBooker.Application.Features.Appointments.Commands;

/// <summary>
/// Command to hold (soft-lock) a time slot.
/// Creates a PENDING appointment that expires after 10 minutes.
/// </summary>
public sealed record HoldSlotCommand(
    Guid TenantId,
    Guid ServiceId,
    Guid ProviderId,
    DateOnly Date,
    TimeOnly StartTime,
    string SessionId) : ICommand<HoldSlotResponse>;

public sealed record HoldSlotResponse(
    Guid AppointmentId,
    DateTime ExpiresAt);

/// <summary>
/// Handler for HoldSlotCommand.
/// Validates availability and creates a pending appointment.
/// </summary>
public sealed class HoldSlotCommandHandler
    : IRequestHandler<HoldSlotCommand, Result<HoldSlotResponse>>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IServiceProviderRepository _providerRepository;
    private readonly IUnitOfWork _unitOfWork;

    private const int LockDurationMinutes = 10;

    public HoldSlotCommandHandler(
        ITenantRepository tenantRepository,
        IAppointmentRepository appointmentRepository,
        IServiceProviderRepository providerRepository,
        IUnitOfWork unitOfWork)
    {
        _tenantRepository = tenantRepository;
        _appointmentRepository = appointmentRepository;
        _providerRepository = providerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<HoldSlotResponse>> Handle(
        HoldSlotCommand request,
        CancellationToken cancellationToken)
    {
        // Validate tenant exists
        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant is null)
            return Error.NotFound("Tenant", request.TenantId);

        // Get provider with services
        var provider = await _providerRepository.GetWithServicesAsync(request.ProviderId, cancellationToken);
        if (provider is null)
            return Error.NotFound("ServiceProvider", request.ProviderId);

        // Get service duration
        var service = provider.Services.FirstOrDefault(s => s.Id == request.ServiceId);
        if (service is null)
            return Error.NotFound("Service", request.ServiceId);

        var endTime = request.StartTime.AddMinutes(service.DurationMinutes);

        // Race condition check: verify slot is still available
        var hasConflict = await _appointmentRepository.HasConflictAsync(
            request.ProviderId,
            request.Date,
            request.StartTime,
            endTime,
            cancellationToken: cancellationToken);

        if (hasConflict)
            return DomainErrors.Appointment.SlotNotAvailable;

        // Create the held appointment
        var appointmentResult = Appointment.Hold(
            request.TenantId,
            request.ServiceId,
            request.ProviderId,
            request.Date,
            request.StartTime,
            service.DurationMinutes,
            request.SessionId,
            LockDurationMinutes);

        if (appointmentResult.IsFailure)
            return appointmentResult.Error;

        var appointment = appointmentResult.Value;

        await _appointmentRepository.AddAsync(appointment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new HoldSlotResponse(
            appointment.Id,
            appointment.ExpiresAt!.Value);
    }
}
