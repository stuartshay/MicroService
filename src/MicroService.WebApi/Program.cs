using System;
using System.IO;
using System.Linq;
using App.Metrics;
using App.Metrics.AspNetCore;
using App.Metrics.Formatters.Prometheus;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace MicroService.WebApi
{
    /// <summary>
    /// Represents the current application.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// 
        /// </summary>
        public static IMetricsRoot Metrics { get; set; }

        /// <summary>
        /// Configuration
        /// </summary>
        public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json", optional: true)
            .Build();

        /// <summary>
        /// The main entry point to the application.
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            Metrics = AppMetrics.CreateDefaultBuilder()
                .OutputMetrics.AsPrometheusPlainText()
                .Build();

            BuildWebHost(args).Run();
        }

        /// <summary>
        /// Builds a new host for the application.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IHost BuildWebHost(string[] args) =>
           Host.CreateDefaultBuilder(args)
                   .ConfigureAppMetricsHostingConfiguration(options =>
                   {
                   })
                   .ConfigureWebHostDefaults(webBuilder =>
                   {
                     webBuilder.UseStartup<Startup>();
                   })
                   .UseMetrics(options =>
                    {
                        options.EndpointOptions = endpointsOptions =>
                        {
                            endpointsOptions.MetricsTextEndpointEnabled = true;
                            endpointsOptions.EnvironmentInfoEndpointEnabled = true;
                            endpointsOptions.MetricsEndpointEnabled = true;

                            //endpointsOptions.MetricsTextEndpointOutputFormatter = Metrics.OutputMetricsFormatters.OfType<MetricsPrometheusTextOutputFormatter>().First();
                            endpointsOptions.MetricsEndpointOutputFormatter = Metrics.OutputMetricsFormatters.OfType<MetricsPrometheusTextOutputFormatter>().First();
                        };
                    })
                   .Build();
    }
}
