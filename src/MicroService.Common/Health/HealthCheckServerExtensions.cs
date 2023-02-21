using Microsoft.Extensions.DependencyInjection;

namespace MicroService.Common.Health
{
    public static class HealthCheckServerExtensions
    {
        public static IHealthChecksBuilder AddVersionHealthCheck(this IHealthChecksBuilder builder)
        {
            builder.AddCheck<VersionHealthCheck>("Version Health Check", tags: new[] { "System" });

            return builder;
        }
    }
}
