using System.Threading;
using System.Threading.Tasks;
using Moq;
using Order.Core.Application.Abstractions;
using Order.Core.Application.Abstractions.Repositories;
using Order.Core.Application.UseCases.Orders.CreateOrder;
using OrderEntity = Order.Core.Domain.Orders.Order;
using Xunit;

namespace Order.UnitTests.Application.UseCases.Orders;

public class CreateOrderCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateOrderCommandHandler _handler;

    public CreateOrderCommandHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new CreateOrderCommandHandler(
            _orderRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_WhenCalled_ShouldCreateOrderPersistAndReturnMappedResponse()
    {
        var command = new CreateOrderCommand();
        OrderEntity? capturedOrder = null;

        _orderRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<OrderEntity>(), It.IsAny<CancellationToken>()))
            .Callback<OrderEntity, CancellationToken>((order, _) => capturedOrder = order)
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var response = await _handler.Handle(command, CancellationToken.None);

        _orderRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<OrderEntity>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            u => u.CommitAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        Assert.NotNull(capturedOrder);
        var order = capturedOrder!;

        Assert.Equal(order.OrderNumber.Value, response.OrderNumber);
        Assert.Equal(order.Status.ToString(), response.Status);
        Assert.Equal(order.CreatedAt, response.CreatedAt);
        Assert.Equal(order.UpdatedAt, response.UpdatedAt);
        Assert.Equal(order.ClosedAt, response.ClosedAt);
        Assert.Equal(order.Total.Amount, response.Total);
    }
}
