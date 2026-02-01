using System.Reflection;

namespace TheBooker.Domain.Common.Primitives;

/// <summary>
/// Base class for smart enumerations with behavior.
/// Provides type-safe alternatives to C# enums with additional functionality.
/// </summary>
public abstract class Enumeration<TEnum> : IEquatable<Enumeration<TEnum>>
    where TEnum : Enumeration<TEnum>
{
    private static readonly Lazy<Dictionary<int, TEnum>> EnumerationsDictionary =
        new(() => CreateEnumerationDictionary(typeof(TEnum)));

    protected Enumeration(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public int Id { get; protected init; }
    public string Name { get; protected init; }

    public static IReadOnlyCollection<TEnum> GetAll() => EnumerationsDictionary.Value.Values.ToList();

    public static TEnum? FromId(int id) =>
        EnumerationsDictionary.Value.TryGetValue(id, out var enumeration) ? enumeration : null;

    public static TEnum? FromName(string name) =>
        EnumerationsDictionary.Value.Values.SingleOrDefault(e =>
            e.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

    public static bool TryFromId(int id, out TEnum? enumeration) =>
        EnumerationsDictionary.Value.TryGetValue(id, out enumeration);

    public static bool TryFromName(string name, out TEnum? enumeration)
    {
        enumeration = FromName(name);
        return enumeration is not null;
    }

    public bool Equals(Enumeration<TEnum>? other)
    {
        if (other is null) return false;
        return GetType() == other.GetType() && Id == other.Id;
    }

    public override bool Equals(object? obj) => Equals(obj as Enumeration<TEnum>);

    public override int GetHashCode() => Id.GetHashCode();

    public override string ToString() => Name;

    public static bool operator ==(Enumeration<TEnum>? left, Enumeration<TEnum>? right)
    {
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    public static bool operator !=(Enumeration<TEnum>? left, Enumeration<TEnum>? right) => !(left == right);

    private static Dictionary<int, TEnum> CreateEnumerationDictionary(Type enumType)
    {
        return enumType
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(f => f.FieldType == enumType)
            .Select(f => (TEnum)f.GetValue(null)!)
            .ToDictionary(e => e.Id);
    }
}
