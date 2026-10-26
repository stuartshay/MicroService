using MicroService.Service.Interfaces;
using MicroService.Service.Models;
using MicroService.Service.Models.Enum.Attributes;
using MicroService.Test.Fixture;
using NetTopologySuite.Features;
using Xunit;

namespace MicroService.Test.Integration
{
    [Trait("Category", "Integration")]
    public class NationalRegisterHistoricPlacesTest : IClassFixture<ShapeServiceFixture>
    {
        public IShapeService<NationalRegisterHistoricPlacesShape> _service;

        private readonly ITestOutputHelper _testOutputHelper;

        public NationalRegisterHistoricPlacesTest(ShapeServiceFixture fixture, ITestOutputHelper output)
        {
            _service = fixture.NationalRegisterHistoricPlacesService;
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

            // Display summary information about the Dbase file
            _testOutputHelper.WriteLine("Dbase info");
            _testOutputHelper.WriteLine($"{sut.Fields.Length} Columns, {sut.NumRecords} Records");

            for (int i = 0; i < sut.NumFields; i++)
            {
                var fldDescriptor = sut.Fields[i];
                _testOutputHelper.WriteLine($"   {fldDescriptor.Name} {fldDescriptor.DbaseType}");
            }
        }

        [Fact(DisplayName = "Get Feature Collection")]
        public void Get_Feature_Collection()
        {
            // Arrange & Act 
            var sut = _service.GetFeatures();

            // Assert
            Assert.NotNull(sut);
            Assert.IsType<List<Feature>>(sut);

            var features = sut.Take(5);
            Assert.NotNull(features);
        }

        [InlineData(-73.9687673796, 40.6108841936, "Kings", "90NR01303")]
        [Theory(DisplayName = "Get Geospatial Point Lookup - WGS84")]
        [Trait("Category", "Integration")]
        public void Get_Geospatial_Point_Lookup_Wgs84(double longitude, double latitude, string expectedCounty, string expectedSphinxNumber)
        {
            var sut = _service.GetFeatureLookup(longitude, latitude, Datum.Wgs84);

            Assert.NotNull(sut);
            Assert.Equal(expectedCounty, sut.County);
            Assert.Equal(expectedSphinxNumber, sut.SphinxNumber);
            Assert.Equal("Ocean Parkway", sut.ResourceName);
        }

        [InlineData("90NR01303", "Kings", "Ocean Parkway")]
        [Theory(DisplayName = "Get Feature Attribute Lookup")]
        public void Get_Feature_Attribute_Lookup(string sphinxNumber, string county, string expectedResourceName)
        {
            // Arrange
            var attributes = new List<KeyValuePair<string, object>>
            {
                new("SphinxNumber", sphinxNumber),
                new("County", county),
            };

            // Act
            var sut = _service.GetFeatureLookup(attributes);
            var value = sut?.FirstOrDefault();

            // Assert
            Assert.NotNull(sut);
            Assert.Equal(expectedResourceName, value?.ResourceName);
            Assert.Equal(county, value?.County);
            Assert.Equal(sphinxNumber, value?.SphinxNumber);
            Assert.NotNull(value!.Geometry);
        }

        [InlineData("90NR01303", "Kings", "Ocean Parkway")]
        [Theory(DisplayName = "GetFeatureCollection returns expected feature collection")]
        public void GetFeatureCollection_ValidInput_ReturnsExpectedFeature(string sphinxNumber, string county, string expectedResourceName)
        {
            // Arrange
            var attributes = new List<KeyValuePair<string, object>>
            {
                new("SphinxNumber", sphinxNumber),
                new("County", county),
            };

            // Act
            var sut = _service.GetFeatureCollection(attributes);
            var typedSut = Assert.IsType<FeatureCollection>(sut);
            var result = typedSut.Single();

            // Assert
            Assert.NotNull(sut);
            Assert.IsType<FeatureCollection>(sut);
            Assert.NotNull(result);

            Assert.Equal(expectedResourceName, result.Attributes["ResourceName"]);
            Assert.Equal(county, result.Attributes["County"]);
            Assert.Equal(sphinxNumber, result.Attributes["SphinxNumber"]);
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
            var sut = _service.GetFeatureList().ToList();

            Assert.NotNull(sut);
            Assert.NotEmpty(sut);
        }
    }
}
