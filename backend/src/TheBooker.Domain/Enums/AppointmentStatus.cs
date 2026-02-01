using TheBooker.Domain.Common.Primitives;

namespace TheBooker.Domain.Enums;

/// <summary>
/// Smart enumeration for appointment statuses with behavior.
/// </summary>
public sealed class AppointmentStatus : Enumeration<AppointmentStatus>
{
    public static readonly AppointmentStatus Pending = new(1, nameof(Pending));
    public static readonly AppointmentStatus Confirmed = new(2, nameof(Confirmed));
    public static readonly AppointmentStatus Cancelled = new(3, nameof(Cancelled));
    public static readonly AppointmentStatus NoShow = new(4, nameof(NoShow));
    public static readonly AppointmentStatus Completed = new(5, nameof(Completed));

    private AppointmentStatus(int id, string name) : base(id, name) { }

    /// <summary>
    /// Determines if this status represents an active/occupied slot.
    /// </summary>
    public bool OccupiesSlot => this == Pending || this == Confirmed;

    /// <summary>
    /// Determines if the appointment can be cancelled from this status.
    /// </summary>
    public bool CanCancel => this == Pending || this == Confirmed;

    /// <summary>
    /// Determines if the appointment can be confirmed from this status.
    /// </summary>
    public bool CanConfirm => this == Pending;

    /// <summary>
    /// Determines if the appointment can be marked as no-show.
    /// </summary>
    public bool CanMarkNoShow => this == Confirmed;

    /// <summary>
    /// Determines if the appointment can be completed.
    /// </summary>
    public bool CanComplete => this == Confirmed;
}
