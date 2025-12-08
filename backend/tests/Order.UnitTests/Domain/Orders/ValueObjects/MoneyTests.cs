using Order.Core.Domain.Orders.ValueObjects;
using Xunit;
using System.Globalization;


namespace Order.UnitTests.Domain.Orders.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void FromDecimal_ShouldSetAmount()
    {
        decimal value = 10.5m;
        var money = Money.FromDecimal(value);

        Assert.Equal(value, money.Amount);
    }

    [Fact]
    public void Add_ShouldSumAmounts()
    {

        var a = Money.FromDecimal(10m);
        var b = Money.FromDecimal(5.5m);

        var result = a.Add(b);

        Assert.Equal(15.5m, result.Amount);
    }

    [Fact]
    public void Subtract_ShouldSubtractAmounts()
    {

        var a = Money.FromDecimal(10m);
        var b = Money.FromDecimal(3m);

        var result = a.Subtract(b);

        Assert.Equal(7m, result.Amount);
    }

    [Fact]
    public void Multiply_ShouldMultiplyByQuantity()
    {
        var money = Money.FromDecimal(10m);

        var result = money.Multiply(3);

        Assert.Equal(30m, result.Amount);
    }

    [Fact]
    public void ToString_ShouldFormatWithTwoDecimalPlaces()
    {

        var money = Money.FromDecimal(10m);

        var str = money.ToString();

        Assert.Equal("10.00", str);
    }

    [Fact]
    public void Equality_ShouldCompareByAmount()
    {

        var a = Money.FromDecimal(10m);
        var b = Money.FromDecimal(10m);
        var c = Money.FromDecimal(15m);

        Assert.True(a == b);
        Assert.True(a.Equals(b));
        Assert.False(a == c);
    }
}
