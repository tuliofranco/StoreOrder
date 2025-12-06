using MediatR;
using Microsoft.AspNetCore.Mvc;
using Order.Core.Application.UseCases.CreateOrder;
using Order.Core.Application.UseCases.GetOrderByOrderNumber;
using Order.Api.ViewModels;
using Order.Api.Extensions;
using Order.Core.Application.UseCases.GetAllOrders;

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

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrder(string id, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ResultViewModel<GetOrderByOrderNumberResponse>(ModelState.GetErrors()));
        try
        {
            var result = await _mediator.Send(new GetOrderByOrderNumberQuery(id), ct);
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
