using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Order.Api.Services;

namespace Order.Api.Controllers.Internal;

[ApiController]
[Route("internal/sql")]
public class SqlController : ControllerBase
{
    private readonly ISqlExecutionService _sql;
    private readonly ILogger<SqlController> _logger;

    public SqlController(ISqlExecutionService sql, ILogger<SqlController> logger)
    {
        _sql = sql;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Execute([FromBody] SqlRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var sqlPreview = request.Query.Length > 120
            ? request.Query[..120] + "..."
            : request.Query;

        _logger.LogInformation(
            "Internal SQL execution requested. Length {Length}. Preview: {Preview}",
            request.Query.Length,
            sqlPreview);

        var result = await _sql.ExecuteQueryAsync(request.Query, HttpContext.RequestAborted);

        _logger.LogInformation(
            "Internal SQL execution completed successfully.");

        return Ok(result);
    }
}