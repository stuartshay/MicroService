using HealthChecks.UI.Client;
using MicroService.Common.Constants;
using MicroService.Common.Health;
using MicroService.Common.Logging;
using MicroService.Data.Repository;
using MicroService.Service.Configuration;
using MicroService.Service.Helpers;
using MicroService.Service.Interfaces;
using MicroService.Service.Models;
using MicroService.Service.Models.Base;
using MicroService.Service.Models.Enum;
using MicroService.Service.Services;
using MicroService.Service.Services.FlatFileService;
using MicroService.WebApi.Extensions;
using MicroService.WebApi.Extensions.Swagger;
using MicroService.WebApi.Services;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Prometheus;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var services = builder.Services;
var env = builder.Environment;

SetupLogger();
SetupConfiguration();
SetupServices();
AddMappings();
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
    services.AddCorsConfiguration();

    services.AddControllers().AddJsonOptions(configure =>
    {
        configure.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        configure.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
        configure.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        configure.JsonSerializerOptions.WriteIndented = true;
    });

    services.AddEndpointsApiExplorer();
    services.AddCustomApiVersioning();

    services.AddHttpContextAccessor();

    // Open Telemetry Tracing
    services.AddOpenTelemetryTracingCustom(configuration, env);

    services.AddMemoryCache();
    services.AddSingleton<CronJobServiceHealthCheck>();

    // Swagger 
    services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
    services.AddSwaggerConfiguration();

    // Razor Pages
    services.AddRazorPages();
}

