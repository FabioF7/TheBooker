using TheBooker.Domain.Common.Primitives;
using TheBooker.Domain.Common.Results;

namespace TheBooker.Domain.ValueObjects;

/// <summary>
/// Value object for customer information on appointments.
/// </summary>
public sealed class CustomerInfo : ValueObject
{
    public string Name { get; private init; } = string.Empty;
    public Email Email { get; private init; } = null!;
    public string? Phone { get; private init; }
    public string? Notes { get; private init; }

    private CustomerInfo() { }

    public static Result<CustomerInfo> Create(
        string name,
        string email,
        string? phone = null,
        string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return DomainErrors.CustomerInfo.NameRequired;

        if (name.Length > 100)
            return DomainErrors.CustomerInfo.NameTooLong;

        var emailResult = Email.Create(email);
        if (emailResult.IsFailure)
            return emailResult.Error;

        return new CustomerInfo
        {
            Name = name.Trim(),
            Email = emailResult.Value,
            Phone = phone?.Trim(),
            Notes = notes?.Trim()
        };
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Name;
        yield return Email;
        yield return Phone;
        yield return Notes;
    }
}
