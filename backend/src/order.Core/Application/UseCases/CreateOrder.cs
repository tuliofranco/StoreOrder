using Order.Core.Application.Abstractions.Repositories;
using OrderEntity = Order.Core.Domain.Orders.Order;

namespace Order.Core.Application.UseCases;

public class CreateOrder
{
    private readonly IOrderRepository _repository;

    public CreateOrder(IOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<OrderEntity> ExecuteAsync( CancellationToken ct = default)
    {
        OrderEntity order = OrderEntity.Create();
        await _repository.AddAsync(order, ct);

        return order;
    }
}