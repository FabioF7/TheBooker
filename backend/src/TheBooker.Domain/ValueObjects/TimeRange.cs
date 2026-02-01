using TheBooker.Domain.Common.Primitives;
using TheBooker.Domain.Common.Results;

namespace TheBooker.Domain.ValueObjects;

/// <summary>
/// Value object representing a time range within a day.
/// </summary>
public sealed class TimeRange : ValueObject
{
    public TimeOnly Start { get; private init; }
    public TimeOnly End { get; private init; }

    private TimeRange() { }

    public static Result<TimeRange> Create(TimeOnly start, TimeOnly end)
    {
        if (end <= start)
            return DomainErrors.TimeRange.EndBeforeStart;

        return new TimeRange { Start = start, End = end };
    }

    /// <summary>
    /// Gets the duration in minutes.
    /// </summary>
    public int DurationMinutes => (int)(End.ToTimeSpan() - Start.ToTimeSpan()).TotalMinutes;

    /// <summary>
    /// Checks if this range overlaps with another.
    /// </summary>
    public bool Overlaps(TimeRange other)
    {
        return Start < other.End && End > other.Start;
    }

    /// <summary>
    /// Checks if this range contains a specific time.
    /// </summary>
    public bool Contains(TimeOnly time)
    {
        return time >= Start && time < End;
    }

    /// <summary>
    /// Checks if this range fully contains another range.
    /// </summary>
    public bool Contains(TimeRange other)
    {
        return Start <= other.Start && End >= other.End;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Start;
        yield return End;
    }

    public override string ToString() => $"{Start:HH:mm} - {End:HH:mm}";
}
