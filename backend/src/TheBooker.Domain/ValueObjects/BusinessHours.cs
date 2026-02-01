using System.Text.Json;
using System.Text.Json.Serialization;
using TheBooker.Domain.Common.Primitives;
using TheBooker.Domain.Common.Results;

namespace TheBooker.Domain.ValueObjects;

/// <summary>
/// Represents business hours for a single day.
/// Simple record for JSON serialization.
/// </summary>
public sealed record DayScheduleData(
    bool IsOpen,
    string? OpenTime,
    string? CloseTime);

/// <summary>
/// Value object representing weekly business hours.
/// Stored as JSON string in database.
/// </summary>
public sealed class BusinessHours : ValueObject
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    [JsonPropertyName("monday")]
    public DayScheduleData Monday { get; set; } = new(false, null, null);
    [JsonPropertyName("tuesday")]
    public DayScheduleData Tuesday { get; set; } = new(false, null, null);
    [JsonPropertyName("wednesday")]
    public DayScheduleData Wednesday { get; set; } = new(false, null, null);
    [JsonPropertyName("thursday")]
    public DayScheduleData Thursday { get; set; } = new(false, null, null);
    [JsonPropertyName("friday")]
    public DayScheduleData Friday { get; set; } = new(false, null, null);
    [JsonPropertyName("saturday")]
    public DayScheduleData Saturday { get; set; } = new(false, null, null);
    [JsonPropertyName("sunday")]
    public DayScheduleData Sunday { get; set; } = new(false, null, null);

    // For EF Core and JSON deserialization
    public BusinessHours() { }

    public static BusinessHours CreateDefault()
    {
        var openDay = new DayScheduleData(true, "09:00", "17:00");
        var closedDay = new DayScheduleData(false, null, null);

        return new BusinessHours
        {
            Monday = openDay,
            Tuesday = openDay,
            Wednesday = openDay,
            Thursday = openDay,
            Friday = openDay,
            Saturday = closedDay,
            Sunday = closedDay
        };
    }

    public static Result<BusinessHours> Create(
        DayScheduleData monday,
        DayScheduleData tuesday,
        DayScheduleData wednesday,
        DayScheduleData thursday,
        DayScheduleData friday,
        DayScheduleData saturday,
        DayScheduleData sunday)
    {
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
    public (bool IsOpen, TimeOnly? OpenTime, TimeOnly? CloseTime) GetScheduleForDay(DayOfWeek dayOfWeek)
    {
        var day = dayOfWeek switch
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

        TimeOnly? openTime = day.OpenTime != null ? TimeOnly.Parse(day.OpenTime) : null;
        TimeOnly? closeTime = day.CloseTime != null ? TimeOnly.Parse(day.CloseTime) : null;

        return (day.IsOpen, openTime, closeTime);
    }

    public string ToJson() => JsonSerializer.Serialize(this, JsonOptions);

    public static BusinessHours FromJson(string json) =>
        JsonSerializer.Deserialize<BusinessHours>(json, JsonOptions) ?? CreateDefault();

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

/// <summary>
/// Helper wrapper for DaySchedule used by availability engine.
/// </summary>
public sealed class DaySchedule
{
    public bool IsOpen { get; init; }
    public TimeOnly? OpenTime { get; init; }
    public TimeOnly? CloseTime { get; init; }

    public static DaySchedule FromData(DayScheduleData data)
    {
        return new DaySchedule
        {
            IsOpen = data.IsOpen,
            OpenTime = data.OpenTime != null ? TimeOnly.Parse(data.OpenTime) : null,
            CloseTime = data.CloseTime != null ? TimeOnly.Parse(data.CloseTime) : null
        };
    }

    public static DaySchedule Closed() => new() { IsOpen = false };
}
