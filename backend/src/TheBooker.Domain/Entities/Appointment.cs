using TheBooker.Domain.Common.Primitives;
using TheBooker.Domain.Common.Results;
using TheBooker.Domain.Enums;
using TheBooker.Domain.Events;
using TheBooker.Domain.ValueObjects;

namespace TheBooker.Domain.Entities;

/// <summary>
/// Appointment aggregate root - the core booking entity.
/// Implements soft-lock pattern for concurrent booking scenarios.
/// </summary>
public sealed class Appointment : AggregateRoot, IAuditableEntity, ITenantEntity
{
    private Appointment(Guid id) : base(id) { }

    // For EF Core
    private Appointment() { }

    // Identity
    public Guid TenantId { get; private set; }
    public Guid ServiceId { get; private set; }
    public Guid ProviderId { get; private set; }

    // Scheduling
    public DateOnly Date { get; private set; }
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }
    public int DurationMinutes { get; private set; }

    // Status & Soft Lock
    public AppointmentStatus Status { get; private set; } = null!;
    public DateTime? LockedAt { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public string? SessionId { get; private set; }

    // Customer Info (null until confirmed)
    public CustomerInfo? Customer { get; private set; }

    // Notes
    public string? Notes { get; private set; }
    public string? CancellationReason { get; private set; }

    // Audit fields
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? ModifiedAtUtc { get; private set; }

    // Navigation
    public Tenant Tenant { get; private set; } = null!;
    public Service Service { get; private set; } = null!;
    public ServiceProvider Provider { get; private set; } = null!;

    /// <summary>
    /// Creates a held/pending appointment (soft lock).
    /// The slot is locked for 10 minutes until confirmed or expired.
    /// </summary>
    public static Result<Appointment> Hold(
        Guid tenantId,
        Guid serviceId,
        Guid providerId,
        DateOnly date,
        TimeOnly startTime,
        int durationMinutes,
        string sessionId,
        int lockMinutes = 10)
    {
        if (durationMinutes <= 0)
            return Error.Validation("Appointment.InvalidDuration", "Duration must be positive.");

        if (string.IsNullOrWhiteSpace(sessionId))
            return Error.Validation("Appointment.SessionRequired", "Session ID is required for holding slots.");

        var endTime = startTime.AddMinutes(durationMinutes);

        var appointment = new Appointment(Guid.NewGuid())
        {
            TenantId = tenantId,
            ServiceId = serviceId,
            ProviderId = providerId,
            Date = date,
            StartTime = startTime,
            EndTime = endTime,
            DurationMinutes = durationMinutes,
            Status = AppointmentStatus.Pending,
            LockedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(lockMinutes),
            SessionId = sessionId,
            CreatedAtUtc = DateTime.UtcNow
        };

        appointment.AddDomainEvent(new AppointmentHeldEvent(
            appointment.Id,
            appointment.TenantId,
            appointment.Date,
            appointment.StartTime));

        return appointment;
    }

    /// <summary>
    /// Confirms the appointment with customer information.
    /// </summary>
    public Result Confirm(CustomerInfo customer, string sessionId, string? notes = null)
    {
        if (!Status.CanConfirm)
            return DomainErrors.Appointment.CannotConfirm;

        if (SessionId != sessionId)
            return DomainErrors.Appointment.InvalidSessionId;

        if (ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value)
            return DomainErrors.Appointment.LockExpired;

        Status = AppointmentStatus.Confirmed;
        Customer = customer;
        Notes = notes?.Trim();
        LockedAt = null;
        ExpiresAt = null;
        SessionId = null;
        ModifiedAtUtc = DateTime.UtcNow;

        AddDomainEvent(new AppointmentConfirmedEvent(Id, TenantId, Customer.Email.Value));

        return Result.Success();
    }

    /// <summary>
    /// Cancels the appointment.
    /// </summary>
    public Result Cancel(string? reason = null)
    {
        if (!Status.CanCancel)
            return DomainErrors.Appointment.CannotCancel;

        var previousStatus = Status;
        Status = AppointmentStatus.Cancelled;
        CancellationReason = reason?.Trim();
        LockedAt = null;
        ExpiresAt = null;
        SessionId = null;
        ModifiedAtUtc = DateTime.UtcNow;

        AddDomainEvent(new AppointmentCancelledEvent(Id, TenantId, previousStatus.Name));

        return Result.Success();
    }

    /// <summary>
    /// Marks the appointment as no-show.
    /// </summary>
    public Result MarkNoShow()
    {
        if (!Status.CanMarkNoShow)
            return Error.Validation("Appointment.CannotMarkNoShow",
                "Only confirmed appointments can be marked as no-show.");

        Status = AppointmentStatus.NoShow;
        ModifiedAtUtc = DateTime.UtcNow;

        return Result.Success();
    }

    /// <summary>
    /// Marks the appointment as completed.
    /// </summary>
    public Result Complete()
    {
        if (!Status.CanComplete)
            return Error.Validation("Appointment.CannotComplete",
                "Only confirmed appointments can be marked as completed.");

        Status = AppointmentStatus.Completed;
        ModifiedAtUtc = DateTime.UtcNow;

        return Result.Success();
    }

    /// <summary>
    /// Checks if the soft lock has expired.
    /// </summary>
    public bool IsLockExpired() =>
        Status == AppointmentStatus.Pending &&
        ExpiresAt.HasValue &&
        DateTime.UtcNow > ExpiresAt.Value;

    /// <summary>
    /// Checks if this appointment occupies a given time slot.
    /// </summary>
    public bool OccupiesSlot(TimeOnly slotStart, TimeOnly slotEnd)
    {
        if (!Status.OccupiesSlot)
            return false;

        // For pending, only if not expired
        if (Status == AppointmentStatus.Pending && IsLockExpired())
            return false;

        return StartTime < slotEnd && EndTime > slotStart;
    }
}
