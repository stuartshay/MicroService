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
                    else if (key == nameof(ShapeProperties.CommunityDistricts))
                        shapeProperties = ShapeProperties.CommunityDistricts.GetAttribute<ShapeAttributes>();
                    else if (key == nameof(ShapeProperties.DSNYDistricts))
                        shapeProperties = ShapeProperties.DSNYDistricts.GetAttribute<ShapeAttributes>();
                    else if (key == nameof(ShapeProperties.HistoricDistricts))
                        shapeProperties = ShapeProperties.HistoricDistricts.GetAttribute<ShapeAttributes>();
                    else if (key == nameof(ShapeProperties.IndividualLandmarkSite))
                        shapeProperties = ShapeProperties.IndividualLandmarkSite.GetAttribute<ShapeAttributes>();
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

                    var shapeDirectory = $"{Path.Combine(options!.Value.ShapeConfiguration.ShapeRootDirectory, shapeProperties!.Directory, shapeProperties.FileName)}";
                    string shapePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), shapeDirectory));

                    return new CachedShapefileDataReader(cache, key, shapePath);
                })
                .AddScoped<BoroughBoundariesService>()
                .AddScoped<CommunityDistrictsService>()
                .AddScoped<DSNYDistrictsService>()
                .AddScoped<HistoricDistrictService>()
                .AddScoped<IndividualLandmarkSiteService>()
                .AddScoped<NeighborhoodsService<NeighborhoodShape>>()
                .AddScoped<NeighborhoodTabulationAreasService>()
                .AddScoped<NypdPolicePrecinctService>()
                .AddScoped<NypdSectorsService>()
                .AddScoped<NychaDevelopmentService<NychaDevelopmentShape>>()
                .AddScoped<ParkService>()
                .AddScoped<ScenicLandmarkService>()
                .AddScoped<SubwayService>()
                .AddScoped<ZipCodeService>()
                .AddOptions()
                .Configure<ApplicationOptions>(Configuration)
                .AddSingleton(Configuration)
                .BuildServiceProvider();

            BoroughBoundariesService = serviceProvider.GetRequiredService<BoroughBoundariesService>();
            CommunityDistrictService = serviceProvider.GetRequiredService<CommunityDistrictsService>();
            DSNYDistrictsService = serviceProvider.GetRequiredService<DSNYDistrictsService>();
            HistoricDistrictService = serviceProvider.GetRequiredService<HistoricDistrictService>();
            IndividualLandmarkSiteService = serviceProvider.GetRequiredService<IndividualLandmarkSiteService>();
            NeighborhoodService = serviceProvider.GetRequiredService<NeighborhoodsService<NeighborhoodShape>>();
            NeighborhoodTabulationAreasService = serviceProvider.GetRequiredService<NeighborhoodTabulationAreasService>();


            //services.AddScoped            <NypdPolicePrecinctService>();
            //services.AddScoped            <NypdSectorsService<NypdSectorShape>>();
            //services.AddScoped            <NychaDevelopmentService<NychaDevelopmentShape>>();

            ParkService = serviceProvider.GetRequiredService<ParkService>();
            ScenicLandmarkService = serviceProvider.GetRequiredService<ScenicLandmarkService>();
            SubwayService = serviceProvider.GetRequiredService<SubwayService>();
            ZipCodeService = serviceProvider.GetRequiredService<ZipCodeService>();
        }

        public IShapeService<BoroughBoundaryShape> BoroughBoundariesService { get; set; }

        public IShapeService<CommunityDistrictShape> CommunityDistrictService { get; set; }

        public IShapeService<DSNYDistrictsShape> DSNYDistrictsService { get; set; }

        public IShapeService<HistoricDistrictShape> HistoricDistrictService { get; set; }

        public IShapeService<IndividualLandmarkSiteShape> IndividualLandmarkSiteService { get; set; }

        public IShapeService<NeighborhoodShape> NeighborhoodService { get; set; }

        public IShapeService<NeighborhoodTabulationAreaShape> NeighborhoodTabulationAreasService { get; set; }



        //services.AddScoped<NypdPolicePrecinctService>();
        //services.AddScoped<NypdSectorsService<NypdSectorShape>>();
        //services.AddScoped<NychaDevelopmentService<NychaDevelopmentShape>>();

        public IShapeService<ParkShape> ParkService { get; set; }

        public IShapeService<ScenicLandmarkShape> ScenicLandmarkService { get; set; }

        public IShapeService<SubwayShape> SubwayService { get; set; }

        public IShapeService<ZipCodeShape> ZipCodeService { get; set; }

        public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json", optional: true)
            .Build();
    }
}
