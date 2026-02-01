using TheBooker.Domain.Common.Primitives;

namespace TheBooker.Domain.Events;

/// <summary>
/// Event raised when an appointment slot is held (pending).
/// </summary>
public sealed record AppointmentHeldEvent(
    Guid AppointmentId,
    Guid TenantId,
    DateOnly Date,
    TimeOnly StartTime) : DomainEventBase;

/// <summary>
/// Event raised when an appointment is confirmed.
/// </summary>
public sealed record AppointmentConfirmedEvent(
    Guid AppointmentId,
    Guid TenantId,
    string CustomerEmail) : DomainEventBase;

/// <summary>
/// Event raised when an appointment is cancelled.
/// </summary>
public sealed record AppointmentCancelledEvent(
    Guid AppointmentId,
    Guid TenantId,
    string PreviousStatus) : DomainEventBase;

/// <summary>
/// Event raised when an appointment is marked as no-show.
/// </summary>
public sealed record AppointmentNoShowEvent(
    Guid AppointmentId,
    Guid TenantId) : DomainEventBase;

/// <summary>
/// Event raised when an appointment is completed.
/// </summary>
public sealed record AppointmentCompletedEvent(
    Guid AppointmentId,
    Guid TenantId) : DomainEventBase;

/// <summary>
/// Event raised when pending appointments expire.
/// </summary>
public sealed record AppointmentExpiredEvent(
    Guid AppointmentId,
    Guid TenantId) : DomainEventBase;
