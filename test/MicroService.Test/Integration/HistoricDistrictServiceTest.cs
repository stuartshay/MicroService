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
    public class HistoricDistrictServiceTest : IClassFixture<ShapeServiceFixture>, IShapeTest
    {
        public IShapeService<HistoricDistrictShape> _service;

        private readonly ITestOutputHelper _testOutputHelper;

        public HistoricDistrictServiceTest(ShapeServiceFixture fixture, ITestOutputHelper output)
        {
            _service = fixture.HistoricDistrictService;
            _testOutputHelper = output;
        }

        [Fact(DisplayName = "Get Shape File Properties")]
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

        [InlineData(1006187, 732036)]
        [Theory(DisplayName = "Get Geospatial Point Lookup Not Found")]
        public void Get_Geospatial_Point_Lookup_Not_Found(double x, double y)
        {
            var sut = _service.GetFeatureLookup(x, y, Datum.Nad83);

            Assert.Null(sut);
        }

        [InlineData(1005244.0510830927, 241013.96112274204, "Grand Concourse Historic District", "LP-02403")]
        [Theory(DisplayName = "Get Geospatial Point Lookup")]
        public void Get_Geospatial_Point_Lookup(double x, double y, string expected, object expected2)
        {
            var sut = _service.GetFeatureLookup(x, y, Datum.Nad83);

            Assert.NotNull(sut);
            Assert.Equal(expected, sut.AreaName);
            Assert.Equal(expected2, sut.LPNumber);
        }

        [InlineData(40.692489, -73.994019, "Brooklyn Heights Historic District", "LP-00099")]
        [Theory(DisplayName = "Get Geospatial Point Lookup")]
        public void Get_Geospatial_Point_Lookup_Wgs84(double latitude, double longitude, string expected, object expected2)
        {
            var sut = _service.GetFeatureLookup(longitude, latitude, Datum.Wgs84);

            Assert.NotNull(sut);
            Assert.Equal(expected, sut.AreaName);
            Assert.Equal(expected2, sut.LPNumber);
        }

        [InlineData("LP-00099", "BK", "Brooklyn Heights Historic District")]
        [InlineData("LP-00224", "MN", "Charlton-King-Vandam Historic District")]
        [InlineData("LP-02403", "BX", "Grand Concourse Historic District")]
        [Theory(DisplayName = "Get Feature Attribute Lookup")]
        public void Get_Feature_Attribute_Lookup(object value1, object value2, string expected)
        {
            // Arrange
            var attributes = new List<KeyValuePair<string, object>>
            {
                new("LPNumber", value1),
                new("BoroName", value2),
            };

            // Act
            var sut = _service.GetFeatureLookup(attributes);
            var value = sut?.FirstOrDefault();

            // Assert
            Assert.NotNull(sut);
            Assert.Equal(expected, value?.AreaName);
        }

        [InlineData("LP-00099", "BK", "Brooklyn Heights Historic District")]
        [InlineData("LP-00224", "MN", "Charlton-King-Vandam Historic District")]
        [InlineData("LP-02403", "BX", "Grand Concourse Historic District")]
        [Theory(DisplayName = "GetFeatureCollection returns expected feature collection")]
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

            Assert.Equal(expected, result.Attributes["AreaName"]);
        }

        [Fact]
        public void Get_Feature_List()
        {
            var sut = _service.GetFeatureList();

            Assert.NotNull(sut);
        }
    }
}