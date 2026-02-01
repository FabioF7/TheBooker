using TheBooker.Domain.Common.Primitives;

namespace TheBooker.Domain.Enums;

/// <summary>
/// Types of schedule overrides.
/// </summary>
public sealed class OverrideType : Enumeration<OverrideType>
{
    /// <summary>
    /// Day is completely closed (holiday, vacation).
    /// </summary>
    public static readonly OverrideType Closed = new(1, nameof(Closed));

    /// <summary>
    /// Modified hours for the day (partial shift).
    /// </summary>
    public static readonly OverrideType ModifiedHours = new(2, nameof(ModifiedHours));

    /// <summary>
    /// Additional availability outside normal hours.
    /// </summary>
    public static readonly OverrideType ExtendedHours = new(3, nameof(ExtendedHours));

    private OverrideType(int id, string name) : base(id, name) { }
}
