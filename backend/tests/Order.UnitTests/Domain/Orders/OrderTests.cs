using System;
using OrderEntity = Order.Core.Domain.Orders.Order;
using OrderItemEntity = Order.Core.Domain.Orders.OrderItem;
using Order.Core.Domain.Orders.Enums;
using Order.Core.Domain.Orders.ValueObjects;
using Xunit;

namespace Order.UnitTests.Domain.Orders;

public class OrderTests
{

    private static OrderEntity CreateOrder(string clientName = "Test Customer")
        => OrderEntity.Create(clientName);

    private static OrderItemEntity CreateItem(
        OrderEntity order,
        string description = "Iphone 30",
        decimal unitPrice = 10m,
        int quantity = 3)
    {
        var item = OrderItemEntity.Create(
            order.Id,
            description,
            Money.FromDecimal(unitPrice),
            quantity);

        order.AddItem(item);
        return item;
    }

    
    private static OrderEntity CreateOrderWithOneItem(
        string clientName = "Tulio Franco",
        string description = "Iphone 30",
        decimal unitPrice = 10m,
        int quantity = 3)
    {
        var order = CreateOrder(clientName);
        CreateItem(order, description, unitPrice, quantity);
        return order;
    }
    
    [Fact]
    public void Create_ShouldInitializeOrderWithDefaultValues()
    {
        var before = DateTime.UtcNow;
        var order = CreateOrder();
        var after = DateTime.UtcNow;

        Assert.NotEqual(Guid.Empty, order.Id);
        Assert.False(string.IsNullOrWhiteSpace(order.OrderNumber.Value));
        Assert.Equal("Test Customer", order.ClientName);

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
        var order = CreateOrder();

        var beforeUpdate = order.UpdatedAt;

        CreateItem(order: order, unitPrice: 10m, quantity: 2);

        Assert.Single(order.Items);
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
        var order = CreateOrder();
        var item = CreateItem(order, description: "Fusca", unitPrice: 10m, quantity: 2);

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
        var order = CreateOrder();
        var item = CreateItem(
            order,
            description: "Slackline",
            unitPrice: 10m,
            quantity: 2);

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
        var order = CreateOrder();
        var item = CreateItem(
            order,
            description: "Prancha de surf",
            unitPrice: 10m,
            quantity: 2);

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
        var order = CreateOrder();
        var item = CreateItem(
            order,
            description: "Paraquedas",
            unitPrice: 10m,
            quantity: 2);

        Assert.Throws<InvalidOperationException>(() =>
            order.UpdateItemQuantity(item, -3)); // 2 -> -1
    }

    [Fact]
    public void Close_WhenOrderIsOpen_ShouldSetStatusClosedAndSetClosedAtAndUpdatedAt()
    {
        var order = CreateOrderWithOneItem();
        order.Close();

        Assert.Equal(OrderStatus.Closed, order.Status);
        Assert.NotNull(order.ClosedAt);
        Assert.NotNull(order.UpdatedAt);
        Assert.Equal(order.ClosedAt, order.UpdatedAt);
    }

    [Fact]
    public void Close_WhenOrderIsNotOpen_ShouldThrow()
    {
        var order = CreateOrderWithOneItem();
        order.Close(); // first close

        Assert.Throws<InvalidOperationException>(() => order.Close()); // second close
    }

    [Fact]
    public void Close_WhenOrderHasNoItems_ShouldThrowInvalidOperationException()
    {
        var order = CreateOrder();

        var ex = Assert.Throws<InvalidOperationException>(() => order.Close());

        Assert.Contains("não pode ser fechado pois não possui itens", ex.Message);
    }
}
