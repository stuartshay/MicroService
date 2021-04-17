using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MicroService.WebApi.Extensions
{
    public static class SystemHealthCheckBuilderExtensions
    {
        public static IHealthChecksBuilder AddFolderHealthCheck(this IHealthChecksBuilder builder, string folderPath, string name = default, HealthStatus? failureStatus = default, IEnumerable<string> tags = default, TimeSpan? timeout = default)
        {
            return builder.Add(new HealthCheckRegistration(
                name ?? "folder",
                sp => new FolderHealthCheck(folderPath),
                failureStatus,
                tags,
                timeout));
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class FolderHealthCheck : IHealthCheck
    {
        private readonly string _folderPath;
        private readonly string _exists;

        /// <summary>
        /// 
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
