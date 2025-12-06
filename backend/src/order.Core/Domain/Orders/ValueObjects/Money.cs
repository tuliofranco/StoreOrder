namespace Order.Core.Domain.Orders.ValueObjects;

public readonly record struct Money
{
    public decimal Amount { get; }

    private Money(decimal amount)
    {
        Amount = amount;
    }

    public static Money FromDecimal(decimal value) => new Money(value);

    public Money Add(Money other)
        => new Money(Amount + other.Amount);

    public Money Multiply(int quantity)
        => new Money(Amount * quantity);

    public override string ToString()
        => Amount.ToString("F2");
}
