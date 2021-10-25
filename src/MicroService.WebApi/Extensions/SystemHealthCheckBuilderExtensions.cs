using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MicroService.WebApi.Extensions
{
    /// <summary>
    /// System Health Check Builder Extensions
    /// </summary>
    public static class SystemHealthCheckBuilderExtensions
    {
        /// <summary>
        /// /
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="folderPath"></param>
        /// <param name="name"></param>
        /// <param name="failureStatus"></param>
        /// <param name="tags"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static IHealthChecksBuilder AddFolderHealthCheck(this IHealthChecksBuilder builder, string folderPath, string name = default, HealthStatus? failureStatus = default, IEnumerable<string> tags = default, TimeSpan? timeout = default) => builder.Add(new HealthCheckRegistration(
                name ?? "folder",
                sp => new FolderHealthCheck(folderPath),
                failureStatus,
                tags,
                timeout));
    }

    /// <summary>
    /// Folder Health Check
    /// </summary>
    public class FolderHealthCheck : IHealthCheck
    {
        private readonly string _folderPath;

        /// <summary>
        /// Initializes a new instance of the <see cref="FolderHealthCheck"/> class.
        /// </summary>
        /// <param name="folderPath"></param>
        public FolderHealthCheck(string folderPath)
        {
            _folderPath = folderPath;
        }

        /// <inheritdoc/>
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            bool exists = System.IO.Directory.Exists(_folderPath);

            var data = new Dictionary<string, object>
            {
                {"FolderPath", _folderPath},
            };

            var healthStatus = exists ? HealthStatus.Healthy : HealthStatus.Unhealthy;
            var healthCheckResult = new HealthCheckResult(healthStatus, "Folder Path Health Check", null, data);
            return Task.FromResult(healthCheckResult);
        }
    }
}
