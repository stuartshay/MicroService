using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Prometheus;

namespace MicroService.WebApi.Extensions
{
    /// <summary>Maps health and metrics endpoints without exposing runtime details in Cloud Run.</summary>
    public static class OperationalEndpointExtensions
    {
        /// <summary>Publishes summary health in Cloud Run and diagnostic endpoints locally.</summary>
        public static void MapOperationalEndpoints(this WebApplication app, string statusPath)
        {
            var cloudRun = app.Environment.IsEnvironment("CloudRun");
            if (!cloudRun)
            {
                app.UseHttpMetrics();
                app.MapMetrics();
            }

            app.MapHealthChecks(statusPath, new HealthCheckOptions
            {
                Predicate = _ => true,
                ResponseWriter = cloudRun
                    ? (context, report) => context.Response.WriteAsJsonAsync(new { status = report.Status.ToString() })
                    : UIResponseWriter.WriteHealthCheckUIResponse
            });
        }
    }
}
