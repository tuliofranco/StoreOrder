using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Order.Infrastructure.HealthChecks
{
    public sealed class RedisDbHealthCheck : IHealthCheck
    {
        private readonly ILogger<RedisDbHealthCheck> _logger;
        private readonly IConnectionMultiplexer _redis;

        public RedisDbHealthCheck(
            IConnectionMultiplexer redis,
            ILogger<RedisDbHealthCheck> logger)
        {
            _redis = redis;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var db = _redis.GetDatabase();

                var latency = await db.PingAsync();

                _logger.LogInformation(
                    "Redis OK. Ping: {Latency} ms",
                    latency.TotalMilliseconds);

                return HealthCheckResult.Healthy("Redis reachable");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao checar Redis");

                return HealthCheckResult.Unhealthy(
                    description: "Redis unreachable",
                    exception: ex
                );
            }
        }
    }
}