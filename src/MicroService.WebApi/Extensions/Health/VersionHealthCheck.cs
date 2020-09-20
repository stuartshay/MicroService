using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace MicroService.WebApi.Extensions.Health
{
    /// <summary>
    /// 
    /// </summary>
    public class VersionHealthCheck : IHealthCheck
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            var applicationVersionNumber = GetType().GetTypeInfo().Assembly.GetName().Version.ToString();

            var result = Task.FromResult(string.IsNullOrEmpty(applicationVersionNumber) ? HealthCheckResult.Unhealthy("failed") : HealthCheckResult.Healthy(applicationVersionNumber));
            return result;
        }
    }

}
