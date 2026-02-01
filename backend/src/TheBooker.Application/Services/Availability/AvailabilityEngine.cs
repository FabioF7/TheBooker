using TheBooker.Domain.Entities;
using TheBooker.Domain.Enums;
using TheBooker.Domain.ValueObjects;

namespace TheBooker.Application.Services.Availability;

/// <summary>
/// Core availability calculation engine.
/// Memory-first approach: fetch data, process in memory, return slots.
/// </summary>
public sealed class AvailabilityEngine
{
    /// <summary>
    /// Calculates available slots for a provider on a given date.
    /// </summary>
    /// <param name="tenant">The tenant with business hours and buffer settings</param>
    /// <param name="provider">The service provider</param>
    /// <param name="existingAppointments">All active appointments for the date</param>
    /// <param name="overrides">Schedule overrides for the date</param>
    /// <param name="serviceDurationMinutes">Duration of the service being booked</param>
    /// <param name="date">The date to calculate availability for</param>
    /// <param name="slotIntervalMinutes">Interval between slot start times (default 15)</param>
    /// <returns>Availability response with slots</returns>
    public AvailabilityResponse CalculateAvailability(
        Tenant tenant,
        ServiceProvider provider,
        IReadOnlyList<Appointment> existingAppointments,
        IReadOnlyList<ScheduleOverride> overrides,
        int serviceDurationMinutes,
        DateOnly date,
        int slotIntervalMinutes = 15)
    {
        // Get effective business hours for this provider
        var businessHours = provider.CustomBusinessHours ?? tenant.BusinessHours;
        var (isOpenDay, openTimeDay, closeTimeDay) = businessHours.GetScheduleForDay(date.DayOfWeek);

        // Check for overrides (provider-specific first, then tenant-wide)
        var applicableOverride = GetApplicableOverride(overrides, provider.Id, date);

        // Determine if day is open and get working hours
        var (isOpen, openTime, closeTime, closedReason) = DetermineWorkingHours(
            isOpenDay, openTimeDay, closeTimeDay, applicableOverride);

        if (!isOpen || !openTime.HasValue || !closeTime.HasValue)
        {
            return new AvailabilityResponse(
                Date: date,
                ProviderId: provider.Id,
                ProviderName: provider.Name,
                IsOpen: false,
                OpenTime: null,
                CloseTime: null,
                Slots: [],
                ClosedReason: closedReason ?? "Closed");
        }

        // Generate all possible slots
        var slots = GenerateSlots(
            openTime.Value,
            closeTime.Value,
            serviceDurationMinutes,
            tenant.BufferMinutes,
            existingAppointments,
            slotIntervalMinutes);

        return new AvailabilityResponse(
            Date: date,
            ProviderId: provider.Id,
            ProviderName: provider.Name,
            IsOpen: true,
            OpenTime: openTime,
            CloseTime: closeTime,
            Slots: slots);
    }

    /// <summary>
    /// Gets the most specific applicable override for the date.
    /// Provider-specific overrides take precedence over tenant-wide.
    /// </summary>
    private static ScheduleOverride? GetApplicableOverride(
        IReadOnlyList<ScheduleOverride> overrides,
        Guid providerId,
        DateOnly date)
    {
        // First check for provider-specific override
        var providerOverride = overrides
            .FirstOrDefault(o => o.ProviderId == providerId && o.AppliesToDate(date));

        if (providerOverride != null)
            return providerOverride;

        // Then check for tenant-wide override
        return overrides
            .FirstOrDefault(o => o.ProviderId == null && o.AppliesToDate(date));
    }

    /// <summary>
    /// Determines working hours based on schedule and overrides.
    /// </summary>
    private static (bool isOpen, TimeOnly? openTime, TimeOnly? closeTime, string? reason)
        DetermineWorkingHours(bool isOpenDay, TimeOnly? openTimeDay, TimeOnly? closeTimeDay, ScheduleOverride? scheduleOverride)
    {
        // If there's an override, it takes precedence
        if (scheduleOverride != null)
        {
            if (scheduleOverride.Type == OverrideType.Closed)
            {
                return (false, null, null, scheduleOverride.Reason ?? "Closed");
            }

            if (scheduleOverride.Type == OverrideType.ModifiedHours &&
                scheduleOverride.ModifiedHours != null)
            {
                return (true,
                    scheduleOverride.ModifiedHours.Start,
                    scheduleOverride.ModifiedHours.End,
                    null);
            }

            // ExtendedHours - could be combined with normal hours
            // For simplicity, use the extended hours directly
            if (scheduleOverride.Type == OverrideType.ExtendedHours &&
                scheduleOverride.ModifiedHours != null)
            {
                return (true,
                    scheduleOverride.ModifiedHours.Start,
                    scheduleOverride.ModifiedHours.End,
                    null);
            }
        }

        // Use regular schedule
        if (!daySchedule.IsOpen)
        {
            return (false, null, null, "Regularly closed");
        }

        return (true, daySchedule.OpenTime, daySchedule.CloseTime, null);
    }

    /// <summary>
    /// Generates time slots with availability status.
    /// Uses List with initial capacity for performance.
    /// </summary>
    private static List<AvailableSlot> GenerateSlots(
        TimeOnly openTime,
        TimeOnly closeTime,
        int serviceDurationMinutes,
        int bufferMinutes,
        IReadOnlyList<Appointment> existingAppointments,
        int slotIntervalMinutes)
    {
        // Calculate approximate number of slots for initial capacity
        var totalMinutes = (closeTime.ToTimeSpan() - openTime.ToTimeSpan()).TotalMinutes;
        var estimatedSlots = (int)(totalMinutes / slotIntervalMinutes) + 1;
        var slots = new List<AvailableSlot>(estimatedSlots);

        var currentTime = openTime;
        var now = DateTime.UtcNow;

        // Filter to only active appointments (Confirmed or non-expired Pending)
        var activeAppointments = existingAppointments
            .Where(a => a.Status == AppointmentStatus.Confirmed ||
                       (a.Status == AppointmentStatus.Pending &&
                        a.ExpiresAt.HasValue &&
                        a.ExpiresAt.Value > now))
            .ToList();

        while (currentTime < closeTime)
        {
            var slotEnd = currentTime.AddMinutes(serviceDurationMinutes);

            // Service must END by closing time (buffer can overflow)
            if (slotEnd > closeTime)
                break;

            // Check if slot conflicts with existing appointments
            var isAvailable = !HasConflict(
                currentTime,
                slotEnd,
                bufferMinutes,
                activeAppointments);

            slots.Add(new AvailableSlot(
                StartTime: currentTime,
                EndTime: slotEnd,
                DurationMinutes: serviceDurationMinutes,
                IsAvailable: isAvailable));

            currentTime = currentTime.AddMinutes(slotIntervalMinutes);
        }

        return slots;
    }

    /// <summary>
    /// Checks if a proposed slot conflicts with existing appointments.
    /// Buffer time is applied AFTER the existing appointment.
    /// </summary>
    private static bool HasConflict(
        TimeOnly proposedStart,
        TimeOnly proposedEnd,
        int bufferMinutes,
        IReadOnlyList<Appointment> appointments)
    {
        foreach (var appointment in appointments)
        {
            // Existing appointment occupies: StartTime to EndTime + Buffer
            var appointmentBlockEnd = appointment.EndTime.AddMinutes(bufferMinutes);

            // Check overlap:
            // Proposed slot conflicts if it starts before appointment block ends
            // AND ends after appointment starts
            if (proposedStart < appointmentBlockEnd && proposedEnd > appointment.StartTime)
            {
                return true;
            }
        }

        return false;
    }
}
