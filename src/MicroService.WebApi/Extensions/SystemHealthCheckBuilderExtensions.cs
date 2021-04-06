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

    public class FolderHealthCheck : IHealthCheck
    {
        private readonly string _folderPath;

        public FolderHealthCheck(string folderPath)
        {
            _folderPath = folderPath;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                if (Directory.Exists(Path.GetFullPath(_folderPath)))
                    return Task.FromResult(HealthCheckResult.Healthy());

                return Task.FromResult(
                    new HealthCheckResult(context.Registration.FailureStatus, description: $"Folder path {_folderPath} is not exists on system"));
            }
            catch (Exception ex)
            {
                return Task.FromResult(
                     new HealthCheckResult(context.Registration.FailureStatus, exception: ex));
            }
        }
    }
}
