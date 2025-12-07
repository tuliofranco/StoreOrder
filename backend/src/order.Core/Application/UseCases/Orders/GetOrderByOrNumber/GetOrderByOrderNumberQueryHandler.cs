using MediatR;
using Order.Core.Application.Abstractions.Repositories;

namespace Order.Core.Application.UseCases.Orders.GetOrderByOrderNumber;

public class GetOrderByOrderNumberQueryHandler : IRequestHandler<GetOrderByOrderNumberQuery, GetOrderByOrderNumberResponse>
{
    private readonly IOrderRepository _repository;

    public GetOrderByOrderNumberQueryHandler(IOrderRepository repository)
    {
        _repository = repository;
    }


    public async Task<GetOrderByOrderNumberResponse> Handle(
        GetOrderByOrderNumberQuery request, 
        CancellationToken ct)
    {
        var order = await _repository.GetByOrderNumberAsync(request.OrderNumber, ct);

        if (order is null)
            throw new Exception("Order not found");

        var items = order.Items
            .Select(i => new GetOrderByOrderNumberItemResponse(
                Id: i.Id,
                Description: i.Description,
                Quantity: i.Quantity,
                UnitPrice: i.UnitPrice.Amount,
                Subtotal: i.Subtotal.Amount
            ))
            .ToList();

        return new GetOrderByOrderNumberResponse(
            OrderNumber: order.OrderNumber.Value,
            Status: order.Status.ToString(),
            CreatedAt: order.CreatedAt,
            UpdatedAt: order.UpdatedAt,
            ClosedAt: order.ClosedAt,
            Total: order.Total.Amount,
            Items: items
        );
    }
}
