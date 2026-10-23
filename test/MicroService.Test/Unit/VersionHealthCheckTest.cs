using MicroService.Common.Health;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Reflection;
using Xunit;

namespace MicroService.Test.Unit
{
    public class VersionHealthCheckTest
    {
        [Fact]
        public async Task CheckHealth_ReturnsEntryAssemblyMetadata()
        {
            var check = new VersionHealthCheck();
            var assembly = Assembly.GetEntryAssembly()!;

            var result = await check.CheckHealthAsync(new HealthCheckContext(), TestContext.Current.CancellationToken);

            Assert.Equal(HealthStatus.Healthy, result.Status);
            Assert.Equal(assembly.GetName().Version!.ToString(), result.Description);
            Assert.Equal(result.Description, result.Data["BuildVersion"]);
            Assert.Equal(new FileInfo(assembly.Location).LastWriteTimeUtc, result.Data["BuildDate"]);
            Assert.Equal(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), result.Data["Environment"]);
            Assert.Equal(DateTimeKind.Utc, Assert.IsType<DateTime>(result.Data["BuildDate"]).Kind);
        }
    }
}
