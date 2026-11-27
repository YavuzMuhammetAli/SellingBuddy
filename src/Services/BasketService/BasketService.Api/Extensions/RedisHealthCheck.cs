using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace BasketService.Api.Extensions
{
    //redis ping, basket can't work without it
    public class RedisHealthCheck : IHealthCheck
    {
        private readonly ConnectionMultiplexer _redis;

        public RedisHealthCheck(ConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var latency = await _redis.GetDatabase().PingAsync();

                return HealthCheckResult.Healthy($"Redis ping {latency.TotalMilliseconds:0.##} ms");
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, "Redis is not reachable", ex);
            }
        }
    }
}
