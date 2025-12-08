using System;
using System.Linq;
using Order.Core.Domain.Orders.ValueObjects;
using Xunit;

namespace Order.UnitTests.Domain.Orders.ValueObjects;

public class OrderNumberTests
{
    [Fact]
    public void Create_ShouldGenerateOrderNumberBasedOnDateAndRandom()
    {
        var utcNow = new DateTime(2025, 12, 7, 10, 20, 30, 123, DateTimeKind.Utc);
        
        // Formato esperado: 20251207123-rrrrr
        
        var orderNumber = OrderNumber.Create(utcNow);
        var value = orderNumber.Value;


        Assert.Equal(17, value.Length);

        var datePart = value.Substring(0, 8);
        var millisPart = value.Substring(8, 3);
        var hyphen = value[11];
        var randomPart = value.Substring(12);

        Assert.Equal("20251207", datePart);
        Assert.Equal("123", millisPart);
        Assert.Equal('-', hyphen);

        Assert.Equal(5, randomPart.Length);
        Assert.True(randomPart.All(char.IsDigit));
    }

    [Fact]
    public void FromString_ShouldWrapValue()
    {
        var raw = "20251207123-12345";

        var orderNumber = OrderNumber.FromString(raw);

        Assert.Equal(raw, orderNumber.Value);
        Assert.Equal(raw, orderNumber.ToString());
    }
}
