using System.Security.Cryptography;

namespace Order.Core.Domain.Orders.ValueObjects;

public readonly record struct OrderNumber
{
    public string Value { get; }

    public OrderNumber(string value)
    {
        Value = value;
    }

    public static OrderNumber Create(DateTime utcNow)
    {
        var date = utcNow.ToString("yyyyMMdd");
        var millis = utcNow.ToString("fff");
        var random = RandomNumberGenerator.GetInt32(0, 100000).ToString("D5");

        var value = $"{date}{millis}-{random}";
        return new OrderNumber(value);
    }

    public override string ToString() => Value;
}
