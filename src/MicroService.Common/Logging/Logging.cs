using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Sinks.Grafana.Loki;
using System.Reflection;

namespace MicroService.Common.Logging
{
    public static class Logging
    {
        public static Action<HostBuilderContext, LoggerConfiguration> ConfigureLogger =>
           (hostingContext, loggerConfiguration) =>
           {
               var env = hostingContext.HostingEnvironment;
               var lokiUri = hostingContext.Configuration.GetValue<string>("GrafanaLokiConfiguration:Uri");
               var lokiEnabled = hostingContext.Configuration.GetValue<bool>("GrafanaLokiConfiguration:Enabled");

               loggerConfiguration.MinimumLevel.Information()
                   .Enrich.FromLogContext()
                   .Enrich.WithExceptionDetails()
                   .Enrich.WithThreadId()
                   .Enrich.WithMachineName()
                   .Enrich.WithProperty("Version", Assembly.GetEntryAssembly()?.GetName().Version!)
                   .Enrich.WithProperty("ApplicationName", env.ApplicationName)
                   .Enrich.WithProperty("EnvironmentName", env.EnvironmentName)
                   .Enrich.WithExceptionDetails()
                   .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                   .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning)
                   .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                   .WriteTo.Console();

               if (!string.IsNullOrEmpty(lokiUri) && lokiEnabled)
               {
                   loggerConfiguration.WriteTo.GrafanaLoki(lokiUri);
               }
           };

    }
}
