using MediatR;
using TheBooker.Application.Common.Behaviors;
using TheBooker.Application.Common.Interfaces;
using TheBooker.Domain;
using TheBooker.Domain.Common.Results;

namespace TheBooker.Application.Features.Appointments.Commands;

/// <summary>
/// Command to cancel an appointment.
/// </summary>
public sealed record CancelAppointmentCommand(
    Guid AppointmentId,
    string? Reason) : ICommand;

/// <summary>
/// Handler for CancelAppointmentCommand.
/// </summary>
public sealed class CancelAppointmentCommandHandler
    : IRequestHandler<CancelAppointmentCommand, Result>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CancelAppointmentCommandHandler(
        IAppointmentRepository appointmentRepository,
        IUnitOfWork unitOfWork)
    {
        _appointmentRepository = appointmentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        CancelAppointmentCommand request,
        CancellationToken cancellationToken)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(
            request.AppointmentId, cancellationToken);

        if (appointment is null)
            return DomainErrors.Appointment.NotFoundById(request.AppointmentId);

        var cancelResult = appointment.Cancel(request.Reason);

        if (cancelResult.IsFailure)
            return cancelResult;

        _appointmentRepository.Update(appointment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
