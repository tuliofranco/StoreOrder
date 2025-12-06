using MediatR;
using Microsoft.AspNetCore.Mvc;
using Order.Core.Application.UseCases.CreateOrder;
using Order.Api.ViewModels;

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
}
