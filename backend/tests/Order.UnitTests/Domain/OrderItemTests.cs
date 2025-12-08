using System;
using OrderItemEntity = Order.Core.Domain.Orders.OrderItem;
using Order.Core.Domain.Orders.ValueObjects;
using Xunit;

namespace Order.UnitTests.Domain.Orders;

public class OrderItemTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldInitializeCorrectly()
    {
        var orderId = Guid.NewGuid();

        var item = OrderItemEntity.Create(
            orderId,
            "  Iphone 22 ",
            Money.FromDecimal(10.5m),
            2
        );


        Assert.NotEqual(Guid.Empty, item.Id);
        Assert.Equal(orderId, item.OrderId);
        Assert.Equal("Iphone", item.Description.Split(' ')[0]);
        Assert.Equal(2, item.Quantity);
        Assert.Equal(10.5m, item.UnitPrice.Amount);

        Assert.NotNull(item.ProductId);
        Assert.False(string.IsNullOrWhiteSpace(item.ProductId));
        Assert.True(item.ProductId.Length <= 60);

        Assert.Equal(21m, item.Subtotal.Amount);
    }

    [Fact]
    public void Create_WithEmptyOrderId_ShouldThrow()
    {

        var orderId = Guid.Empty;

        Assert.Throws<ArgumentException>(() =>
            OrderItemEntity.Create(orderId, "Produto A", Money.FromDecimal(10m), 1));
    }

    [Theory]
    [InlineData(null!)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidDescription_ShouldThrow(string? description)
    {

        var orderId = Guid.NewGuid();


        Assert.Throws<ArgumentException>(() =>
            OrderItemEntity.Create(orderId, description, Money.FromDecimal(10m), 1));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-5)]
    public void Create_WithNonPositiveQuantity_ShouldThrow(int quantity)
    {

        var orderId = Guid.NewGuid();


        Assert.Throws<ArgumentOutOfRangeException>(() =>
            OrderItemEntity.Create(orderId, "Produto A", Money.FromDecimal(10m), quantity));
    }

    [Fact]
    public void ChangeQuantity_WithPositiveDelta_ShouldIncreaseQuantity()
    {

        var item = OrderItemEntity.Create(
            Guid.NewGuid(),
            "Produto A",
            Money.FromDecimal(10m),
            2
        );


        item.ChangeQuantity(3); // 2 -> 5


        Assert.Equal(5, item.Quantity);
    }

    [Fact]
    public void ChangeQuantity_WithZeroDelta_ShouldNotChangeQuantity()
    {

        var item = OrderItemEntity.Create(
            Guid.NewGuid(),
            "Produto A",
            Money.FromDecimal(10m),
            2
        );


        item.ChangeQuantity(0);


        Assert.Equal(2, item.Quantity);
    }

    [Fact]
    public void ChangeQuantity_WithNegativeDelta_DownToZero_ShouldSetQuantityToZero()
    {

        var item = OrderItemEntity.Create(
            Guid.NewGuid(),
            "Samsung",
            Money.FromDecimal(10m),
            2
        );


        item.ChangeQuantity(-2); // 2 -> 0


        Assert.Equal(0, item.Quantity);
    }

    [Fact]
    public void ChangeQuantity_WithNegativeDelta_BelowZero_ShouldThrow()
    {

        var item = OrderItemEntity.Create(
            Guid.NewGuid(),
            "MacBook",
            Money.FromDecimal(10m),
            2
        );


        Assert.Throws<InvalidOperationException>(() =>
            item.ChangeQuantity(-3)); // 2 -> -1
    }

    [Fact]
    public void GetTotal_ShouldReturnSubtotal()
    {

        var item = OrderItemEntity.Create(
            Guid.NewGuid(),
            "Mamba Water",
            Money.FromDecimal(10m),
            3
        );


        var total = item.GetTotal();


        Assert.Equal(item.Subtotal.Amount, total.Amount);
        Assert.Equal(30m, total.Amount);
    }

    [Fact]
    public void ProductId_ShouldBeGeneratedWithMaxLength60()
    {

        var item = OrderItemEntity.Create(
            Guid.NewGuid(),
            new string('A', 100), // descrição bem grande
            Money.FromDecimal(99.99m),
            1
        );


        var productId = item.ProductId;


        Assert.False(string.IsNullOrWhiteSpace(productId));
        Assert.True(productId!.Length <= 60);
    }
}
