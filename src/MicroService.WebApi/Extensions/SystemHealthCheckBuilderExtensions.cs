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

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            bool exists = System.IO.Directory.Exists(_folderPath);
            if (exists)
            {
                return Task.FromResult(HealthCheckResult.Healthy());
            }
            else
            {
                return Task.FromResult(new HealthCheckResult(
                    context.Registration.FailureStatus,
                    description: $"Folder path {_folderPath} is not exists on system"));
            }
        }
    }
}
