using System.Data;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Order.Api.Services;
using Order.Infrastructure.Persistence;

namespace Order.Api.Services;

public class SqlExecutionService : ISqlExecutionService
{
    private readonly StoreOrderDbContext _db;
        private readonly ILogger<SqlExecutionService> _logger;

    public SqlExecutionService(StoreOrderDbContext db, ILogger<SqlExecutionService> logger)
    {
        _db = db;
        _logger = logger;
    }

    [SuppressMessage(
        "Security",
        "CA2100:Review SQL queries for security vulnerabilities",
        Justification = "Endpoint interno para consultas ad-hoc; acesso restrito, SQL é intencionalmente dinâmico.")]
    public async Task<object?> ExecuteQueryAsync(string query, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Executing dynamic SQL: {Sql}",
            query);
            
        await using var connection = _db.Database.GetDbConnection();
        await connection.OpenAsync(ct);

        await using var command = connection.CreateCommand();
        command.CommandText = query;
        command.CommandType = CommandType.Text;

        await using var reader = await command.ExecuteReaderAsync(ct);

        var columns = Enumerable.Range(0, reader.FieldCount)
                                .Select(reader.GetName)
                                .ToArray();

        var rows = new List<Dictionary<string, object?>>();

        while (await reader.ReadAsync(ct))
        {
            var row = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

            foreach (var column in columns)
            {
                var value = reader[column];
                row[column] = value == DBNull.Value ? null : value;
            }

            rows.Add(row);
        }

        return new
        {
            columns,
            rows
        };
    }
}
