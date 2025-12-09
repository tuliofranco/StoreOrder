using Microsoft.AspNetCore.Mvc;
using Order.Api.Services;


namespace Order.Api.Internal;
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
         try
        {
            var result = await _sql.ExecuteQueryAsync(request.Query, HttpContext.RequestAborted);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Erro ao executar SQL.", details = ex.Message });
        }
    }
}

public record SqlRequest(string Query);
