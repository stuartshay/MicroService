using MicroService.Service.Models.Enum.Attributes;
using MicroService.Service.Services;
using MicroService.Test.Fixture;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MicroService.Test.Integration
{
    /// <summary>
    /// The map layer shapes (docs/map-layers-plan.md) and the refreshed landmark shapes, read from files/.
    /// </summary>
    [Trait("Category", "Integration")]
    public class MapLayerShapeServiceTest : IClassFixture<ShapeServiceFixture>
    {
        private readonly ShapeServiceFixture _fixture;

        public MapLayerShapeServiceTest(ShapeServiceFixture fixture)
        {
            _fixture = fixture;
        }

        private T Service<T>() where T : notnull => _fixture.ServiceProvider.GetRequiredService<T>();

        [Fact(DisplayName = "Map layers - each new shape loads every feature from its export")]
        public void NewShapes_LoadEveryFeature()
        {
            Assert.Equal(392, Service<PrivatelyOwnedPublicSpacesService>().GetFeatures().Count);
            Assert.Equal(2755, Service<GreenstreetsService>().GetFeatures().Count);
            Assert.Equal(216, Service<LibrariesService>().GetFeatures().Count);
            Assert.Equal(230, Service<PostOfficesService>().GetFeatures().Count);
        }

        [Fact(DisplayName = "Map layers - a point lookup finds the nearest library, post office and POPS, in either datum")]
        public void PointShapes_FindTheNearest()
        {
            var library = Service<LibrariesService>().GetFeatureLookup(-73.9990, 40.7345, Datum.Wgs84);
            Assert.Equal("Jefferson Market", library?.Name);
            Assert.Equal("NYPL", library?.System);
            Assert.Equal("425 Avenue of the Americas", library?.Address);

            var postOffice = Service<PostOfficesService>().GetFeatureLookup(-73.9755, 40.7533, Datum.Wgs84);
            Assert.Equal("Grand Central Station", postOffice?.Name);
            Assert.Equal("450 LEXINGTON AVENUE", postOffice?.Address);

            // 1350 Sixth Avenue in NAD83 State Plane feet
            var pops = Service<PrivatelyOwnedPublicSpacesService>().GetFeatureLookup(990406, 217104, Datum.Nad83);
            Assert.Equal("M050022", pops?.PopsNumber);
            Assert.Equal("Plaza", pops?.PublicSpaceType);
        }

        [Fact(DisplayName = "Map layers - the nearest point is measured in metres, not degrees")]
        public void PointShapes_MeasureInMetres()
        {
            // 802 m to the Schwarzman Building, 995 m to Columbus; in raw degrees Columbus looks nearer
            var library = Service<LibrariesService>().GetFeatureLookup(-73.991, 40.756, Datum.Wgs84);

            Assert.Equal("Stephen A. Schwarzman Building", library?.Name);
        }

        [Fact(DisplayName = "Map layers - a Greenstreet is found by a point inside it, and by its property number")]
        public void Greenstreets_FoundByPointAndAttribute()
        {
            var service = Service<GreenstreetsService>();
            var inside = service.GetFeatures().First(f => (string)f.Attributes["gispropnum"] == "QZ757").Geometry.InteriorPoint;

            var found = service.GetFeatureLookup(inside.X, inside.Y, Datum.Wgs84);

            Assert.Equal("QZ757", found?.PropertyNumber);
            Assert.Equal("Rockaway Beach Blvd bet. B 129 St & B 130 St", found?.Location);
            Assert.Single(service.GetFeatureLookup([new("gispropnum", "QZ757")]));
        }

        [Fact(DisplayName = "Map layers - each shape's attribute lookup GeoJSON carries its features and color")]
        public void FeatureCollections_CarryTheirColor()
        {
            var library = Assert.Single(Service<LibrariesService>().GetFeatureCollection([new("name", "Jefferson Market")]));
            Assert.Equal("blue", library.Attributes["ShapeColor"]);
            Assert.Equal("Jefferson Market", library.Attributes["Name"]);

            var postOffice = Assert.Single(Service<PostOfficesService>().GetFeatureCollection([new("name", "Grand Central Station")]));
            Assert.Equal("red", postOffice.Attributes["ShapeColor"]);
            Assert.Equal("450 LEXINGTON AVENUE", postOffice.Attributes["Address"]);

            var pops = Assert.Single(Service<PrivatelyOwnedPublicSpacesService>().GetFeatureCollection([new("pops_numbe", "M050022")]));
            Assert.Equal("yellow", pops.Attributes["ShapeColor"]);
            Assert.Equal("Plaza", pops.Attributes["PublicSpaceType"]);

            var greenstreet = Assert.Single(Service<GreenstreetsService>().GetFeatureCollection([new("gispropnum", "QZ757")]));
            Assert.Equal("green", greenstreet.Attributes["ShapeColor"]);
            Assert.Equal("Active", greenstreet.Attributes["Status"]);
        }

        [Theory(DisplayName = "Map layers - the districts the city's file dropped are kept, so their LP numbers still find an outline")]
        [InlineData("LP-02102", "Murray Hill Historic District")]
        [InlineData("LP-00861", "Carnegie Hill Historic District")]
        [InlineData("LP-02651", "Dorrance Brooks Square Historic District")]
        public void HistoricDistricts_KeepTheDropped(string lpNumber, string name)
        {
            var feature = _fixture.HistoricDistrictService.GetFeatures().Single(f => (string)f.Attributes["lp_number"] == lpNumber);

            Assert.Equal(name, feature.Attributes["area_name"]);
            Assert.Equal("Yes", feature.Attributes["kept"]);
            Assert.True(feature.Geometry.Area > 0);
        }

        [Fact(DisplayName = "Map layers - the Scenic Landmarks the city's file dropped are kept; the refreshed ones are not marked")]
        public void ScenicLandmarks_KeepTheDropped()
        {
            var features = _fixture.ScenicLandmarkService.GetFeatures();

            Assert.Equal(11, features.Count);
            Assert.Equal(["LP-00871", "LP-00998", "LP-02002"],
                features.Where(f => (string)f.Attributes["kept"] == "Yes").Select(f => (string)f.Attributes["lp_number"]).Order());
            Assert.Equal("Yes", features.Single(f => (string)f.Attributes["lp_number"] == "LP-00879").Attributes["current_"]);
        }

        [Fact(DisplayName = "Map layers - Borough Boundaries is WGS84 and still answers NAD83 lookups")]
        public void BoroughBoundaries_AnswerBothDatums()
        {
            Assert.Equal("Manhattan", _fixture.BoroughBoundariesService.GetFeatureLookup(-73.9712, 40.7831, Datum.Wgs84)?.BoroName);
            Assert.Equal("Brooklyn", _fixture.BoroughBoundariesService.GetFeatureLookup(985000, 190000, Datum.Nad83)?.BoroName);
            Assert.InRange(_fixture.BoroughBoundariesService.GetFeatures().First().Geometry.Coordinate.X, -74.3, -73.6);
        }
    }
}
