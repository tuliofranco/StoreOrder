using System.Threading;

namespace Order.Api.Services;

public interface ISqlExecutionService
{
    Task<object?> ExecuteQueryAsync(string query, CancellationToken ct = default);
}
