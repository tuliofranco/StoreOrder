using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Order.Core.Application.Abstractions.Repositories;
using Order.Core.Application.UseCases.Orders.GetOrderByOrderNumber;
using Order.Core.Domain.Orders.ValueObjects;
using OrderEntity = Order.Core.Domain.Orders.Order;
using OrderItemEntity = Order.Core.Domain.Orders.OrderItem;
using Xunit;
using Order.Core.Application.Abstractions;
using Order.Core.Application.Common.Exceptions;

namespace Order.UnitTests.Application.UseCases.Orders;

public class GetOrderByOrderNumberQueryHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<ICacheService> _orderCacheMock;
    private readonly GetOrderByOrderNumberQueryHandler _handler;

    public GetOrderByOrderNumberQueryHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _orderCacheMock = new Mock<ICacheService>();
        _handler = new GetOrderByOrderNumberQueryHandler(_orderRepositoryMock.Object, _orderCacheMock!.Object);
    }

    [Fact]
    public async Task Handle_WhenOrderExists_ShouldMapOrderAndItemsToResponse()
    {
        var query = new GetOrderByOrderNumberQuery("20251208123-00001");

        // cache miss
        _orderCacheMock
            .Setup(c => c.GetAsync<GetOrderByOrderNumberResponse>(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetOrderByOrderNumberResponse?)null);

        _orderCacheMock
            .Setup(c => c.SetAsync(
                It.IsAny<string>(),
                It.IsAny<GetOrderByOrderNumberResponse>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var order = OrderEntity.Create("Tulio Franco");

        var unitPrice = Money.FromDecimal(10m);
        var item = OrderItemEntity.Create(order.Id, "Product A", unitPrice, 2);
        order.AddItem(item); // assume que o domínio atualiza Total e Items

        _orderRepositoryMock
            .Setup(r => r.GetByOrderNumberAsync(query.OrderNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var response = await _handler.Handle(query, CancellationToken.None);

        _orderRepositoryMock.Verify(
            r => r.GetByOrderNumberAsync(query.OrderNumber, It.IsAny<CancellationToken>()),
            Times.Once);

        // mapeamento da ordem
        Assert.Equal(order.OrderNumber.Value, response.OrderNumber);
        Assert.Equal(order.Status.ToString(), response.Status);
        Assert.Equal(order.CreatedAt, response.CreatedAt);
        Assert.Equal(order.UpdatedAt, response.UpdatedAt);
        Assert.Equal(order.ClosedAt, response.ClosedAt);
        Assert.Equal(order.Total.Amount, response.Total);

        // mapeamento dos itens
        var responseItem = Assert.Single(response.Items);
        Assert.Equal(item.ProductId, responseItem.ProductId);
        Assert.Equal(item.Description, responseItem.Description);
        Assert.Equal(item.Quantity, responseItem.Quantity);
        Assert.Equal(item.UnitPrice.Amount, responseItem.UnitPrice);
        Assert.Equal(item.Subtotal.Amount, responseItem.Subtotal);
    }

    [Fact]
    public async Task Handle_WhenOrderDoesNotExist_ShouldThrowException()
    {
        var query = new GetOrderByOrderNumberQuery("NON-EXISTENT");

        _orderCacheMock
            .Setup(c => c.GetAsync<GetOrderByOrderNumberResponse>(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetOrderByOrderNumberResponse?)null);

        _orderRepositoryMock
            .Setup(r => r.GetByOrderNumberAsync(query.OrderNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrderEntity?)null);

        var ex = await Assert.ThrowsAsync<OrderNotFoundException>(
            () => _handler.Handle(query, CancellationToken.None));

        Assert.Equal("Order not found", ex.Message);

        _orderRepositoryMock.Verify(
            r => r.GetByOrderNumberAsync(query.OrderNumber, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
