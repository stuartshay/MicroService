using MicroService.Service.Configuration;
using MicroService.Service.Helpers;
using MicroService.Service.Interfaces;
using MicroService.Service.Models;
using MicroService.Service.Models.Enum;
using MicroService.Service.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MicroService.Test.Fixture
{
    public class ShapeServiceFixture : FixtureConfig
    {
        public ShapeServiceFixture()
        {
            var serviceProvider = new ServiceCollection()
                .AddMemoryCache()
                .AddScoped<ShapefileDataReaderResolver>(serviceProvider => key =>
                {
                    ShapeAttributes shapeProperties = null!;
                    if (key == nameof(ShapeProperties.BoroughBoundaries))
                        shapeProperties = ShapeProperties.BoroughBoundaries.GetAttribute<ShapeAttributes>();

                    var options = serviceProvider.GetService<IOptions<ApplicationOptions>>();
                    var cache = serviceProvider.GetService<IMemoryCache>();

                    var shapeDirectory = $"{Path.Combine(options!.Value.ShapeConfiguration.ShapeRootDirectory, shapeProperties!.Directory, shapeProperties.FileName)}";
                    string shapePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), shapeDirectory));

                    return new CachedShapefileDataReader(cache, key, shapePath);
                })
                .AddScoped<BoroughBoundariesService>()
                .AddOptions()
                .Configure<ApplicationOptions>(Configuration)
                .AddSingleton(Configuration)
                .BuildServiceProvider();

            BoroughBoundariesService = serviceProvider.GetRequiredService<BoroughBoundariesService>();
        }

        public IShapeService<BoroughBoundaryShape> BoroughBoundariesService { get; set; }

        public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json", optional: true)
            .Build();
    }
}
