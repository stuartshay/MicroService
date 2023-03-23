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
    public class CommunityDistrictServiceTest : IClassFixture<ShapeServiceFixture>, IShapeTest
    {
        public IShapeService<CommunityDistrictShape> _service;

        private readonly ITestOutputHelper _testOutputHelper;

        public CommunityDistrictServiceTest(ShapeServiceFixture fixture, ITestOutputHelper output)
        {
            _service = fixture.CommunityDistrictService;
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
            Assert.Equal(expected, sut.BoroName);
            Assert.Equal(expected2, sut.BoroCd);
        }

        [InlineData(-73.898840, 40.860570, "Bronx", 205)]
        [InlineData(-73.982600, 40.746340, "Manhattan", 105)]
        [InlineData(-73.946450, 40.7469908, "Queens", 402)]
        [Theory(DisplayName = "Get Geospatial Point Lookup - WGS84")]
        public void Get_Geospatial_Point_Lookup_Wgs84(double x, double y, string expected, object expected2)
        {
            var sut = _service.GetFeatureLookup(x, y, Datum.Wgs84);

            Assert.NotNull(sut);
            Assert.Equal(expected, sut.BoroName);
            Assert.Equal(expected2, sut.BoroCd);
        }

        [InlineData(312, "", "312")]
        [Theory(Skip = "TODO-FIX", DisplayName = "Get Feature Attribute Lookup")]
        public void Get_Feature_Attribute_Lookup(object value1, object value2, string expected)
        {
            var attributes = new List<KeyValuePair<string, object>>
            {
                new("BoroCd", value1),
            };

            var sut = _service.GetFeatureLookup(attributes);
            var result = sut.FirstOrDefault();

            Assert.NotNull(sut);
            Assert.Equal(int.Parse(expected), result?.Cd);
        }


        [Theory(DisplayName = "GetFeatureCollection returns expected feature collection")]
        [InlineData("312", "Manhattan", "Manhattan")]
        public void GetFeatureCollection_ValidInput_ReturnsExpectedFeature(string value1, string value2, string expected)
        {
            // Arrange
            var attributes = new List<KeyValuePair<string, object>>
            {
                new("BoroCd", value1),
            };

            // Act
            var sut = _service.GetFeatureCollection(attributes);
            //var result = sut.Single();

            //// Assert
            //Assert.NotNull(sut);
            //Assert.IsType<FeatureCollection>(sut);
            //Assert.NotNull(result);;
            //Assert.Equal(int.Parse(value1), (int)result.Attributes["BoroCode"]);
        }


        [InlineData(1006187, 732036)]
        [Theory(DisplayName = "Get Geospatial Point Lookup Not Found")]
        public void Get_Geospatial_Point_Lookup_Not_Found(double x, double y)
        {
            var sut = _service.GetFeatureLookup(x, y, Datum.Nad83);

            Assert.Null(sut);
        }


        [Fact]
        public void Get_Feature_List()
        {
            var sut = _service.GetFeatureList();

            Assert.NotNull(sut);
        }
    }
}
