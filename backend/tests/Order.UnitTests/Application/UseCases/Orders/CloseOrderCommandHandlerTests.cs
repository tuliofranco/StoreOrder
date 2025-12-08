using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Order.Core.Application.Abstractions;
using Order.Core.Application.Abstractions.Repositories;
using Order.Core.Application.UseCases.Orders.CloseOrder;
using Order.Core.Domain.Orders.Enums;
using Order.Core.Domain.Orders.ValueObjects;
using OrderEntity = Order.Core.Domain.Orders.Order;
using OrderItemEntity = Order.Core.Domain.Orders.OrderItem;
using Xunit;

namespace Order.UnitTests.Application.UseCases.Orders;

public class CloseOrderCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CloseOrderCommandHandler _handler;

    public CloseOrderCommandHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new CloseOrderCommandHandler(
            _orderRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_WhenOrderIsOpenAndHasItems_ShouldCloseOrderAndCommit()
    {
        var orderNumber = "20251208001-00001";
        var command = new CloseOrderCommand(orderNumber);

        var order = OrderEntity.Create();

        var unitPrice = Money.FromDecimal(10m);
        var item = OrderItemEntity.Create(order.Id, "Product A", unitPrice, 2);
        order.AddItem(item);

        _orderRepositoryMock
            .Setup(r => r.GetByOrderNumberAsync(orderNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _unitOfWorkMock
            .Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var response = await _handler.Handle(command, CancellationToken.None);

        _unitOfWorkMock.Verify(
            u => u.CommitAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        Assert.Equal(OrderStatus.Closed, order.Status);
        Assert.NotNull(order.ClosedAt);

        Assert.Equal(order.OrderNumber.Value, response.OrderNumber);
        Assert.Equal(order.Status.ToString(), response.Status);
        Assert.Equal(order.CreatedAt, response.CreatedAt);
        Assert.Equal(order.UpdatedAt, response.UpdatedAt);
        Assert.Equal(order.ClosedAt, response.ClosedAt);
        Assert.Equal(order.Total.Amount, response.Total);
    }

    [Fact]
    public async Task Handle_WhenOrderDoesNotExist_ShouldThrowKeyNotFoundException()
    {

        var orderNumber = "NON-EXISTENT";
        var command = new CloseOrderCommand(orderNumber);

        _orderRepositoryMock
            .Setup(r => r.GetByOrderNumberAsync(orderNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrderEntity?)null);

        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));

        Assert.Contains(orderNumber, ex.Message);

        _orderRepositoryMock.Verify(
            r => r.GetByOrderNumberAsync(orderNumber, It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            u => u.CommitAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenOrderStatusIsNotOpen_ShouldThrowInvalidOperationException()
    {

        var orderNumber = "20251208002-00001";
        var command = new CloseOrderCommand(orderNumber);

        var order = OrderEntity.Create();

        var unitPrice = Money.FromDecimal(10m);
        var item = OrderItemEntity.Create(order.Id, "Product B", unitPrice, 1);
        order.AddItem(item);

        order.Close();

        _orderRepositoryMock
            .Setup(r => r.GetByOrderNumberAsync(orderNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        Assert.Contains(orderNumber, ex.Message);
        Assert.Contains(order.Status.ToString(), ex.Message);

        _unitOfWorkMock.Verify(
            u => u.CommitAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenOrderHasNoItems_ShouldThrowInvalidOperationException()
    {
        var orderNumber = "20251208003-00001";
        var command = new CloseOrderCommand(orderNumber);

        var order = OrderEntity.Create(); // Items vazio

        _orderRepositoryMock
            .Setup(r => r.GetByOrderNumberAsync(orderNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        Assert.Contains(orderNumber, ex.Message);
        Assert.Contains("não pode ser fechado", ex.Message, StringComparison.OrdinalIgnoreCase);

        _unitOfWorkMock.Verify(
            u => u.CommitAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
