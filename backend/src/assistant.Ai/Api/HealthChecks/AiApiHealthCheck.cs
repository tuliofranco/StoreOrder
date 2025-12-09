using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Assistant.Ai.Api.HealthChecks
{
    public sealed class AiApiHealthCheck : IHealthCheck
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<AiApiHealthCheck> _logger;

        public AiApiHealthCheck(
            IHttpClientFactory httpClientFactory,
            ILogger<AiApiHealthCheck> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("order-api");

                var response = await client.GetAsync("/health", cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "OrderApi health returned StatusCode {StatusCode}",
                        (int)response.StatusCode);

                    return HealthCheckResult.Unhealthy(
                        $"OrderApi returned status {(int)response.StatusCode}");
                }

                _logger.LogInformation("OrderApi health OK from Assistant.Ai.");
                return HealthCheckResult.Healthy("OrderApi reachable from Assistant.Ai");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao checar OrderApi a partir da Assistant.Ai");

                return HealthCheckResult.Unhealthy(
                    description: "OrderApi unreachable from Assistant.Ai",
                    exception: ex);
            }
        }
    }
}
