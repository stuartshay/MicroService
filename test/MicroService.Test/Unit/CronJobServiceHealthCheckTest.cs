using MicroService.WebApi.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Xunit;

namespace MicroService.Test.Unit
{
    public class CronJobServiceHealthCheckTest
    {
        [Fact]
        public async Task CheckHealthAsync_ReturnsHealthy_WhenStartupTaskCompleted()
        {
            var healthCheck = new CronJobServiceHealthCheck { StartupTaskCompleted = true };

            var result = await healthCheck.CheckHealthAsync(new HealthCheckContext(), TestContext.Current.CancellationToken);

            Assert.Equal(HealthStatus.Healthy, result.Status);
        }

        [Fact]
        public async Task CheckHealthAsync_ReturnsUnhealthy_WhenStartupTaskNotCompleted()
        {
            var healthCheck = new CronJobServiceHealthCheck { StartupTaskCompleted = false };

            var result = await healthCheck.CheckHealthAsync(new HealthCheckContext(), TestContext.Current.CancellationToken);

            Assert.Equal(HealthStatus.Unhealthy, result.Status);
        }
    }
}
