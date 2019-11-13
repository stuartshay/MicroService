using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MicroService.WebApi.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public class RandomHealthCheck : IHealthCheck
    {
        /// <inheritdoc/>
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            if (DateTime.UtcNow.Minute % 2 == 0)
            {
                return Task.FromResult(HealthCheckResult.Healthy());
            }

            return Task.FromResult(HealthCheckResult.Unhealthy(description: "failed"));
        }
    }
}
