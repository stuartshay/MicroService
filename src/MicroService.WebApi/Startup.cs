using System.Collections.Generic;
using App.Metrics;
using HealthChecks.UI.Client;
using MicroService.Data.Repository;
using MicroService.Service.Configuration;
using MicroService.Service.Interfaces;
using MicroService.Service.Models;
using MicroService.Service.Models.Base;
using MicroService.Service.Models.Enum;
using MicroService.Service.Services;
using MicroService.WebApi.Extensions;
using MicroService.WebApi.Extensions.Swagger;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MicroService.WebApi
{
    /// <summary>
    /// Startup
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup" /> class.
        /// </summary>
        /// <param name="env"></param>
        /// <param name="configuration"></param>
        public Startup(IWebHostEnvironment env, IConfiguration configuration)
        {
            Configuration = configuration;
            HostingEnvironment = env;
        }

        /// <summary>
        /// Configuration.
        /// </summary>
        public IConfiguration Configuration { get; }

        public delegate IShapeService<ShapeBase> ShapeServiceResolver(string key);

        /// <summary>
        /// WebHost Environment.
        /// </summary>
        private IWebHostEnvironment HostingEnvironment { get; }

        /// <summary>
        /// Configure Services.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.Configure<ApplicationOptions>(Configuration);
            services.AddSingleton(Configuration);

            var config = Configuration.Get<ApplicationOptions>();
            services.DisplayConfiguration(Configuration, HostingEnvironment);

            services.AddCustomApiVersioning();
            services.AddCustomHealthCheck(Configuration);

            var metrics = AppMetrics.CreateDefaultBuilder()
                .OutputMetrics
                .AsPrometheusPlainText()
                .Build();

            metrics.Options.ReportingEnabled = true;

            services.AddMetrics(metrics);
            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
            services.AddSwaggerConfiguration();
            services.AddCorsConfiguration();

            // Repositories
            services.AddScoped<ITestDataRepository>(x => new TestDataRepository(config.ConnectionStrings.PostgreSql));

            // Services
            services.AddScoped<ICalculationService, CalculationService>();

            // Feature Service Lookups
            services.AddScoped<BoroughBoundariesService>();
            services.AddScoped<HistoricDistrictService>();
            services.AddScoped<NypdPolicePrecinctService>();
            services.AddScoped<NypdSectorsService<NypdSectorShape>>();
            services.AddScoped<ZipCodeService<ZipCodeShape>>();

            services.AddScoped<ShapeServiceResolver>(serviceProvider => key =>
            {
                return key switch
                {
                    nameof(ShapeProperties.BoroughBoundaries) => serviceProvider.GetService<BoroughBoundariesService>(),
                    nameof(ShapeProperties.HistoricDistricts) => serviceProvider.GetService<HistoricDistrictService>(),
                    nameof(ShapeProperties.NypdPolicePrecincts) => serviceProvider.GetService<NypdPolicePrecinctService>(),
                    nameof(ShapeProperties.NypdSectors) => serviceProvider.GetService<NypdSectorsService<NypdSectorShape>>(),
                    nameof(ShapeProperties.ZipCodes) => serviceProvider.GetService<ZipCodeService<ZipCodeShape>>(),
                    _ => throw new KeyNotFoundException(key),
                };
            });

            services.AddCustomControllers(Configuration);
        }

        /// <summary>
        /// Configure
        /// </summary>
        /// <param name="app">IApplicationBuilder</param>
        /// <param name="env">IWebHostEnvironment</param>
        /// <param name="provider">IApiVersionDescriptionProvider</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider provider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHealthChecks("/healthz", new HealthCheckOptions()
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
            });

            app.ConfigureSwagger(provider);
            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseCors();
            app.UseMetricsEndpoint();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecksUI(config =>
                {
                    config.UIPath = $"/healthcheck-ui";
                });
                endpoints.MapControllers();
            });
        }
    }
}
