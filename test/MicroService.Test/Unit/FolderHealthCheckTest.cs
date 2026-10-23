using MicroService.WebApi.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Xunit;

namespace MicroService.Test.Unit
{
    public class FolderHealthCheckTest
    {
        [Fact]
        public void AddFolderHealthCheck_RegistersHealthCheck()
        {
            var services = new ServiceCollection();

            services.AddHealthChecks().AddFolderHealthCheck(Path.GetTempPath(), name: "test-folder");

            using var provider = services.BuildServiceProvider();
            var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<HealthCheckServiceOptions>>().Value;

            Assert.Contains(options.Registrations, r => r.Name == "test-folder");
        }

        [Fact]
        public async Task CheckHealthAsync_ReturnsHealthy_WhenFolderExists()
        {
            var healthCheck = new FolderHealthCheck(Path.GetTempPath());

            var result = await healthCheck.CheckHealthAsync(new HealthCheckContext(), TestContext.Current.CancellationToken);

            Assert.Equal(HealthStatus.Healthy, result.Status);
        }

        [Fact]
        public async Task CheckHealthAsync_ReturnsUnhealthy_WhenFolderDoesNotExist()
        {
            var missingPath = Path.Join(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            var healthCheck = new FolderHealthCheck(missingPath);

            var result = await healthCheck.CheckHealthAsync(new HealthCheckContext(), TestContext.Current.CancellationToken);

            Assert.Equal(HealthStatus.Unhealthy, result.Status);
        }
    }
}
