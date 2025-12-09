using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Order.Core.Application.Abstractions;
using Order.Core.Application.Abstractions.Repositories;
using Order.Core.Application.UseCases.OrderItem.RemoveItem;
using Order.Core.Domain.Orders.Enums;
using Order.Core.Domain.Orders.ValueObjects;
using OrderEntity = Order.Core.Domain.Orders.Order;
using OrderItemEntity = Order.Core.Domain.Orders.OrderItem;
using Xunit;
using Order.Core.Application.Common.Exceptions;
using Microsoft.Extensions.Logging;

namespace Order.UnitTests.Application.UseCases.OrderItem;

public class RemoveOrderItemCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<RemoveOrderItemCommandHandler>> _loggerMock;
    private readonly RemoveOrderItemCommandHandler _handler;

    public RemoveOrderItemCommandHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<RemoveOrderItemCommandHandler>>();

        _handler = new RemoveOrderItemCommandHandler(
            _orderRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task Handle_WhenOrderExistsAndItemExists_ShouldRemoveItemAndCommit()
    {
        var orderNumber = "20251208001-00001";
        var order = OrderEntity.Create("Tulio Franco");

        var unitPrice = Money.FromDecimal(10m);
        var item = OrderItemEntity.Create(order.Id, "Product A", unitPrice, 2);
        order.AddItem(item);

        var command = new RemoveOrderItemCommand(
            OrderNumber: orderNumber,
            ProductId: item.ProductId!
        );

        _orderRepositoryMock
            .Setup(r => r.GetByOrderNumberAsync(orderNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _unitOfWorkMock
            .Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()));

        var response = await _handler.Handle(command, CancellationToken.None);

        _orderRepositoryMock.Verify(
            r => r.GetByOrderNumberAsync(orderNumber, It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            u => u.CommitAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        // Item totalmente removido -> Não existe quantidade de produto igual a 0;
        Assert.DoesNotContain(order.Items, i => i.ProductId == item.ProductId);

        Assert.Equal(order.OrderNumber.Value, response.OrderNumber);
        Assert.Equal(order.Status.ToString(), response.Status);
        Assert.Equal(order.CreatedAt, response.CreatedAt);
        Assert.Equal(order.UpdatedAt, response.UpdatedAt);
        Assert.Equal(order.Total.Amount, response.Total);
        Assert.Equal(order.Items.Count, response.Items.Count);
    }

    [Fact]
    public async Task Handle_WhenOrderDoesNotExist_ShouldThrowKeyNotFoundException()
    {
        var orderNumber = "NON-EXISTENT";
        var command = new RemoveOrderItemCommand(
            OrderNumber: orderNumber,
            ProductId: "ANY_PRODUCT_ID"
        );

        _orderRepositoryMock
            .Setup(r => r.GetByOrderNumberAsync(orderNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrderEntity?)null);

        var ex = await Assert.ThrowsAsync<OrderNotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));

        Assert.Contains(orderNumber, ex.Message);

        _unitOfWorkMock.Verify(
            u => u.CommitAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenOrderIsClosed_ShouldThrowInvalidOperationException()
    {
        var orderNumber = "20251208002-00001";
        var order = OrderEntity.Create("Tulio Franco");

        var unitPrice = Money.FromDecimal(10m);
        var item = OrderItemEntity.Create(order.Id, "Product B", unitPrice, 1);
        order.AddItem(item);

        order.Close();
        Assert.Equal(OrderStatus.Closed, order.Status);

        var command = new RemoveOrderItemCommand(
            OrderNumber: orderNumber,
            ProductId: item.ProductId!
        );

        _orderRepositoryMock
            .Setup(r => r.GetByOrderNumberAsync(orderNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        Assert.Contains("Cannot modify a closed order", ex.Message);

        _unitOfWorkMock.Verify(
            u => u.CommitAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenItemDoesNotExistInOrder_ShouldThrowKeyNotFoundException()
    {
        var orderNumber = "20251208003-00001";
        var order = OrderEntity.Create("Tulio Franco");

        var command = new RemoveOrderItemCommand(
            OrderNumber: orderNumber,
            ProductId: "UNKNOWN_PRODUCT_ID"
        );

        _orderRepositoryMock
            .Setup(r => r.GetByOrderNumberAsync(orderNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var ex = await Assert.ThrowsAsync<OrderNotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));

        Assert.Contains("UNKNOWN_PRODUCT_ID", ex.Message);
        Assert.Contains(orderNumber, ex.Message);

        _unitOfWorkMock.Verify(
            u => u.CommitAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
