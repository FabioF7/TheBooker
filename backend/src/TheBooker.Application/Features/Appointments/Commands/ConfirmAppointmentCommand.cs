using MediatR;
using TheBooker.Application.Common.Behaviors;
using TheBooker.Application.Common.Interfaces;
using TheBooker.Domain;
using TheBooker.Domain.Common.Results;
using TheBooker.Domain.ValueObjects;

namespace TheBooker.Application.Features.Appointments.Commands;

/// <summary>
/// Command to confirm a held appointment.
/// Requires valid session ID and customer information.
/// </summary>
public sealed record ConfirmAppointmentCommand(
    Guid AppointmentId,
    string SessionId,
    string CustomerName,
    string CustomerEmail,
    string? CustomerPhone,
    string? Notes) : ICommand<ConfirmAppointmentResponse>;

public sealed record ConfirmAppointmentResponse(
    Guid AppointmentId,
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string CustomerEmail);

/// <summary>
/// Handler for ConfirmAppointmentCommand.
/// </summary>
public sealed class ConfirmAppointmentCommandHandler
    : IRequestHandler<ConfirmAppointmentCommand, Result<ConfirmAppointmentResponse>>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ConfirmAppointmentCommandHandler(
        IAppointmentRepository appointmentRepository,
        IUnitOfWork unitOfWork)
    {
        _appointmentRepository = appointmentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ConfirmAppointmentResponse>> Handle(
        ConfirmAppointmentCommand request,
        CancellationToken cancellationToken)
    {
        // Get the appointment
        var appointment = await _appointmentRepository.GetByIdAsync(
            request.AppointmentId, cancellationToken);

        if (appointment is null)
            return DomainErrors.Appointment.NotFoundById(request.AppointmentId);

        // Create customer info
        var customerResult = CustomerInfo.Create(
            request.CustomerName,
            request.CustomerEmail,
            request.CustomerPhone,
            request.Notes);

        if (customerResult.IsFailure)
            return customerResult.Error;

        // Confirm the appointment (domain validates session and expiry)
        var confirmResult = appointment.Confirm(
            customerResult.Value,
            request.SessionId,
            request.Notes);

        if (confirmResult.IsFailure)
            return confirmResult.Error;

        _appointmentRepository.Update(appointment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ConfirmAppointmentResponse(
            appointment.Id,
            appointment.Date,
            appointment.StartTime,
            appointment.EndTime,
            request.CustomerEmail);
    }
}
