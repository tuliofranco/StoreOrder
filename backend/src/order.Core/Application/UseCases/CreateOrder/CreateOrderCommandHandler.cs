using MediatR;
using Order.Core.Application.Abstractions.Repositories;
using OrderEntity = Order.Core.Domain.Orders.Order;

namespace Order.Core.Application.UseCases.CreateOrder;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, CreateOrderResponse>
{
    private readonly IOrderRepository _repository;

    public CreateOrderCommandHandler(IOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<CreateOrderResponse> Handle(CreateOrderCommand request, CancellationToken ct = default)
    {
        OrderEntity order = OrderEntity.Create();
        await _repository.AddAsync(order, ct);

        return new CreateOrderResponse(
            order.OrderNumber.Value,
            order.Status.ToString(),
            order.CreatedAt,
            order.UpdatedAt,
            order.ClosedAt,
            order.Total.Amount
        );
    }
}