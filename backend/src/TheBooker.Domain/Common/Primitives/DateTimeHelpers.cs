namespace TheBooker.Domain.Common.Primitives;

/// <summary>
/// Helper methods for working with DateOnly and TimeOnly in scheduling context.
/// </summary>
public static class DateTimeHelpers
{
    /// <summary>
    /// Converts DateOnly and TimeOnly to DateTime in specified timezone, then to UTC.
    /// </summary>
    public static DateTime ToUtc(DateOnly date, TimeOnly time, string timeZoneId)
    {
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        var localDateTime = date.ToDateTime(time);
        return TimeZoneInfo.ConvertTimeToUtc(localDateTime, timeZone);
    }

    /// <summary>
    /// Converts UTC DateTime to local DateOnly in specified timezone.
    /// </summary>
    public static DateOnly ToLocalDate(DateTime utcDateTime, string timeZoneId)
    {
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        var localDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, timeZone);
        return DateOnly.FromDateTime(localDateTime);
    }

    /// <summary>
    /// Converts UTC DateTime to local TimeOnly in specified timezone.
    /// </summary>
    public static TimeOnly ToLocalTime(DateTime utcDateTime, string timeZoneId)
    {
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        var localDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, timeZone);
        return TimeOnly.FromDateTime(localDateTime);
    }

    /// <summary>
    /// Gets the start of day in UTC for a given date in specified timezone.
    /// </summary>
    public static DateTime GetDayStartUtc(DateOnly date, string timeZoneId)
    {
        return ToUtc(date, TimeOnly.MinValue, timeZoneId);
    }

    /// <summary>
    /// Gets the end of day in UTC for a given date in specified timezone.
    /// </summary>
    public static DateTime GetDayEndUtc(DateOnly date, string timeZoneId)
    {
        // End of day is start of next day minus 1 tick
        return ToUtc(date.AddDays(1), TimeOnly.MinValue, timeZoneId).AddTicks(-1);
    }

    /// <summary>
    /// Checks if a time range spans across midnight.
    /// </summary>
    public static bool SpansMidnight(TimeOnly start, TimeOnly end)
    {
        return end < start;
    }

    /// <summary>
    /// Calculates the duration in minutes between two times, handling midnight crossing.
    /// </summary>
    public static int GetDurationMinutes(TimeOnly start, TimeOnly end)
    {
        if (end >= start)
        {
            return (int)(end.ToTimeSpan() - start.ToTimeSpan()).TotalMinutes;
        }

        // Spans midnight
        var beforeMidnight = TimeOnly.MaxValue.ToTimeSpan() - start.ToTimeSpan();
        var afterMidnight = end.ToTimeSpan();
        return (int)(beforeMidnight + afterMidnight).TotalMinutes + 1;
    }

    /// <summary>
    /// Adds minutes to a time, returning the new time.
    /// </summary>
    public static TimeOnly AddMinutes(TimeOnly time, int minutes)
    {
        return time.AddMinutes(minutes);
    }

    /// <summary>
    /// Checks if a time falls within a range (inclusive start, exclusive end).
    /// </summary>
    public static bool IsInRange(TimeOnly time, TimeOnly start, TimeOnly end)
    {
        if (end > start)
        {
            return time >= start && time < end;
        }

        // Range spans midnight
        return time >= start || time < end;
    }
}
