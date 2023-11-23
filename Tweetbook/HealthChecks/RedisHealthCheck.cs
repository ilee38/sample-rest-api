using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Tweetbook.HealthChecks
{
    public class RedisHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
