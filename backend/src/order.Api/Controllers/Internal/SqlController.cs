using Microsoft.AspNetCore.Mvc;
using Order.Api.Services;


namespace Order.Api.Controllers.Internal;
[ApiController]
[Route("internal/sql")]
public class SqlController : ControllerBase
{
    private readonly ISqlExecutionService _sql;

    public SqlController(ISqlExecutionService sql)
    {
        _sql = sql;
    }

    [HttpPost]
    public async Task<IActionResult> Execute([FromBody] SqlRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var result = await _sql.ExecuteQueryAsync(request.Query, HttpContext.RequestAborted);
        return Ok(result);
    }
}

