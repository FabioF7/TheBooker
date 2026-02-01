using TheBooker.Domain.Common.Primitives;
using TheBooker.Domain.Common.Results;

namespace TheBooker.Domain.ValueObjects;

/// <summary>
/// Represents business hours for a single day.
/// </summary>
public sealed class DaySchedule : ValueObject
{
    public bool IsOpen { get; private init; }
    public TimeOnly? OpenTime { get; private init; }
    public TimeOnly? CloseTime { get; private init; }

    private DaySchedule() { }

    public static Result<DaySchedule> Create(bool isOpen, TimeOnly? openTime = null, TimeOnly? closeTime = null)
    {
        if (isOpen)
        {
            if (openTime is null || closeTime is null)
                return DomainErrors.BusinessHours.OpenDayRequiresTimes;

            if (closeTime <= openTime)
                return DomainErrors.BusinessHours.CloseTimeBeforeOpenTime;
        }

        return new DaySchedule
        {
            IsOpen = isOpen,
            OpenTime = isOpen ? openTime : null,
            CloseTime = isOpen ? closeTime : null
        };
    }

    public static DaySchedule Closed() => new() { IsOpen = false };

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return IsOpen;
        yield return OpenTime;
        yield return CloseTime;
    }
}

/// <summary>
/// Value object representing weekly business hours.
/// Designed for JSON serialization to PostgreSQL JSONB.
/// </summary>
public sealed class BusinessHours : ValueObject
{
    public DaySchedule Monday { get; private init; } = DaySchedule.Closed();
    public DaySchedule Tuesday { get; private init; } = DaySchedule.Closed();
    public DaySchedule Wednesday { get; private init; } = DaySchedule.Closed();
    public DaySchedule Thursday { get; private init; } = DaySchedule.Closed();
    public DaySchedule Friday { get; private init; } = DaySchedule.Closed();
    public DaySchedule Saturday { get; private init; } = DaySchedule.Closed();
    public DaySchedule Sunday { get; private init; } = DaySchedule.Closed();

    private BusinessHours() { }

    public static BusinessHours CreateDefault()
    {
        var defaultOpen = new TimeOnly(9, 0);
        var defaultClose = new TimeOnly(17, 0);
        var openDay = DaySchedule.Create(true, defaultOpen, defaultClose).Value;

        return new BusinessHours
        {
            Monday = openDay,
            Tuesday = openDay,
            Wednesday = openDay,
            Thursday = openDay,
            Friday = openDay,
            Saturday = DaySchedule.Closed(),
            Sunday = DaySchedule.Closed()
        };
    }

    public static Result<BusinessHours> Create(
        DaySchedule monday,
        DaySchedule tuesday,
        DaySchedule wednesday,
        DaySchedule thursday,
        DaySchedule friday,
        DaySchedule saturday,
        DaySchedule sunday)
    {
        // Ensure at least one day is open
        var hasOpenDay = monday.IsOpen || tuesday.IsOpen || wednesday.IsOpen ||
                         thursday.IsOpen || friday.IsOpen || saturday.IsOpen || sunday.IsOpen;

        if (!hasOpenDay)
            return DomainErrors.BusinessHours.AtLeastOneDayMustBeOpen;

        return new BusinessHours
        {
            Monday = monday,
            Tuesday = tuesday,
            Wednesday = wednesday,
            Thursday = thursday,
            Friday = friday,
            Saturday = saturday,
            Sunday = sunday
        };
    }

    /// <summary>
    /// Gets the schedule for a specific day of week.
    /// </summary>
    public DaySchedule GetScheduleForDay(DayOfWeek dayOfWeek) => dayOfWeek switch
    {
        DayOfWeek.Monday => Monday,
        DayOfWeek.Tuesday => Tuesday,
        DayOfWeek.Wednesday => Wednesday,
        DayOfWeek.Thursday => Thursday,
        DayOfWeek.Friday => Friday,
        DayOfWeek.Saturday => Saturday,
        DayOfWeek.Sunday => Sunday,
        _ => throw new ArgumentOutOfRangeException(nameof(dayOfWeek))
    };

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Monday;
        yield return Tuesday;
        yield return Wednesday;
        yield return Thursday;
        yield return Friday;
        yield return Saturday;
        yield return Sunday;
    }
}
