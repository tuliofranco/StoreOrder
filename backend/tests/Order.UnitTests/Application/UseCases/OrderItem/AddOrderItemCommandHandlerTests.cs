using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Order.Core.Application.Abstractions;
using Order.Core.Application.Abstractions.Repositories;
using Order.Core.Application.UseCases.OrderItem.AddItem;
using Order.Core.Domain.Orders.Enums;
using Order.Core.Domain.Orders.ValueObjects;
using OrderEntity = Order.Core.Domain.Orders.Order;
using OrderItemEntity = Order.Core.Domain.Orders.OrderItem;
using Xunit;
using Order.Core.Application.Common.Exceptions;
using Microsoft.Extensions.Logging;

namespace Order.UnitTests.Application.UseCases.OrderItem;

public class AddOrderItemCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IOrderItemRepository> _orderItemRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<AddOrderItemCommandHandler>> _loggerMock;
    private readonly AddOrderItemCommandHandler _handler;

    public AddOrderItemCommandHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _orderItemRepositoryMock = new Mock<IOrderItemRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<AddOrderItemCommandHandler>>();

        _handler = new AddOrderItemCommandHandler(
            _orderRepositoryMock.Object,
            _orderItemRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object
        );
    }

    private static OrderEntity CreateClosedOrderWithItems()
    {
        var order = OrderEntity.Create("Tulio Franco");

        var unitPrice = Money.FromDecimal(10m);
        var item = OrderItemEntity.Create(order.Id, "Product A", unitPrice, 1);

        order.AddItem(item);
        order.Close();

        return order;
    }

    [Fact]
    public async Task Handle_WhenOrderExistsAndItemIsNew_ShouldAddItemPersistAndReturnMappedResponse()
    {
        var orderNumber = "20251208001-00001";
        var command = new AddOrderItemCommand(
            OrderNumber: orderNumber,
            Description: "Product A",
            UnitPrice: 10m,
            Quantity: 2
        );

        var order = OrderEntity.Create("Tulio Franco");

        _orderRepositoryMock
            .Setup(r => r.GetByOrderNumberAsync(orderNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _orderItemRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<OrderItemEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()));

        var response = await _handler.Handle(command, CancellationToken.None);

        _orderRepositoryMock.Verify(
            r => r.GetByOrderNumberAsync(orderNumber, It.IsAny<CancellationToken>()),
            Times.Once);

        _orderItemRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<OrderItemEntity>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            u => u.CommitAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        Assert.Single(order.Items);
        var addedItem = order.Items.Single();
        Assert.Equal(command.Description, addedItem.Description);
        Assert.Equal(command.Quantity, addedItem.Quantity);
        Assert.Equal(command.UnitPrice, addedItem.UnitPrice.Amount);

        Assert.Equal(order.OrderNumber.Value, response.OrderNumber);
        Assert.Equal(order.Status.ToString(), response.Status);
        Assert.Equal(order.CreatedAt, response.CreatedAt);
        Assert.Equal(order.UpdatedAt, response.UpdatedAt);
        Assert.Equal(order.Total.Amount, response.Total);

        var responseItem = Assert.Single(response.Items);
        Assert.Equal(addedItem.ProductId, responseItem.ProductId);
        Assert.Equal(addedItem.Description, responseItem.Description);
        Assert.Equal(addedItem.Quantity, responseItem.Quantity);
        Assert.Equal(addedItem.UnitPrice.Amount, responseItem.UnitPrice);
        Assert.Equal(addedItem.Subtotal.Amount, responseItem.Subtotal);
    }

    [Fact]
    public async Task Handle_WhenItemAlreadyExists_ShouldUpdateQuantityWithoutAddingNewItem()
    {
        var orderNumber = "20251208002-00001";
        var unitPrice = Money.FromDecimal(15m);
        var order = OrderEntity.Create("Tulio Franco");

        var existingItem = OrderItemEntity.Create(order.Id, "Product B", unitPrice, 1);
        order.AddItem(existingItem);

        var command = new AddOrderItemCommand(
            OrderNumber: orderNumber,
            Description: existingItem.Description,
            UnitPrice: existingItem.UnitPrice.Amount,
            Quantity: 3
        );

        _orderRepositoryMock
            .Setup(r => r.GetByOrderNumberAsync(orderNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _unitOfWorkMock
            .Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()));

        var previousQuantity = existingItem.Quantity;

        var response = await _handler.Handle(command, CancellationToken.None);

        _orderItemRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<OrderItemEntity>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            u => u.CommitAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        Assert.Single(order.Items);
        var updatedItem = order.Items.Single();
        Assert.Equal(existingItem.Description, updatedItem.Description);
        Assert.Equal(existingItem.UnitPrice.Amount, updatedItem.UnitPrice.Amount);

        Assert.NotEqual(previousQuantity, updatedItem.Quantity);

        Assert.Equal(order.OrderNumber.Value, response.OrderNumber);
        Assert.Equal(order.Status.ToString(), response.Status);
        Assert.Equal(order.CreatedAt, response.CreatedAt);
        Assert.Equal(order.UpdatedAt, response.UpdatedAt);
        Assert.Equal(order.Total.Amount, response.Total);

        Assert.Single(response.Items);
    }

    [Fact]
    public async Task Handle_WhenOrderDoesNotExist_ShouldThrowKeyNotFoundException()
    {
        var orderNumber = "NON-EXISTENT";
        var command = new AddOrderItemCommand(
            OrderNumber: orderNumber,
            Description: "Anything",
            UnitPrice: 10m,
            Quantity: 1
        );

        _orderRepositoryMock
            .Setup(r => r.GetByOrderNumberAsync(orderNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrderEntity?)null);

        var ex = await Assert.ThrowsAsync<OrderNotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));

        Assert.Contains(orderNumber, ex.Message);

        _orderItemRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<OrderItemEntity>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            u => u.CommitAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenOrderIsClosed_ShouldThrowInvalidOperationException()
    {
        var order = CreateClosedOrderWithItems();
        var orderNumber = order.OrderNumber.Value;

        var command = new AddOrderItemCommand(
            OrderNumber: orderNumber,
            Description: "Product X",
            UnitPrice: 50m,
            Quantity: 1
        );

        _orderRepositoryMock
            .Setup(r => r.GetByOrderNumberAsync(orderNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        Assert.Contains("Cannot modify a closed order", ex.Message);
        Assert.Contains(orderNumber, ex.Message);

        _orderItemRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<OrderItemEntity>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            u => u.CommitAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
