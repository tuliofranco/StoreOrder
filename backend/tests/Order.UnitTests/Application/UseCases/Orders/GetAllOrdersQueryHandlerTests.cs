using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Order.Core.Application.Abstractions.Repositories;
using Order.Core.Application.Common;
using Order.Core.Application.UseCases.Orders.GetAllOrders;
using Order.Core.Domain.Orders.Enums;
using OrderEntity = Order.Core.Domain.Orders.Order;
using Xunit;

namespace Order.UnitTests.Application.UseCases.Orders;

public class GetAllOrdersQueryHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly GetAllOrdersQueryHandler _handler;

    public GetAllOrdersQueryHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _handler = new GetAllOrdersQueryHandler(_orderRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnPagedResultWithMappedItems()
    {
        var statusFilter = OrderStatus.Open;

        var order1 = OrderEntity.Create();
        var order2 = OrderEntity.Create();

        var pagedOrders = new PagedResult<OrderEntity>
        {
            Items = new List<OrderEntity> { order1, order2 },
            Page = 1,
            PageSize = 10,
            TotalItems = 2,
            TotalPages = 1
        };

        _orderRepositoryMock
            .Setup(r => r.CountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        _orderRepositoryMock
            .Setup(r => r.GetPagedAsync(1, 10, statusFilter, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedOrders);

        var query = new GetAllOrdersQuery(page: 1, pageSize: 10, Status: statusFilter);

        var result = await _handler.Handle(query, CancellationToken.None);

        _orderRepositoryMock.Verify(
            r => r.CountAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        _orderRepositoryMock.Verify(
            r => r.GetPagedAsync(1, 10, statusFilter, It.IsAny<CancellationToken>()),
            Times.Once);

        // paginação
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(2, result.TotalItems);
        Assert.Equal(1, result.TotalPages);
        Assert.Equal(2, result.Items.Count());

        // mapeamento do primeiro item
        var firstResponse = result.Items.First();
        Assert.Equal(order1.OrderNumber.Value, firstResponse.OrderNumber);
        Assert.Equal(order1.Status.ToString(), firstResponse.Status);
        Assert.Equal(order1.CreatedAt, firstResponse.CreatedAt);
        Assert.Equal(order1.UpdatedAt, firstResponse.UpdatedAt);
        Assert.Equal(order1.ClosedAt, firstResponse.ClosedAt);
        Assert.Equal(order1.Total.Amount, firstResponse.Total);
    }

    [Fact]
    public async Task Handle_ShouldNormalizeNegativePageAndPageSize()
    {
        var order = OrderEntity.Create();

        var pagedOrders = new PagedResult<OrderEntity>
        {
            Items = new List<OrderEntity> { order },
            Page = 0,
            PageSize = 25,
            TotalItems = 1,
            TotalPages = 1
        };

        _orderRepositoryMock
            .Setup(r => r.CountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _orderRepositoryMock
            .Setup(r => r.GetPagedAsync(0, 25, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedOrders);

        var query = new GetAllOrdersQuery(page: -5, pageSize: -10, Status: null);

        var result = await _handler.Handle(query, CancellationToken.None);

        // normalização de page/pageSize
        _orderRepositoryMock.Verify(
            r => r.GetPagedAsync(0, 25, null, It.IsAny<CancellationToken>()),
            Times.Once);

        Assert.Equal(0, result.Page);
        Assert.Equal(25, result.PageSize);
        Assert.Single(result.Items);
    }
}
