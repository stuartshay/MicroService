using MicroService.Service.Configuration;
using MicroService.Service.Interfaces;
using MicroService.Service.Models;
using MicroService.Service.Services;
using MicroService.Service.Services.Base;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MicroService.Test.Fixture
{
    public class ShapeServiceFixture : FixtureConfig
    {
        public ShapeServiceFixture()
        {
            var services = new ServiceCollection()
                .AddMemoryCache()
                .AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information))
                .AddScoped<ShapefileDataReaderResolver>(serviceProvider => key =>
                    ShapeRegistry.CreateDataReader(
                        serviceProvider.GetRequiredService<IMemoryCache>(),
                        serviceProvider.GetRequiredService<IOptions<ApplicationOptions>>(),
                        key))
                .AddScoped<ShapeServiceResolver>(serviceProvider => key =>
                    ShapeRegistry.ResolveService(serviceProvider, key))
                .AddOptions()
                .Configure<ApplicationOptions>(Configuration)
                .AddSingleton(Configuration);

            foreach (var registration in ShapeRegistry.All)
            {
                services.AddScoped(registration.ServiceType);
            }

            ServiceProvider = services.BuildServiceProvider();
            ShapeServiceResolver = ServiceProvider.GetRequiredService<ShapeServiceResolver>();

            BoroughBoundariesService = ServiceProvider.GetRequiredService<BoroughBoundariesService>();
            CommunityDistrictService = ServiceProvider.GetRequiredService<CommunityDistrictsService>();
            DSNYDistrictsService = ServiceProvider.GetRequiredService<DsnyDistrictsService>();
            HistoricDistrictService = ServiceProvider.GetRequiredService<HistoricDistrictService>();
            IndividualLandmarkSiteService = ServiceProvider.GetRequiredService<IndividualLandmarkSiteService>();
            IndividualLandmarkHistoricDistrictsService = ServiceProvider.GetRequiredService<IndividualLandmarkHistoricDistrictsService>();
            NationalRegisterHistoricPlacesService = ServiceProvider.GetRequiredService<NationalRegisterHistoricPlacesService>();
            NeighborhoodService = ServiceProvider.GetRequiredService<NeighborhoodsService>();
            NeighborhoodTabulationAreasService = ServiceProvider.GetRequiredService<NeighborhoodTabulationAreasService>();
            NypdPolicePrecinctService = ServiceProvider.GetRequiredService<NypdPolicePrecinctService>();
            NypdSectorsService = ServiceProvider.GetRequiredService<NypdSectorsService>();
            NychaDevelopmentService = ServiceProvider.GetRequiredService<NychaDevelopmentService>();
            ParkService = ServiceProvider.GetRequiredService<ParkService>();
            ScenicLandmarkService = ServiceProvider.GetRequiredService<ScenicLandmarkService>();
            SubwayService = ServiceProvider.GetRequiredService<SubwayService>();
            SubwayPointService = ServiceProvider.GetRequiredService<SubwayService>();
            ZipCodeService = ServiceProvider.GetRequiredService<ZipCodeService>();
        }

        public ServiceProvider ServiceProvider { get; }

        public ShapeServiceResolver ShapeServiceResolver { get; }

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
