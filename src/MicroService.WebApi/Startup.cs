﻿using System.Collections.Generic;
using System.IO;
using App.Metrics;
using HealthChecks.UI.Client;
using MicroService.Data.Repository;
using MicroService.Service.Configuration;
using MicroService.Service.Helpers;
using MicroService.Service.Interfaces;
using MicroService.Service.Models;
using MicroService.Service.Models.Base;
using MicroService.Service.Models.Enum;
using MicroService.Service.Models.FlatFileModels;
using MicroService.Service.Services;
using MicroService.Service.Services.FlatFileService;
using MicroService.WebApi.Extensions;
using MicroService.WebApi.Extensions.Swagger;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Caching.Memory;
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
        /// Shape Service Resolver
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public delegate IShapeService<ShapeBase> ShapeServiceResolver(string key);

        public delegate IFlatFileService<FlatFileBase> FlatFileResolver(string key);

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
            services.AddMemoryCache();

            var config = Configuration.Get<ApplicationOptions>();
            services.DisplayConfiguration(Configuration, HostingEnvironment);

            services.AddCustomApiVersioning();
            services.AddCustomHealthCheck(Configuration);

            var metrics = AppMetrics.CreateDefaultBuilder()
                .OutputMetrics
                .AsPrometheusPlainText()
                .Build();

            MetricsHelpers.SetMetricsCustomTag(metrics, "OSDescription", System.Runtime.InteropServices.RuntimeInformation.OSDescription);

            metrics.Options.ReportingEnabled = true;
            services.AddMetrics(metrics);
            services.AddAppMetricsHealthPublishing();
            services.AddAppMetricsCollectors();

            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
            services.AddSwaggerConfiguration();
            services.AddCorsConfiguration();

            // Repositories
            services.AddScoped<ITestDataRepository>(x => new TestDataRepository(config.ConnectionStrings.PostgreSql));

            // Services
            services.AddScoped<ICalculationService, CalculationService>();

            services.AddScoped<ShapefileDataReaderResolver>(serviceProvider => key =>
            {
                ShapeAttributes shapeProperties = null;
                if (key == nameof(ShapeProperties.BoroughBoundaries))
                    shapeProperties = ShapeProperties.BoroughBoundaries.GetAttribute<ShapeAttributes>();
                else if (key == nameof(ShapeProperties.HistoricDistricts))
                    shapeProperties = ShapeProperties.HistoricDistricts.GetAttribute<ShapeAttributes>();
                else if (key == nameof(ShapeProperties.Neighborhoods))
                    shapeProperties = ShapeProperties.Neighborhoods.GetAttribute<ShapeAttributes>();
                else if (key == nameof(ShapeProperties.NypdPolicePrecincts))
                    shapeProperties = ShapeProperties.NypdPolicePrecincts.GetAttribute<ShapeAttributes>();
                else if (key == nameof(ShapeProperties.NypdSectors))
                    shapeProperties = ShapeProperties.NypdSectors.GetAttribute<ShapeAttributes>();
                else if (key == nameof(ShapeProperties.Parks))
                    shapeProperties = ShapeProperties.Parks.GetAttribute<ShapeAttributes>();
                else if (key == nameof(ShapeProperties.Subway))
                    shapeProperties = ShapeProperties.Subway.GetAttribute<ShapeAttributes>();
                else if (key == nameof(ShapeProperties.ZipCodes))
                    shapeProperties = ShapeProperties.ZipCodes.GetAttribute<ShapeAttributes>();
                else
                    throw new KeyNotFoundException(key);

                var options = serviceProvider.GetService<IOptions<ApplicationOptions>>();
                var cache = serviceProvider.GetService<IMemoryCache>();

                var shapeDirectory = $"{Path.Combine(options.Value.ShapeConfiguration.ShapeRootDirectory, shapeProperties.Directory, shapeProperties.FileName)}";
                string shapePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), shapeDirectory));

                return new CachedShapefileDataReader(cache, key, shapePath);
            });

            // Feature Service Lookups
            services.AddScoped<BoroughBoundariesService>();
            services.AddScoped<HistoricDistrictService>();
            services.AddScoped<NeighborhoodsService<NeighborhoodShape>>();
            services.AddScoped<NypdPolicePrecinctService>();
            services.AddScoped<NypdSectorsService<NypdSectorShape>>();
            services.AddScoped<ParkService<ParkShape>>();
            services.AddScoped<SubwayService<SubwayShape>>();
            services.AddScoped<ZipCodeService<ZipCodeShape>>();

            services.AddScoped<ShapeServiceResolver>(serviceProvider => key =>
            {
                return key switch
                {
                    nameof(ShapeProperties.BoroughBoundaries) => serviceProvider.GetService<BoroughBoundariesService>(),
                    nameof(ShapeProperties.HistoricDistricts) => serviceProvider.GetService<HistoricDistrictService>(),
                    nameof(ShapeProperties.Neighborhoods) => serviceProvider.GetService<NeighborhoodsService<NeighborhoodShape>>(),
                    nameof(ShapeProperties.NypdPolicePrecincts) => serviceProvider.GetService<NypdPolicePrecinctService>(),
                    nameof(ShapeProperties.NypdSectors) => serviceProvider.GetService<NypdSectorsService<NypdSectorShape>>(),
                    nameof(ShapeProperties.Parks) => serviceProvider.GetService<ParkService<ParkShape>>(),
                    nameof(ShapeProperties.Subway) => serviceProvider.GetService<SubwayService<SubwayShape>>(),
                    nameof(ShapeProperties.ZipCodes) => serviceProvider.GetService<ZipCodeService<ZipCodeShape>>(),
                    _ => throw new KeyNotFoundException(key)
                };
            });

            // Flat File Service Lookups
            services.AddScoped<StationFlatFileService>();
            services.AddScoped<StationComplexFlatFileService>();

            services.AddScoped<FlatFileResolver>(serviceProvider => key =>
            {
                return key switch
                {
                    nameof(FlatFileProperties.SubwayStationLocations) => serviceProvider.GetService<StationFlatFileService>(),
                    nameof(FlatFileProperties.SubwayStationComplex) => serviceProvider.GetService<StationComplexFlatFileService>(),
                    _ => throw new KeyNotFoundException(key)
                };
            });


            services.AddCustomControllers(Configuration);
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });
        }

        /// <summary>
        /// Configure
        /// </summary>
        /// <param name="app">IApplicationBuilder</param>
        /// <param name="env">IWebHostEnvironment</param>
        /// <param name="provider">IApiVersionDescriptionProvider</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider provider)
        {
            var forwardedHeaderOptions = new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
            };
            forwardedHeaderOptions.KnownNetworks.Clear();
            forwardedHeaderOptions.KnownProxies.Clear();
            app.UseForwardedHeaders(forwardedHeaderOptions);

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
