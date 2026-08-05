using Microsoft.Extensions.DependencyInjection;

namespace MicroService.Common.Health
{
    public static class HealthCheckServerExtensions
    {
        private static readonly string[] SystemTag = { "System" };

        public static IHealthChecksBuilder AddVersionHealthCheck(this IHealthChecksBuilder builder)
        {
            builder.AddCheck<VersionHealthCheck>("Version Health Check", tags: SystemTag);

            return builder;
        }
    }
}
