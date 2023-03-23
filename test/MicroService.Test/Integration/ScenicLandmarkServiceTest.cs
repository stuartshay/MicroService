using MicroService.Service.Interfaces;
using MicroService.Service.Models;
using MicroService.Service.Models.Enum.Attributes;
using MicroService.Test.Fixture;
using MicroService.Test.Integration.Interfaces;
using NetTopologySuite.Features;
using NetTopologySuite.IO;
using Xunit;
using Xunit.Abstractions;

namespace MicroService.Test.Integration
{
    public class ScenicLandmarkServiceTest : IClassFixture<ShapeServiceFixture>, IShapeTest
    {
        public IShapeService<ScenicLandmarkShape> _service;

        private readonly ITestOutputHelper _testOutputHelper;

        public ScenicLandmarkServiceTest(ShapeServiceFixture fixture, ITestOutputHelper output)
        {
            _service = fixture.ScenicLandmarkService;
            _testOutputHelper = output;
        }

        [Fact(DisplayName = "Get Shape File Properties")]
        [Trait("Category", "Integration")]
        public void Get_Shape_Properties()
        {
            var sut = _service.GetShapeProperties();
            Assert.NotNull(sut);

            _testOutputHelper.WriteLine($"Shape type: {sut.ShapeType}");

            //Display the min and max bounds of the shapefile
            var bounds = sut.Bounds;
            _testOutputHelper.WriteLine($"Min bounds: ({bounds.MinX},{bounds.MinY})");
            _testOutputHelper.WriteLine($"Max bounds: ({bounds.MaxX},{bounds.MaxY})");
        }


        [Fact(DisplayName = "Get Shape File Database Properties")]
        [Trait("Category", "Integration")]
        public void Get_Shape_Database_Properties()
        {
            var sut = _service.GetShapeDatabaseProperties();
            Assert.NotNull(sut);

            //Display summary information about the Dbase file
            _testOutputHelper.WriteLine("Dbase info");
            _testOutputHelper.WriteLine($"{sut.Fields.Length} Columns, {sut.NumRecords} Records");

            for (int i = 0; i < sut.NumFields; i++)
            {
                DbaseFieldDescriptor fldDescriptor = sut.Fields[i];
                _testOutputHelper.WriteLine($"   {fldDescriptor.Name} {fldDescriptor.DbaseType}");
            }
        }

        [Fact(DisplayName = "Get Feature Collection")]
        public void Get_Feature_Collection()
        {
            var sut = _service.GetFeatures();
            Assert.NotNull(sut);
            Assert.IsType<List<Feature>>(sut);
        }

        [Fact(DisplayName = "Get Feature Collection")]
        [Trait("Category", "Integration")]
        public void Get_Borough_Feature_Collection()
        {
            var sut = _service.GetFeatures();
            Assert.NotNull(sut);
            Assert.IsType<List<Feature>>(sut);
        }


        [InlineData(991228.1942826601, 220507.29488507056, "Central Park", "MN")]
        [Theory(DisplayName = "Get Geospatial Point Lookup - NAD83")]
        [Trait("Category", "Integration")]
        public void Get_Geospatial_Point_Lookup(double x, double y, string expected, object expected2)
        {
            var sut = _service.GetFeatureLookup(x, y, Datum.Nad83);

            Assert.NotNull(sut);
            Assert.Equal(expected, sut.AreaName);
            Assert.Equal(expected2, sut.BoroName);
        }

        [InlineData(-73.965624, 40.78268, "Central Park", "MN")]
        [Theory(DisplayName = "Get Geospatial Point Lookup - WGS84")]
        public void Get_Geospatial_Point_Lookup_Wgs84(double x, double y, string expected, object expected2)
        {
            var sut = _service.GetFeatureLookup(x, y, Datum.Wgs84);

            Assert.NotNull(sut);
            Assert.Equal(expected, sut.AreaName);
            Assert.Equal(expected2, sut.BoroName);
        }

        [InlineData("LP-00879", "MN", "Bryant Park")]
        [Theory(DisplayName = "Get Feature Attribute Lookup")]
        public void Get_Feature_Attribute_Lookup(object value1, object value2, string expected)
        {
            var attributes = new List<KeyValuePair<string, object>>
            {
                new("LPNumber", value1),
                new("BoroName", value2),
            };

            var sut = _service.GetFeatureLookup(attributes);
            var result = sut.FirstOrDefault();

            Assert.NotNull(sut);
            Assert.Equal(expected, result?.AreaName);
        }


        [InlineData(1006187, 732036)]
        [Theory(DisplayName = "Get Geospatial Point Lookup Not Found")]
        [Trait("Category", "Integration")]
        public void Get_Geospatial_Point_Lookup_Not_Found(double x, double y)
        {
            var sut = _service.GetFeatureLookup(x, y, Datum.Nad83);

            Assert.Null(sut);
        }

        [Theory(DisplayName = "GetFeatureCollection returns expected feature collection")]
        [InlineData("LP-00879", "MN", "Bryant Park")]
        [InlineData("LP-00851", "MN", "Central Park")]
        [InlineData("LP-00857", "MN", "Verdi Square")]
        [InlineData("LP-00871", "BK", "Ocean Parkway")]
        public void GetFeatureCollection_ValidInput_ReturnsExpectedFeature(string value1, string value2, string expected)
        {
            // Arrange
            var attributes = new List<KeyValuePair<string, object>>
            {
                new("LPNumber", value1),
                new("BoroName", value2),
            };

            // Act
            var sut = _service.GetFeatureCollection(attributes);
            var result = sut.Single();

            // Assert
            Assert.NotNull(sut);
            Assert.IsType<FeatureCollection>(sut);
            Assert.NotNull(result);
            Assert.IsType<Feature>(result);
            Assert.Equal(expected, result.Attributes["AreaName"]);
            Assert.Equal(value1, result.Attributes["LPNumber"]);
            Assert.Equal(value2, result.Attributes["BoroName"]);
        }

        [Theory(DisplayName = "GetFeatureCollection returns empty feature collection")]
        [InlineData("LP-99999", "MN")] // Invalid LPNumber
        [InlineData("LP-00879", "XX")] // Invalid BoroName
        [InlineData("LP-99999", "XX")] // Invalid LPNumber and BoroName
        public void GetFeatureCollection_InvalidInput_ReturnsEmptyFeatureCollection(string value1, string value2)
        {
            // Arrange
            var attributes = new List<KeyValuePair<string, object>>
            {
                new("LPNumber", value1),
                new("BoroName", value2),
            };

            // Act
            var sut = _service.GetFeatureCollection(attributes);

            // Assert
            Assert.NotNull(sut);
            Assert.IsType<FeatureCollection>(sut);
            Assert.Empty(sut);
        }

        [Fact]
        public void Get_Feature_List()
        {
            var sut = _service.GetFeatureList();

            Assert.NotNull(sut);
        }

    }
}
