using MediatR;
using Order.Core.Application.Abstractions.Repositories;

namespace Order.Core.Application.UseCases.GetAllOrders;

public class GetAllOrdersQueryHandler 
    : IRequestHandler<GetAllOrdersQuery, IEnumerable<GetAllOrdersResponse>>
{
    private readonly IOrderRepository _repository;

    public GetAllOrdersQueryHandler(IOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<GetAllOrdersResponse>> Handle(
        GetAllOrdersQuery request,
        CancellationToken ct)
    {
        var orders = await _repository.ListOrdersAsync(ct);

        return orders.Select(order => new GetAllOrdersResponse(
            order!.OrderNumber.Value,
            order.Status.ToString(),
            order.CreatedAt,
            order.UpdatedAt,
            order.ClosedAt,
            order.Total.Amount
        ));
    }
}
