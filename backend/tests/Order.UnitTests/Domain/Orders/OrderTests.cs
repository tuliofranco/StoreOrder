using System;
using OrderEntity = Order.Core.Domain.Orders.Order;
using OrderItemEntity = Order.Core.Domain.Orders.OrderItem;
using Order.Core.Domain.Orders.Enums;
using Order.Core.Domain.Orders.ValueObjects;
using Xunit;

namespace Order.UnitTests.Domain.Orders;

public class OrderTests
{
    [Fact]
    public void Create_ShouldInitializeOrderWithDefaultValues()
    {
        var before = DateTime.UtcNow;
        var order = OrderEntity.Create();
        var after = DateTime.UtcNow;

        Assert.NotEqual(Guid.Empty, order.Id);
        Assert.False(string.IsNullOrWhiteSpace(order.OrderNumber.Value));

        Assert.Equal(OrderStatus.Open, order.Status);

        Assert.InRange(order.CreatedAt, before, after);
        Assert.Null(order.UpdatedAt);
        Assert.Null(order.ClosedAt);
        Assert.Null(order.DeletedAt);

        Assert.Empty(order.Items);
        Assert.Equal(0m, order.Total.Amount);
    }

    [Fact]
    public void AddItem_ShouldAddItemToList_AndIncreaseTotal_AndUpdateUpdatedAt()
    {
        var order = OrderEntity.Create();
        var item = OrderItemEntity.Create(
            order.Id,
            "Iphone 30",
            Money.FromDecimal(10m),
            2
        );

        var beforeUpdate = order.UpdatedAt;
        order.AddItem(item);

        Assert.Single(order.Items);
        Assert.Contains(item, order.Items);
        Assert.Equal(20m, order.Total.Amount);
        Assert.NotNull(order.UpdatedAt);
        if (beforeUpdate.HasValue)
        {
            Assert.True(order.UpdatedAt > beforeUpdate);
        }
        else
        {
            Assert.True(order.UpdatedAt >= order.CreatedAt);
        }
    }

    [Fact]
    public void RemoveItem_ShouldRemoveItemFromList_AndDecreaseTotal_AndUpdateUpdatedAt()
    {
        var order = OrderEntity.Create();
        var item = OrderItemEntity.Create(
            order.Id,
            "Fusca",
            Money.FromDecimal(10m),
            2
        );

        order.AddItem(item);
        var totalBeforeRemove = order.Total.Amount;
        var beforeUpdate = order.UpdatedAt;
        order.RemoveItem(item);

        Assert.Empty(order.Items);
        Assert.Equal(totalBeforeRemove - item.Subtotal.Amount, order.Total.Amount);

        Assert.NotNull(order.UpdatedAt);
        Assert.True(order.UpdatedAt > beforeUpdate);
    }

    [Fact]
    public void UpdateItemQuantity_WithPositiveDelta_ShouldIncreaseQuantityAndTotal()
    {
        var order = OrderEntity.Create();
        var item = OrderItemEntity.Create(
            order.Id,
            "Slackline",
            Money.FromDecimal(10m),
            2
        );

        order.AddItem(item);
        var totalBefore = order.Total.Amount;

        var beforeUpdate = order.UpdatedAt;
        order.UpdateItemQuantity(item, 1);  // 2 -> 3

        Assert.Equal(3, item.Quantity);
        Assert.Equal(totalBefore + 10m, order.Total.Amount);

        Assert.NotNull(order.UpdatedAt);
        Assert.True(order.UpdatedAt >= beforeUpdate);
    }

    [Fact]
    public void UpdateItemQuantity_WhenQuantityBecomesZero_ShouldRemoveItemAndDecreaseTotal()
    {
        var order = OrderEntity.Create();
        var item = OrderItemEntity.Create(
            order.Id,
            "Prancha de surf",
            Money.FromDecimal(10m),
            2
        );

        order.AddItem(item);
        var totalBefore = order.Total.Amount;


        var beforeUpdate = order.UpdatedAt;
        order.UpdateItemQuantity(item, -2); // 2 -> 0 => remove


        Assert.Equal(0, item.Quantity);
        Assert.Empty(order.Items);
        Assert.Equal(totalBefore - 20m, order.Total.Amount);

        Assert.NotNull(order.UpdatedAt);
        Assert.True(order.UpdatedAt >= beforeUpdate);
    }

    [Fact]
    public void UpdateItemQuantity_WithDeltaThatMakesQuantityNegative_ShouldThrow()
    {
        var order = OrderEntity.Create();
        var item = OrderItemEntity.Create(
            order.Id,
            "Paraquedas",
            Money.FromDecimal(10m),
            2
        );

        order.AddItem(item);

        Assert.Throws<InvalidOperationException>(() =>
            order.UpdateItemQuantity(item, -3)); // 2 -> -1
    }

    [Fact]
    public void Close_WhenOrderIsOpen_ShouldSetStatusClosedAndSetClosedAtAndUpdatedAt()
    {

        var order = OrderEntity.Create();
        order.Close();

        Assert.Equal(OrderStatus.Closed, order.Status);
        Assert.NotNull(order.ClosedAt);
        Assert.NotNull(order.UpdatedAt);
        Assert.Equal(order.ClosedAt, order.UpdatedAt);
    }

    [Fact]
    public void Close_WhenOrderIsNotOpen_ShouldThrow()
    {
        var order = OrderEntity.Create();
        order.Close();

        Assert.Throws<InvalidOperationException>(() => order.Close()); // segunda vez deve falhar
    }
}
