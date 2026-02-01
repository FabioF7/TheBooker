using System.Text.RegularExpressions;
using TheBooker.Domain.Common.Primitives;
using TheBooker.Domain.Common.Results;

namespace TheBooker.Domain.ValueObjects;

/// <summary>
/// Value object representing a URL-safe slug for tenant identification.
/// </summary>
public sealed partial class Slug : ValueObject
{
    public const int MinLength = 3;
    public const int MaxLength = 50;

    public string Value { get; private init; } = string.Empty;

    private Slug() { }

    public static Result<Slug> Create(string? slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
            return DomainErrors.Slug.Empty;

        slug = slug.Trim().ToLowerInvariant();

        if (slug.Length < MinLength)
            return DomainErrors.Slug.TooShort;

        if (slug.Length > MaxLength)
            return DomainErrors.Slug.TooLong;

        if (!SlugRegex().IsMatch(slug))
            return DomainErrors.Slug.InvalidFormat;

        return new Slug { Value = slug };
    }

    /// <summary>
    /// Creates a slug from a name by converting to lowercase and replacing spaces/special chars with hyphens.
    /// </summary>
    public static Result<Slug> CreateFromName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return DomainErrors.Slug.Empty;

        var slug = name.Trim().ToLowerInvariant();
        slug = NonAlphanumericRegex().Replace(slug, "-");
        slug = MultipleHyphensRegex().Replace(slug, "-");
        slug = slug.Trim('-');

        return Create(slug);
    }

    [GeneratedRegex(@"^[a-z0-9]+(?:-[a-z0-9]+)*$", RegexOptions.Compiled)]
    private static partial Regex SlugRegex();

    [GeneratedRegex(@"[^a-z0-9]+", RegexOptions.Compiled)]
    private static partial Regex NonAlphanumericRegex();

    [GeneratedRegex(@"-+", RegexOptions.Compiled)]
    private static partial Regex MultipleHyphensRegex();

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(Slug slug) => slug.Value;
}
