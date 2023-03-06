using AutoMapper;
using HealthChecks.UI.Client;
using MicroService.Common.Constants;
using MicroService.Common.Health;
using MicroService.Common.Logging;
using MicroService.Data.Repository;
using MicroService.Service.Configuration;
using MicroService.Service.Helpers;
using MicroService.Service.Interfaces;
using MicroService.Service.Mappings;
using MicroService.Service.Models.Base;
using MicroService.Service.Models.Enum;
using MicroService.Service.Services;
using MicroService.Service.Services.DataService;
using MicroService.Service.Services.FlatFileService;
using MicroService.WebApi.Extensions;
using MicroService.WebApi.Extensions.Swagger;
using MicroService.WebApi.Services;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NetTopologySuite.IO.Converters;
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
SetupMappings();
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

void SetupMappings()
{
    services.AddSingleton(_ => new MapperConfiguration(cfg =>
    {
        cfg.AddProfile<ShapeMappings>();
        cfg.AddProfile<FeatureToNationalRegisterHistoricPlacesShapeProfile>();
    }).CreateMapper());


}

void SetupServices()
{
    // Cors Configuration
    services.AddCorsConfiguration();

    services.AddControllers().AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.WriteIndented = true;
        options.JsonSerializerOptions.Converters.Add(new GeoJsonConverterFactory());
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
        ShapeAttribute? shapeProperties;
        if (key == nameof(ShapeProperties.BoroughBoundaries))
            shapeProperties = ShapeProperties.BoroughBoundaries.GetAttribute<ShapeAttribute>();
        else if (key == nameof(ShapeProperties.CommunityDistricts))
            shapeProperties = ShapeProperties.CommunityDistricts.GetAttribute<ShapeAttribute>();
        else if (key == nameof(ShapeProperties.DSNYDistricts))
            shapeProperties = ShapeProperties.DSNYDistricts.GetAttribute<ShapeAttribute>();
        else if (key == nameof(ShapeProperties.HistoricDistricts))
            shapeProperties = ShapeProperties.HistoricDistricts.GetAttribute<ShapeAttribute>();
        else if (key == nameof(ShapeProperties.IndividualLandmarkSite))
            shapeProperties = ShapeProperties.IndividualLandmarkSite.GetAttribute<ShapeAttribute>();
        else if (key == nameof(ShapeProperties.NationalRegisterHistoricPlaces))
            shapeProperties = ShapeProperties.NationalRegisterHistoricPlaces.GetAttribute<ShapeAttribute>();
        else if (key == nameof(ShapeProperties.Neighborhoods))
            shapeProperties = ShapeProperties.Neighborhoods.GetAttribute<ShapeAttribute>();
        else if (key == nameof(ShapeProperties.NeighborhoodTabulationAreas))
            shapeProperties = ShapeProperties.NeighborhoodTabulationAreas.GetAttribute<ShapeAttribute>();
        else if (key == nameof(ShapeProperties.NypdPolicePrecincts))
            shapeProperties = ShapeProperties.NypdPolicePrecincts.GetAttribute<ShapeAttribute>();
        else if (key == nameof(ShapeProperties.NypdSectors))
            shapeProperties = ShapeProperties.NypdSectors.GetAttribute<ShapeAttribute>();
        else if (key == nameof(ShapeProperties.NychaDevelopments))
            shapeProperties = ShapeProperties.NychaDevelopments.GetAttribute<ShapeAttribute>();
        else if (key == nameof(ShapeProperties.Parks))
            shapeProperties = ShapeProperties.Parks.GetAttribute<ShapeAttribute>();
        else if (key == nameof(ShapeProperties.ScenicLandmarks))
            shapeProperties = ShapeProperties.ScenicLandmarks.GetAttribute<ShapeAttribute>();
        else if (key == nameof(ShapeProperties.Subway))
            shapeProperties = ShapeProperties.Subway.GetAttribute<ShapeAttribute>();
        else if (key == nameof(ShapeProperties.ZipCodes))
            shapeProperties = ShapeProperties.ZipCodes.GetAttribute<ShapeAttribute>();
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
    services.AddScoped<DsnyDistrictsService>();
    services.AddScoped<HistoricDistrictService>();
    services.AddScoped<IndividualLandmarkSiteService>();

    services.AddScoped<NationalRegisterHistoricPlacesService>();


    services.AddScoped<NeighborhoodsService>();
    services.AddScoped<NeighborhoodTabulationAreasService>();
    services.AddScoped<NypdPolicePrecinctService>();
    services.AddScoped<NypdSectorsService>();
    services.AddScoped<NychaDevelopmentService>();
    services.AddScoped<ParkService>();
    services.AddScoped<ScenicLandmarkService>();
    services.AddScoped<SubwayService>();
    services.AddScoped<ZipCodeService>();

    services.AddScoped<ShapeServiceResolver>(serviceProvider => key =>
    {
        return (key switch
        {
            nameof(ShapeProperties.BoroughBoundaries) => serviceProvider.GetService<BoroughBoundariesService>(),
            nameof(ShapeProperties.CommunityDistricts) => serviceProvider.GetService<CommunityDistrictsService>(),
            nameof(ShapeProperties.DSNYDistricts) => serviceProvider.GetService<DsnyDistrictsService>(),
            nameof(ShapeProperties.HistoricDistricts) => serviceProvider.GetService<HistoricDistrictService>(),
            nameof(ShapeProperties.IndividualLandmarkSite) => serviceProvider.GetService<IndividualLandmarkSiteService>(),
            nameof(ShapeProperties.NationalRegisterHistoricPlaces) => serviceProvider.GetService<NationalRegisterHistoricPlacesService>(),
            nameof(ShapeProperties.Neighborhoods) => serviceProvider.GetService<NeighborhoodsService>(),
            nameof(ShapeProperties.NeighborhoodTabulationAreas) => serviceProvider.GetService<NeighborhoodTabulationAreasService>(),
            nameof(ShapeProperties.NypdPolicePrecincts) => serviceProvider.GetService<NypdPolicePrecinctService>(),
            nameof(ShapeProperties.NypdSectors) => serviceProvider.GetService<NypdSectorsService>(),
            nameof(ShapeProperties.NychaDevelopments) => serviceProvider.GetService<NychaDevelopmentService>(),
            nameof(ShapeProperties.Parks) => serviceProvider.GetService<ParkService>(),
            nameof(ShapeProperties.ScenicLandmarks) => serviceProvider.GetService<ScenicLandmarkService>(),
            nameof(ShapeProperties.Subway) => serviceProvider.GetService<SubwayService>(),
            nameof(ShapeProperties.ZipCodes) => serviceProvider.GetService<ZipCodeService>(),
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
    app.MapMetrics();

    app.MapHealthChecks("/healthz", new HealthCheckOptions
    {
        Predicate = _ => true,
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
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
public delegate IFlatFileService FlatFileResolver(string key);