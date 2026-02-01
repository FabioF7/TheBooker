using TheBooker.Domain.Common.Primitives;
using TheBooker.Domain.Common.Results;

namespace TheBooker.Domain.ValueObjects;

/// <summary>
/// Value object representing monetary value with currency.
/// </summary>
public sealed class Money : ValueObject
{
    public decimal Amount { get; private init; }
    public string Currency { get; private init; } = "USD";

    private Money() { }

    public static Result<Money> Create(decimal amount, string currency = "USD")
    {
        if (amount < 0)
            return DomainErrors.Money.NegativeAmount;

        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
            return DomainErrors.Money.InvalidCurrency;

        return new Money
        {
            Amount = Math.Round(amount, 2),
            Currency = currency.ToUpperInvariant()
        };
    }

    public static Money Zero(string currency = "USD") => new() { Amount = 0, Currency = currency };

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot add money with different currencies.");

        return new Money { Amount = Amount + other.Amount, Currency = Currency };
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() => $"{Amount:F2} {Currency}";
}
