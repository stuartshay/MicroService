using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Net;
using System.Text.Json;
using Xunit;

namespace MicroService.Test.Extensions
{
    public class OperationalEndpointTest
    {
        [Theory]
        [InlineData("CloudRun", false)]
        [InlineData("Docker", true)]
        public async Task HealthDetailsAndMetrics_AreExposedOnlyOutsideCloudRun(string environment, bool exposeDetails)
        {
            using var factory = new ApiFactory(environment);
            using var client = factory.CreateClient();
            using var response = await client.GetAsync("/status", TestContext.Current.CancellationToken);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
            Assert.Equal("Healthy", json.RootElement.GetProperty("status").GetString());
            Assert.Equal(exposeDetails, json.RootElement.TryGetProperty("entries", out _));
            if (!exposeDetails)
            {
                Assert.Single(json.RootElement.EnumerateObject());
            }

            using var metrics = await client.GetAsync("/metrics", TestContext.Current.CancellationToken);
            Assert.Equal(exposeDetails ? HttpStatusCode.OK : HttpStatusCode.NotFound, metrics.StatusCode);
            using var health = await client.GetAsync("/health", TestContext.Current.CancellationToken);
            Assert.Equal("Healthy", await health.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        }

        [Fact]
        public async Task UnhealthyCloudRunStatus_Returns503WithoutExceptionDetails()
        {
            using var factory = new ApiFactory("CloudRun", unhealthy: true);
            using var client = factory.CreateClient();
            using var response = await client.GetAsync("/status", TestContext.Current.CancellationToken);
            Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
            Assert.Single(json.RootElement.EnumerateObject());
            Assert.Equal("Unhealthy", json.RootElement.GetProperty("status").GetString());
        }

        private sealed class ApiFactory(string environment, bool unhealthy = false) : WebApplicationFactory<Program>
        {
            protected override void ConfigureWebHost(IWebHostBuilder builder)
            {
                builder.UseEnvironment(environment);
                builder.UseSetting("HealthCheckConfiguration:HealthCheckJson", "/status");
                var root = new DirectoryInfo(AppContext.BaseDirectory);
                while (root != null && !File.Exists(Path.Join(root.FullName, "MicroService.sln")))
                {
                    root = root.Parent;
                }
                builder.UseSetting("ShapeConfiguration:ShapeRootDirectory", Path.Join(root!.FullName, "files"));
                if (unhealthy)
                {
                    builder.ConfigureServices(services => services.AddHealthChecks().AddCheck(
                        "private-failure", () => HealthCheckResult.Unhealthy("private diagnostic details")));
                }
            }
        }
    }
}
