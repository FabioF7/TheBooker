using TheBooker.Domain.Common.Primitives;
using TheBooker.Domain.Common.Results;
using TheBooker.Domain.Enums;
using TheBooker.Domain.ValueObjects;

namespace TheBooker.Domain.Entities;

/// <summary>
/// Schedule override for handling closed days or modified hours.
/// Can apply to a specific provider or the entire tenant.
/// </summary>
public sealed class ScheduleOverride : Entity, IAuditableEntity, ITenantEntity
{
    private ScheduleOverride(Guid id) : base(id) { }

    // For EF Core
    private ScheduleOverride() { }

    public Guid TenantId { get; private set; }
    public Guid? ProviderId { get; private set; } // Null = applies to entire tenant
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public OverrideType Type { get; private set; } = null!;
    public TimeRange? ModifiedHours { get; private set; } // For ModifiedHours/ExtendedHours types
    public string? Reason { get; private set; }

    // Audit fields
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? ModifiedAtUtc { get; private set; }

    // Navigation
    public Tenant Tenant { get; private set; } = null!;
    public ServiceProvider? Provider { get; private set; }

    /// <summary>
    /// Creates a closed day override (e.g., holiday).
    /// </summary>
    public static Result<ScheduleOverride> CreateClosed(
        Guid tenantId,
        DateOnly startDate,
        DateOnly endDate,
        string? reason = null,
        Guid? providerId = null)
    {
        if (endDate < startDate)
            return DomainErrors.ScheduleOverride.InvalidDateRange;

        return new ScheduleOverride(Guid.NewGuid())
        {
            TenantId = tenantId,
            ProviderId = providerId,
            StartDate = startDate,
            EndDate = endDate,
            Type = OverrideType.Closed,
            Reason = reason?.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a modified hours override (e.g., early close).
    /// </summary>
    public static Result<ScheduleOverride> CreateModifiedHours(
        Guid tenantId,
        DateOnly startDate,
        DateOnly endDate,
        TimeOnly openTime,
        TimeOnly closeTime,
        string? reason = null,
        Guid? providerId = null)
    {
        if (endDate < startDate)
            return DomainErrors.ScheduleOverride.InvalidDateRange;

        var timeRangeResult = TimeRange.Create(openTime, closeTime);
        if (timeRangeResult.IsFailure)
            return timeRangeResult.Error;

        return new ScheduleOverride(Guid.NewGuid())
        {
            TenantId = tenantId,
            ProviderId = providerId,
            StartDate = startDate,
            EndDate = endDate,
            Type = OverrideType.ModifiedHours,
            ModifiedHours = timeRangeResult.Value,
            Reason = reason?.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates an extended hours override (additional availability).
    /// </summary>
    public static Result<ScheduleOverride> CreateExtendedHours(
        Guid tenantId,
        DateOnly startDate,
        DateOnly endDate,
        TimeOnly openTime,
        TimeOnly closeTime,
        string? reason = null,
        Guid? providerId = null)
    {
        if (endDate < startDate)
            return DomainErrors.ScheduleOverride.InvalidDateRange;

        var timeRangeResult = TimeRange.Create(openTime, closeTime);
        if (timeRangeResult.IsFailure)
            return timeRangeResult.Error;

        return new ScheduleOverride(Guid.NewGuid())
        {
            TenantId = tenantId,
            ProviderId = providerId,
            StartDate = startDate,
            EndDate = endDate,
            Type = OverrideType.ExtendedHours,
            ModifiedHours = timeRangeResult.Value,
            Reason = reason?.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Checks if this override applies to a specific date.
    /// </summary>
    public bool AppliesToDate(DateOnly date) =>
        date >= StartDate && date <= EndDate;

    /// <summary>
    /// Checks if this override applies to a specific provider (or is tenant-wide).
    /// </summary>
    public bool AppliesToProvider(Guid? providerId) =>
        ProviderId is null || ProviderId == providerId;
}
