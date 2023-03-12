using MicroService.Service.Interfaces;
using MicroService.Service.Models;
using MicroService.Test.Fixture;
using MicroService.Test.Integration.Interfaces;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Xunit;
using Xunit.Abstractions;

namespace MicroService.Test.Integration
{
    public class SubwayServiceTest : IClassFixture<ShapeServiceFixture>, IShapeTest
    {
        public IShapeService<SubwayShape> _service;
        public IPointService<SubwayShape> _pointService;

        private readonly ITestOutputHelper _testOutputHelper;

        public SubwayServiceTest(ShapeServiceFixture fixture, ITestOutputHelper output)
        {
            _service = fixture.SubwayService;
            _pointService = fixture.SubwayPointService;
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

            //Display summary information about the Shape file
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

        [InlineData(1006187, 232036, "Bronx", 0)]
        [InlineData(1000443, 0239270, "Manhattan", 0)]
        [InlineData(1021192.9426658918, 212550.01741990919, "Queens", 0)]
        [Theory(DisplayName = "Get Feature Point Lookup")]
        public void Get_Feature_Point_Lookup(double x, double y, string expected, object lookupExpectedl)
        {
            var sut = _service.GetFeatureLookup(x, y);

            Assert.NotNull(sut);
            //Assert.Equal(expected, sut.BoroName);
        }

        [InlineData(27006187, 932036)]
        [Theory(Skip = "TODO: FIX", DisplayName = "Get Feature Point Lookup Not Found")]
        public void Get_Feature_Point_Lookup_Not_Found(double x, double y)
        {
            var sut = _service.GetFeatureLookup(x, y);

            Assert.Null(sut);
        }

        [InlineData("Junction Boulevard & Roosevelt Avenue at NE corner", "", "1789")]
        [Theory(DisplayName = "Get Feature Attribute Lookup")]
        public void Get_Feature_Attribute_Lookup(object value1, object value2, string expected)
        {
            var attributes = new List<KeyValuePair<string, object>>
            {
                 new("Name", value1),
            };

            var sut = _service.GetFeatureLookup(attributes);
            var result = sut.FirstOrDefault();

            Assert.NotNull(sut);
            Assert.Equal(value1, result?.Name);
            Assert.Equal(Double.Parse(expected), result?.ObjectId);
        }


        [InlineData("Junction Boulevard & Roosevelt Avenue at NE corner", "7", "1789")]
        [Theory(DisplayName = "GetFeatureCollection returns expected feature collection")]
        public void GetFeatureCollection_ValidInput_ReturnsExpectedFeature(string value1, string value2, string expected)
        {
            // Arrange
            var attributes = new List<KeyValuePair<string, object>>
            {
                new("Name", value1),
                new("Line", value2)
            };

            // Act
            var sut = _service.GetFeatureCollection(attributes);
            var result = sut.Single();

            // Assert
            Assert.NotNull(sut);
            Assert.IsType<FeatureCollection>(sut);
            Assert.NotNull(result);

            Assert.Equal(double.Parse(expected), result.Attributes["ObjectId"]);
        }

        [Fact(Skip = "TODO FIX")]
        public void FindPointsByRadius_ReturnsPointsWithinRadius()
        {
            // Arrange
            var centerPoint = new Point(40.7677792, -73.969123);
            var radius = 0.05; // meters

            // Act
            var pointsWithinRadius = _pointService.FindPointsByRadius(centerPoint, radius);

            // Assert
            Assert.NotEmpty(pointsWithinRadius);
        }


        [Fact]
        public void Get_Feature_List()
        {
            var sut = _service.GetFeatureList();

            Assert.NotNull(sut);
        }

    }
}