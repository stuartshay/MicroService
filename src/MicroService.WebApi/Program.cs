using Asp.Versioning.ApiExplorer;
using MicroService.Common.Constants;
using MicroService.Common.Health;
using MicroService.Common.Logging;
using MicroService.Service.Configuration;
using MicroService.Service.Interfaces;
using MicroService.Service.Models.Base;
using MicroService.Service.Models.Enum;
using MicroService.Service.Services.Base;
using MicroService.Service.Services.FlatFileService;
using MicroService.Service.Services.Subway;
using MicroService.WebApi.Extensions;
using MicroService.WebApi.Extensions.OpenApi;
using MicroService.WebApi.Services;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NetTopologySuite.IO.Converters;
using Prometheus;
using Serilog;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var services = builder.Services;
var env = builder.Environment;

SetupLogger();
SetupConfiguration();
SetupServices();
AddServices();
AddHealthCheckServices();

var app = builder.Build();
SetupApp();
app.Run();

void SetupLogger()
{
    builder.Host.UseSerilog(Logging.ConfigureLogger);
}

void SetupConfiguration()
{
    services.AddOptions();
    services.Configure<ApplicationOptions>(configuration);
    services.Configure<MicroService.Common.Configuration.ApplicationOptions>(configuration);
    services.AddSingleton(configuration);

    services.DisplayConfiguration(configuration);
}

void SetupServices()
{
    // Cors Configuration
    services.AddCorsConfiguration(configuration);

    services.AddControllers().AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.WriteIndented = true;
        options.JsonSerializerOptions.Converters.Add(new GeoJsonConverterFactory());
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

    services.AddEndpointsApiExplorer();
    services.AddCustomApiVersioning();

    services.AddHttpContextAccessor();

    services.AddMemoryCache();
    services.AddSingleton<CronJobServiceHealthCheck>();

    // OpenAPI documents (Microsoft.AspNetCore.OpenApi)
    services.AddOpenApiDocuments();

    // Razor Pages
    services.AddRazorPages();
}

void AddServices()
{
    var config = configuration.Get<ApplicationOptions>()
        ?? throw new InvalidOperationException("Application configuration could not be loaded.");
    var shapeConfiguration = config.ShapeConfiguration
        ?? throw new InvalidOperationException("ShapeConfiguration is required.");

    services.AddScoped<ShapefileDataReaderResolver>(serviceProvider => key =>
        ShapeRegistry.CreateDataReader(
            serviceProvider.GetRequiredService<IMemoryCache>(),
            serviceProvider.GetRequiredService<IOptions<ApplicationOptions>>(),
            key));

    foreach (var registration in ShapeRegistry.All)
    {
        services.AddScoped(registration.ServiceType);
    }

    // Register Shape Service Resolver
    services.AddScoped<ShapeServiceResolver>(serviceProvider => key =>
        ShapeRegistry.ResolveService(serviceProvider, key));

    // Flat File Service Lookups
    services.AddScoped<StationFlatFileService>();
    services.AddScoped<StationComplexFlatFileService>();

    // Subway services, lines and nearby stations (api/v1/Subway)
    services.AddSingleton<SubwayLineCache>();
    services.AddScoped<SubwayRouteService>();
    services.AddScoped<SubwayStationLookupService>();

    services.AddScoped<FlatFileResolver>(serviceProvider => key =>
    {
        return (key switch
        {
            nameof(FlatFileProperties.SubwayStationLocations) => serviceProvider.GetService<StationFlatFileService>(),
            nameof(FlatFileProperties.SubwayStationComplex) => serviceProvider.GetService<StationComplexFlatFileService>(),
            _ => throw new KeyNotFoundException(key)
        })!;
    });

    // Cron Job Service
    services.AddCronJob<InMemoryCacheShapefileCronJobService>(x =>
    {
        x.TimeZoneInfo = TimeZoneInfo.Local;
        x.CronExpression = shapeConfiguration.CronExpression;
    });
}

void AddHealthCheckServices()
{
    var config = configuration.Get<ApplicationOptions>()
        ?? throw new InvalidOperationException("Application configuration could not be loaded.");
    var shapeDirectory = config.ShapeConfiguration?.ShapeRootDirectory
        ?? throw new InvalidOperationException("ShapeConfiguration:ShapeRootDirectory configuration is required.");
    string shapePath = Path.GetFullPath(shapeDirectory, Directory.GetCurrentDirectory());

    services
        .AddHealthChecks()
        .AddVersionHealthCheck()
        .AddFolderHealthCheck(shapePath, "Shape Root Directory")
        .AddCheck<CronJobServiceHealthCheck>("Cron Job Health Check", tags: new[] { HealthCheckType.ReadinessCheck.ToString() })
        .ForwardToPrometheus()
        .Services
        .AddControllers();
}

void SetupApp()
{
    var config = configuration.Get<MicroService.Common.Configuration.ApplicationOptions>();

    app.UseHttpsRedirectionOutsideCloudRun(app.Environment);
    app.UseStaticFiles();
    app.UseRouting();
    app.UseCors();

    app.MapOpenApiDocuments();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.MapOperationalEndpoints(config!.HealthCheckConfiguration!.HealthCheckJson!);

    app.UseHealthChecks($"/health");

    var option = new RewriteOptions();
    option.AddRedirect("^$", "swagger");
    app.UseRewriter(option);
}

/// <summary>
/// Shape Service Resolver
/// </summary>
/// <param name="key"></param>
/// <returns></returns>
public delegate IShapeService<ShapeBase> ShapeServiceResolver(string key);

/// <summary>
/// Flat File Service
/// </summary>
/// <param name="key"></param>
/// <returns></returns>
public delegate IFlatFileService FlatFileResolver(string key);
