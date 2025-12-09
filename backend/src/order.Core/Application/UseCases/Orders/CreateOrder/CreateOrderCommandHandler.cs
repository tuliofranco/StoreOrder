using MediatR;
using Order.Core.Application.Abstractions;
using Order.Core.Application.Abstractions.Repositories;
using OrderEntity = Order.Core.Domain.Orders.Order;

namespace Order.Core.Application.UseCases.Orders.CreateOrder;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, CreateOrderResponse>
{
    private readonly IOrderRepository _repository;
    private readonly IUnitOfWork _uow;

    public CreateOrderCommandHandler(IOrderRepository repository, IUnitOfWork uow)
    {
        _repository = repository;
        _uow = uow;
    }

    public async Task<CreateOrderResponse> Handle(CreateOrderCommand request, CancellationToken ct = default)
    {
        OrderEntity order = OrderEntity.Create(request.clientName);
        await _repository.AddAsync(order, ct);

        await _uow.CommitAsync();

        return new CreateOrderResponse(
            order.OrderNumber.Value,
            order.ClientName,
            order.Status.ToString(),
            order.CreatedAt,
            order.UpdatedAt,
            order.ClosedAt,
            order.Total.Amount
        );
    }
}