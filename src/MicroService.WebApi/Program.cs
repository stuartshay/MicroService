using System;
using System.IO;
using App.Metrics;
using App.Metrics.AspNetCore;
using App.Metrics.Formatters.Prometheus;
using Microsoft.AspNetCore;
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
            CreateHostBuilder(args).Build().Run();
        }

        /// <summary>
        /// Builds a new host for the application.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
           Host.CreateDefaultBuilder(args)
               .ConfigureWebHostDefaults(webBuilder =>
               {
                   webBuilder.UseStartup<Startup>()
                        .ConfigureMetricsWithDefaults(builder => { builder.OutputMetrics.AsPrometheusPlainText(); })
                        .UseMetrics(options =>
                         {
                             options.EndpointOptions = endpointsOptions =>
                             {
                                endpointsOptions.MetricsTextEndpointOutputFormatter =
                                new MetricsPrometheusTextOutputFormatter();
                             };
                         });
               });


        ///// <summary>
        ///// Builds a new web host for the application.
        ///// </summary>
        ///// <param name="args"></param>
        ///// <returns></returns>
        //public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
        //    WebHost.CreateDefaultBuilder(args)
        //     .ConfigureMetricsWithDefaults(builder => { builder.OutputMetrics.AsPrometheusPlainText(); })
        //     .UseMetrics(
        //            options =>
        //            {
        //                options.EndpointOptions = endpointsOptions =>
        //                {
        //                    endpointsOptions.MetricsTextEndpointOutputFormatter =
        //                        new MetricsPrometheusTextOutputFormatter();
        //                };
        //            })
        //    .UseStartup<Startup>()
        //    .CaptureStartupErrors(true)
        //    .UseConfiguration(Configuration);
    }
}
