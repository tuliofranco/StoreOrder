using MediatR;
using Microsoft.AspNetCore.Mvc;
using Order.Core.Application.UseCases.Orders.CreateOrder;
using Order.Core.Application.UseCases.Orders.GetOrderByOrderNumber;
using Order.Api.ViewModels;
using Order.Api.Extensions;
using Order.Core.Application.UseCases.Orders.GetAllOrders;
using Order.Core.Application.UseCases.OrderItem.AddItem;
using Order.Api.ViewModels.OrderItem;


namespace Order.Api.Controllers;

[ApiController]
[Route("api/v1/orders")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder(CancellationToken ct)
    {
        try
        {
            var result = await _mediator.Send(new CreateOrderCommand(), ct);
            return Ok(new ResultViewModel<CreateOrderResponse>(result));
        }
        catch
        {
            return StatusCode(500, new ResultViewModel<CreateOrderResponse>("001X001 - Falha interna no servidor"));
        }
    }


    [HttpPost("{orderNumber}/addItem")]
    public async Task<IActionResult> AddItemToOrder(
        string orderNumber,
        [FromBody] AddOrderItemRequest request,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ResultViewModel<GetOrderByOrderNumberResponse>(ModelState.GetErrors()));

        try
        {
            var command = new AddOrderItemCommand(
                OrderNumber: orderNumber,
                Description: request.Description,
                UnitPrice: request.UnitPrice,
                Quantity: request.Quantity
            );

            var result = await _mediator.Send(command, ct);

            return Ok(new ResultViewModel<AddOrderItemResponse>(result));
        }
        catch
        {
            return NotFound(new ResultViewModel<AddOrderItemResponse>("Order not found"));
        }
    }

    [HttpGet("{orderNumber}")]
    public async Task<IActionResult> GetOrder(string orderNumber, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ResultViewModel<GetOrderByOrderNumberResponse>(ModelState.GetErrors()));
        try
        {
            var result = await _mediator.Send(new GetOrderByOrderNumberQuery(orderNumber), ct);
            return Ok(new ResultViewModel<GetOrderByOrderNumberResponse>(result));
        }
        catch
        {
            return NotFound(new ResultViewModel<string>("Order not found"));
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAllOrders(CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ResultViewModel<IEnumerable<GetAllOrdersResponse>>(ModelState.GetErrors()));

        var result = await _mediator.Send(new GetAllOrdersQuery(), ct);

        return Ok(new ResultViewModel<IEnumerable<GetAllOrdersResponse>>(result));
    }
}
