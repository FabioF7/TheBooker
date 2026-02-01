using System.Text.RegularExpressions;
using TheBooker.Domain.Common.Primitives;
using TheBooker.Domain.Common.Results;

namespace TheBooker.Domain.ValueObjects;

/// <summary>
/// Value object representing a validated email address.
/// </summary>
public sealed partial class Email : ValueObject
{
    public const int MaxLength = 256;

    public string Value { get; private init; } = string.Empty;

    private Email() { }

    public static Result<Email> Create(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return DomainErrors.Email.Empty;

        email = email.Trim().ToLowerInvariant();

        if (email.Length > MaxLength)
            return DomainErrors.Email.TooLong;

        if (!EmailRegex().IsMatch(email))
            return DomainErrors.Email.InvalidFormat;

        return new Email { Value = email };
    }

    [GeneratedRegex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", RegexOptions.Compiled)]
    private static partial Regex EmailRegex();

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(Email email) => email.Value;
}
