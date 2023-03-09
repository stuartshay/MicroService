using AutoMapper;
using AutoMapper.Internal;
using MicroService.Service.Configuration;
using MicroService.Service.Helpers;
using MicroService.Service.Interfaces;
using MicroService.Service.Mappings;
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
                .AddLogging()
                .AddScoped<ShapefileDataReaderResolver>(serviceProvider => key =>
                {
                    ShapeAttribute shapeProperties = null!;
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
                    else if (key == nameof(ShapeProperties.IndividualLandmarkHistoricDistricts))
                        shapeProperties = ShapeProperties.IndividualLandmarkHistoricDistricts.GetAttribute<ShapeAttribute>();
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

                    var shapeDirectory = $"{Path.Combine(options!.Value.ShapeConfiguration.ShapeRootDirectory, shapeProperties!.Directory, shapeProperties.FileName)}";
                    string shapePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), shapeDirectory));

                    return new CachedShapefileDataReader(cache, key, shapePath);
                })
                .AddScoped<BoroughBoundariesService>()
                .AddScoped<CommunityDistrictsService>()
                .AddScoped<DsnyDistrictsService>()
                .AddScoped<HistoricDistrictService>()
                .AddScoped<IndividualLandmarkSiteService>()
                .AddScoped<IndividualLandmarkHistoricDistrictsService>()
                .AddScoped<NationalRegisterHistoricPlacesService>()
                .AddScoped<NeighborhoodsService>()
                .AddScoped<NeighborhoodTabulationAreasService>()
                .AddScoped<NypdPolicePrecinctService>()
                .AddScoped<NypdSectorsService>()
                .AddScoped<NychaDevelopmentService>()
                .AddScoped<ParkService>()
                .AddScoped<ScenicLandmarkService>()
                .AddScoped<SubwayService>()
                .AddScoped<ZipCodeService>()
                .AddOptions()
                .Configure<ApplicationOptions>(Configuration)
                .AddSingleton(Configuration)

                .AddSingleton(_ => new MapperConfiguration(cfg =>
                {
                    cfg.Internal().AllowAdditiveTypeMapCreation = true;
                    cfg.AddMaps(typeof(FeatureToBoroughBoundaryShapeProfile).Assembly);
                }).CreateMapper())

                .BuildServiceProvider();

            BoroughBoundariesService = serviceProvider.GetRequiredService<BoroughBoundariesService>();
            CommunityDistrictService = serviceProvider.GetRequiredService<CommunityDistrictsService>();
            DSNYDistrictsService = serviceProvider.GetRequiredService<DsnyDistrictsService>();
            HistoricDistrictService = serviceProvider.GetRequiredService<HistoricDistrictService>();
            IndividualLandmarkSiteService = serviceProvider.GetRequiredService<IndividualLandmarkSiteService>();
            IndividualLandmarkHistoricDistrictsService = serviceProvider.GetRequiredService<IndividualLandmarkHistoricDistrictsService>();
            NationalRegisterHistoricPlacesService = serviceProvider.GetRequiredService<NationalRegisterHistoricPlacesService>();
            NeighborhoodService = serviceProvider.GetRequiredService<NeighborhoodsService>();
            NeighborhoodTabulationAreasService = serviceProvider.GetRequiredService<NeighborhoodTabulationAreasService>();
            NypdPolicePrecinctService = serviceProvider.GetRequiredService<NypdPolicePrecinctService>();
            NypdSectorsService = serviceProvider.GetRequiredService<NypdSectorsService>();
            NychaDevelopmentService = serviceProvider.GetRequiredService<NychaDevelopmentService>();
            ParkService = serviceProvider.GetRequiredService<ParkService>();
            ScenicLandmarkService = serviceProvider.GetRequiredService<ScenicLandmarkService>();
            SubwayService = serviceProvider.GetRequiredService<SubwayService>();
            SubwayPointService = serviceProvider.GetRequiredService<SubwayService>();
            ZipCodeService = serviceProvider.GetRequiredService<ZipCodeService>();
        }

        public IShapeService<BoroughBoundaryShape> BoroughBoundariesService { get; set; }

        public IShapeService<CommunityDistrictShape> CommunityDistrictService { get; set; }

        public IShapeService<DsnyDistrictsShape> DSNYDistrictsService { get; set; }

        public IShapeService<HistoricDistrictShape> HistoricDistrictService { get; set; }

        public IShapeService<IndividualLandmarkSiteShape> IndividualLandmarkSiteService { get; set; }

        public IShapeService<IndividualLandmarkHistoricDistrictsShape> IndividualLandmarkHistoricDistrictsService { get; set; }

        public IShapeService<NationalRegisterHistoricPlacesShape> NationalRegisterHistoricPlacesService { get; set; }

        public IShapeService<NeighborhoodShape> NeighborhoodService { get; set; }

        public IShapeService<NeighborhoodTabulationAreaShape> NeighborhoodTabulationAreasService { get; set; }

        public IShapeService<NypdPrecinctShape> NypdPolicePrecinctService { get; set; }

        public IShapeService<NypdSectorShape> NypdSectorsService { get; set; }

        public IShapeService<NychaDevelopmentShape> NychaDevelopmentService { get; set; }

        public IShapeService<ParkShape> ParkService { get; set; }

        public IShapeService<ScenicLandmarkShape> ScenicLandmarkService { get; set; }

        public IShapeService<SubwayShape> SubwayService { get; set; }

        public IPointService<SubwayShape> SubwayPointService { get; set; }

        public IShapeService<ZipCodeShape> ZipCodeService { get; set; }

        public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json", optional: true)
            .Build();
    }
}
