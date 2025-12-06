using MediatR;
using Order.Core.Application.Abstractions.Repositories;

namespace Order.Core.Application.UseCases.GetOrderByOrNumber;

public class GetOrderQueryHandler : IRequestHandler<GetOrderQuery, GetOrderResponse>
{
    private readonly IOrderRepository _repository;

    public GetOrderQueryHandler(IOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<GetOrderResponse> Handle(GetOrderQuery request, CancellationToken ct)
    {
        var order = await _repository.GetByIdAsync(request.OrderNumber, ct);

        if (order is null)
            throw new Exception("Order not found");

        return new GetOrderResponse(
        order.OrderNumber.Value,
        order.Status.ToString(),
        order.CreatedAt,
        order.UpdatedAt,
        order.ClosedAt,
        order.Total.Amount
    );
    }
}
