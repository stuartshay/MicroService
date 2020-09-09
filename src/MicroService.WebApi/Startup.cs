using App.Metrics;
using HealthChecks.UI.Client;
using MicroService.Data.Repository;
using MicroService.Service.Configuration;
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
            services.AddScoped<IBoroughBoundariesService, BoroughBoundariesService>();

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
