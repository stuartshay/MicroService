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
    public class DsnyDistrictsServiceTest : IClassFixture<ShapeServiceFixture>, IShapeTest
    {
        public IShapeService<DsnyDistrictsShape> _service;

        private readonly ITestOutputHelper _testOutputHelper;

        public DsnyDistrictsServiceTest(ShapeServiceFixture fixture, ITestOutputHelper output)
        {
            _service = fixture.DSNYDistrictsService;
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
        [Trait("Category", "Integration")]
        public void Get_Feature_Collection()
        {
            var sut = _service.GetFeatures();
            Assert.NotNull(sut);
            Assert.IsType<List<Feature>>(sut);
        }

        [InlineData(1006187, 232036, "Bronx", 201)]
        [InlineData(1000443, 0239270, "Manhattan", 110)]
        [Theory(DisplayName = "Get Geospatial Point Lookup")]
        public void Get_Geospatial_Point_Lookup(double x, double y, string expected, object expected2)
        {
            var sut = _service.GetFeatureLookup(x, y, Datum.Nad83);

            Assert.NotNull(sut);
            Assert.Equal(expected, sut.OperationZoneName);
            Assert.Equal(expected2, sut.DistrictCode);
        }

        [InlineData(40.860570, -73.898840, "BX", 205)]
        [InlineData(40.746340, -73.982600, "MN", 105)]
        [InlineData(40.7469908, -73.946450, "QW", 402)]
        [Theory(DisplayName = "Get Geospatial Point Lookup - WGS84")]
        public void Get_Geospatial_Point_Lookup_Wgs84(double latitude, double longitude, string expected, object expected2)
        {
            var sut = _service.GetFeatureLookup(longitude, latitude, Datum.Wgs84);

            Assert.NotNull(sut);
            Assert.Equal(expected, sut.OperationZone);
            Assert.Equal(expected2, sut.DistrictCode);
        }

        [InlineData(1006187, 732036)]
        [Theory(DisplayName = "Get Geospatial Point Lookup Not Found")]
        [Trait("Category", "Integration")]
        public void Get_Geospatial_Point_Lookup_Not_Found(double x, double y)
        {
            var sut = _service.GetFeatureLookup(x, y, Datum.Nad83);

            Assert.Null(sut);
        }


        [InlineData("MN01", "72", "Manhattan")]
        [InlineData("SI02", "107", "Staten Island")]
        [Theory(DisplayName = "Get Feature Attribute Lookup")]
        public void Get_Feature_Attribute_Lookup(object value1, object value2, string expected)
        {
            var attributes = new List<KeyValuePair<string, object>>
            {
                new("District", value1),
                new("Fid", value2),
            };

            var sut = _service.GetFeatureLookup(attributes);
            var result = sut.FirstOrDefault();

            Assert.NotNull(sut);
            Assert.Equal(expected, result?.OperationZoneName);
        }

        [InlineData("MN01", "72", "Manhattan")]
        [InlineData("SI02", "107", "Staten Island")]
        [Theory(DisplayName = "GetFeatureCollection returns expected feature collection")]
        public void GetFeatureCollection_ValidInput_ReturnsExpectedFeature(string value1, string value2, string expected)
        {
            // Arrange
            var attributes = new List<KeyValuePair<string, object>>
            {
                new("District", value1),
                new("Fid", value2),
            };

            // Act
            var sut = _service.GetFeatureCollection(attributes);
            var result = sut.Single();

            // Assert
            Assert.NotNull(sut);
            Assert.IsType<FeatureCollection>(sut);
            Assert.NotNull(result);

            Assert.Equal(expected, result.Attributes["OperationZoneName"]);
        }

        [Fact]
        public void Get_Feature_List()
        {
            var sut = _service.GetFeatureList();

            Assert.NotNull(sut);
        }
    }
}
