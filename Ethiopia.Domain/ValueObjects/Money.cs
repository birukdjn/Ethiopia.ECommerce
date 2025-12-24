namespace Ethiopia.Domain.ValueObjects;

public sealed class Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    private static readonly HashSet<string> ValidCurrencies = new() { "USD", "EUR", "GBP", "ETB" };

    public Money(decimal amount, string currency)
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative", nameof(amount));
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency is required");

        if (!ValidCurrencies.Contains(currency.ToUpper()))
            throw new ArgumentException($"Invalid currency: {currency}", nameof(currency));

        Amount = decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
        Currency = currency.ToUpper();
    }

    public static Money FromUsd(decimal amount) => new(amount, "USD");
    public static Money FromEtb(decimal amount) => new(amount, "ETB");

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot add {Currency} and {other.Currency}");

        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot subtract {Currency} and {other.Currency}");

        return new Money(Amount - other.Amount, Currency);
    }

    public Money Multiply(decimal multiplier) =>
        new(Amount * multiplier, Currency);

    public Money ApplyPercentage(decimal percentage) =>
        Multiply(percentage / 100);

    public override string ToString() => $"{Currency} {Amount:N2}";

    public static bool operator <=(Money left, Money right) =>
        left.Currency == right.Currency && left.Amount <= right.Amount;

    public static bool operator >=(Money left, Money right) =>
        left.Currency == right.Currency && left.Amount >= right.Amount;
}