void AddServices()
{
    var config = configuration.Get<ApplicationOptions>();

    // Repositories
    services.AddScoped<ITestDataRepository>(x => new TestDataRepository(config!.ConnectionStrings.PostgreSql));

    // Services
    services.AddScoped<ICalculationService, CalculationService>();

    services.AddScoped<ShapefileDataReaderResolver>(serviceProvider => key =>
    {
        ShapeAttributes? shapeProperties;
        if (key == nameof(ShapeProperties.BoroughBoundaries))
            shapeProperties = ShapeProperties.BoroughBoundaries.GetAttribute<ShapeAttributes>();
        else if (key == nameof(ShapeProperties.CommunityDistricts))
            shapeProperties = ShapeProperties.CommunityDistricts.GetAttribute<ShapeAttributes>();
        else if (key == nameof(ShapeProperties.DSNYDistricts))
            shapeProperties = ShapeProperties.DSNYDistricts.GetAttribute<ShapeAttributes>();
        else if (key == nameof(ShapeProperties.HistoricDistricts))
            shapeProperties = ShapeProperties.HistoricDistricts.GetAttribute<ShapeAttributes>();
        else if (key == nameof(ShapeProperties.Neighborhoods))
            shapeProperties = ShapeProperties.Neighborhoods.GetAttribute<ShapeAttributes>();
        else if (key == nameof(ShapeProperties.NeighborhoodTabulationAreas))
            shapeProperties = ShapeProperties.NeighborhoodTabulationAreas.GetAttribute<ShapeAttributes>();
        else if (key == nameof(ShapeProperties.NypdPolicePrecincts))
            shapeProperties = ShapeProperties.NypdPolicePrecincts.GetAttribute<ShapeAttributes>();
        else if (key == nameof(ShapeProperties.NypdSectors))
            shapeProperties = ShapeProperties.NypdSectors.GetAttribute<ShapeAttributes>();
        else if (key == nameof(ShapeProperties.NychaDevelopments))
            shapeProperties = ShapeProperties.NychaDevelopments.GetAttribute<ShapeAttributes>();
        else if (key == nameof(ShapeProperties.Parks))
            shapeProperties = ShapeProperties.Parks.GetAttribute<ShapeAttributes>();
        else if (key == nameof(ShapeProperties.ScenicLandmarks))
            shapeProperties = ShapeProperties.ScenicLandmarks.GetAttribute<ShapeAttributes>();
        else if (key == nameof(ShapeProperties.Subway))
            shapeProperties = ShapeProperties.Subway.GetAttribute<ShapeAttributes>();
        else if (key == nameof(ShapeProperties.ZipCodes))
            shapeProperties = ShapeProperties.ZipCodes.GetAttribute<ShapeAttributes>();
        else
            throw new KeyNotFoundException(key);

        var options = serviceProvider.GetService<IOptions<ApplicationOptions>>();
        var cache = serviceProvider.GetService<IMemoryCache>();

        var rootDirectory = options?.Value?.ShapeConfiguration.ShapeSystemRootDirectory;
        string shapeFileNamePath = Path.Combine(rootDirectory!, shapeProperties.Directory, shapeProperties.FileName);

        return new CachedShapefileDataReader(cache, key, shapeFileNamePath);
    });

    // Feature Service Lookups
    services.AddScoped<BoroughBoundariesService>();
    services.AddScoped<CommunityDistrictsService>();
    services.AddScoped<DSNYDistrictsService>();
    services.AddScoped<HistoricDistrictService>();
    services.AddScoped<NeighborhoodsService<NeighborhoodShape>>();
    services.AddScoped<NeighborhoodTabulationAreasService>();
    services.AddScoped<NypdPolicePrecinctService>();
    services.AddScoped<NypdSectorsService<NypdSectorShape>>();
    services.AddScoped<NychaDevelopmentService<NychaDevelopmentShape>>();
    services.AddScoped<ParkService<ParkShape>>();
    services.AddScoped<ScenicLandmarkService>();
    services.AddScoped<SubwayService<SubwayShape>>();
    services.AddScoped<ZipCodeService<ZipCodeShape>>();

    services.AddScoped<ShapeServiceResolver>(serviceProvider => key =>
    {
        return (key switch
        {
            nameof(ShapeProperties.BoroughBoundaries) => serviceProvider.GetService<BoroughBoundariesService>(),
            nameof(ShapeProperties.CommunityDistricts) => serviceProvider.GetService<CommunityDistrictsService>(),
            nameof(ShapeProperties.DSNYDistricts) => serviceProvider.GetService<DSNYDistrictsService>(),
            nameof(ShapeProperties.HistoricDistricts) => serviceProvider.GetService<HistoricDistrictService>(),
            nameof(ShapeProperties.Neighborhoods) => serviceProvider.GetService<NeighborhoodsService<NeighborhoodShape>>(),
            nameof(ShapeProperties.NeighborhoodTabulationAreas) => serviceProvider.GetService<NeighborhoodTabulationAreasService>(),
            nameof(ShapeProperties.NypdPolicePrecincts) => serviceProvider.GetService<NypdPolicePrecinctService>(),
            nameof(ShapeProperties.NypdSectors) => serviceProvider.GetService<NypdSectorsService<NypdSectorShape>>(),
            nameof(ShapeProperties.NychaDevelopments) => serviceProvider.GetService<NychaDevelopmentService<NychaDevelopmentShape>>(),
            nameof(ShapeProperties.Parks) => serviceProvider.GetService<ParkService<ParkShape>>(),
            nameof(ShapeProperties.ScenicLandmarks) => serviceProvider.GetService<ScenicLandmarkService>(),
            nameof(ShapeProperties.Subway) => serviceProvider.GetService<SubwayService<SubwayShape>>(),
            nameof(ShapeProperties.ZipCodes) => serviceProvider.GetService<ZipCodeService<ZipCodeShape>>(),
            _ => throw new KeyNotFoundException(key)
        })!;
    });

    // Flat File Service Lookups
    services.AddScoped<StationFlatFileService>();
    services.AddScoped<StationComplexFlatFileService>();

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
        x.CronExpression = config!.ShapeConfiguration.CronExpression;
    });
}

void AddMappings()
{
}

void AddHealthCheckServices()
{
    var config = configuration.Get<ApplicationOptions>();
    var shapeDirectory = config!.ShapeConfiguration.ShapeRootDirectory;
    string shapePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), shapeDirectory));

    services
        // .AddHealthChecksUI()
        // .AddInMemoryStorage()
        // .Services
        .AddHealthChecks()
        .AddVersionHealthCheck()
        .AddFolderHealthCheck(shapePath, "Shape Root Directory")
        .AddCheck<CronJobServiceHealthCheck>("Cron Job Health Check", tags: new[] { HealthCheckType.ReadinessCheck.ToString() })
        .AddNpgSql(config.ConnectionStrings.PostgreSql)
        .ForwardToPrometheus()
        .Services
        .AddControllers();
}

void SetupApp()
{
    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseCors();

    var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
    app.UseSwaggerCustom(provider);

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.UseHttpMetrics();

    app.UseEndpoints(endpoints =>
    {
        endpoints.MapMetrics();
        endpoints.MapHealthChecks("/healthz", new HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });
    });

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
public delegate IFlatFileService<FlatFileBase> FlatFileResolver(string key);