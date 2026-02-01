namespace TheBooker.Application.Services.Availability;

/// <summary>
/// Represents an available time slot for booking.
/// </summary>
public sealed record AvailableSlot(
    TimeOnly StartTime,
    TimeOnly EndTime,
    int DurationMinutes,
    bool IsAvailable = true);

/// <summary>
/// Request parameters for availability calculation.
/// </summary>
public sealed record AvailabilityRequest(
    Guid TenantId,
    Guid ProviderId,
    Guid ServiceId,
    DateOnly Date,
    int ServiceDurationMinutes,
    int SlotIntervalMinutes = 15);

/// <summary>
/// Response containing available slots for a date.
/// </summary>
public sealed record AvailabilityResponse(
    DateOnly Date,
    Guid ProviderId,
    string ProviderName,
    bool IsOpen,
    TimeOnly? OpenTime,
    TimeOnly? CloseTime,
    IReadOnlyList<AvailableSlot> Slots,
    string? ClosedReason = null);
