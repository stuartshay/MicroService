using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MicroService.WebApi.Services
{
    internal class CronJobServiceHealthCheck : IHealthCheck
    {
        public bool StartupTaskCompleted { get; set; }

        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (StartupTaskCompleted)
            {
                return Task.FromResult(
                        HealthCheckResult.Healthy("The startup task of cron job service is finished."));
            }

            return Task.FromResult(
                HealthCheckResult.Unhealthy("The startup task of cron job service is still running."));
        }
    }
}
