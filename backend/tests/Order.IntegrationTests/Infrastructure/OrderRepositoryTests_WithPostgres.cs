using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Order.Core.Domain.Orders.Enums;
using Order.Core.Domain.Orders.ValueObjects;
using Order.Infrastructure.Persistence.Repositories;
using Xunit;
using OrderEntity = Order.Core.Domain.Orders.Order;

namespace Order.IntegrationTests.Infrastructure;

[Collection("PostgresCollection")]
public class OrderRepositoryTests_WithPostgres
{
    private readonly PostgresFixture _fixture;

    public OrderRepositoryTests_WithPostgres(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetPagedAsync_ShouldFilterByStatus_UsingRealPostgres()
    {
        // arrange
        await using var context = _fixture.CreateContext();
        var repo = new OrderRepository(context);

        var now = DateTime.UtcNow;

        var closedOrder = CreateOrder(OrderStatus.Closed, now.AddMinutes(-30));
        var openOrder   = CreateOrder(OrderStatus.Open, now.AddMinutes(-10));

        context.Orders.AddRange(closedOrder, openOrder);
        await context.SaveChangesAsync();

        // act
        var result = await repo.GetPagedAsync(
            page: 0,
            pageSize: 10,
            status: OrderStatus.Closed,
            ct: CancellationToken.None);

        // assert
        result.TotalItems.Should().Be(1);
        result.Items.Should().HaveCount(1);
        foreach (var o in result.Items)
        {
            o.Status.Should().Be(OrderStatus.Closed);
        }
    }

    private static OrderEntity CreateOrder(OrderStatus status, DateTime createdAt)
    {
        var order = (OrderEntity)Activator.CreateInstance(
            typeof(OrderEntity),
            nonPublic: true
        )!;

        typeof(OrderEntity).GetProperty(nameof(OrderEntity.Id))!
            .SetValue(order, Guid.NewGuid());

        typeof(OrderEntity).GetProperty(nameof(OrderEntity.OrderNumber))!
            .SetValue(order, OrderNumber.Create(createdAt));

        typeof(OrderEntity).GetProperty(nameof(OrderEntity.Status))!
            .SetValue(order, status);

        typeof(OrderEntity).GetProperty(nameof(OrderEntity.CreatedAt))!
            .SetValue(order, createdAt);

        typeof(OrderEntity).GetProperty(nameof(OrderEntity.Total))!
            .SetValue(order, Money.FromDecimal(10m));

        return order;
    }
}
