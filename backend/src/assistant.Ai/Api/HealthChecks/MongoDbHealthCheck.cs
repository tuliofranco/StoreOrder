using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Assistant.Ai.Api.HealthChecks;
public sealed class MongoDbHealthCheck : IHealthCheck
{
    private readonly IMongoClient _mongoClient;
    private readonly ILogger<MongoDbHealthCheck> _logger;

    public MongoDbHealthCheck(
        IMongoClient mongoClient,
        ILogger<MongoDbHealthCheck> logger)
    {
        _mongoClient = mongoClient;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var database = _mongoClient.GetDatabase("admin");

            var command = new BsonDocument("ping", 1);
            await database.RunCommandAsync<BsonDocument>(command, cancellationToken: cancellationToken);

            _logger.LogInformation("MongoDB OK (ping=1).");
            return HealthCheckResult.Healthy("MongoDB reachable");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao checar MongoDB");
            return HealthCheckResult.Unhealthy(
                description: "MongoDB unreachable",
                exception: ex
            );
        }
    }
}