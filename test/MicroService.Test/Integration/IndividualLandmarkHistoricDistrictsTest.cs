using MicroService.Service.Interfaces;
using MicroService.Service.Models;
using MicroService.Service.Models.Enum.Attributes;
using MicroService.Test.Fixture;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Xunit;

namespace MicroService.Test.Integration
{
    [Trait("Category", "Integration")]
    public class IndividualLandmarkHistoricDistrictsTest : IClassFixture<ShapeServiceFixture>
    {
        public IShapeService<IndividualLandmarkHistoricDistrictsShape> _service;

        private static readonly Envelope NycWgs84Bounds = new(-74.3, -73.6, 40.4, 41.0);

        private readonly ITestOutputHelper _testOutputHelper;

        public IndividualLandmarkHistoricDistrictsTest(ShapeServiceFixture fixture, ITestOutputHelper output)
        {
            _service = fixture.IndividualLandmarkHistoricDistrictsService;
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
                DbaseFieldDescriptor fldDescriptor = sut.Fields[i];
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

        [Fact]
        public void GetFeatureLookup_ReturnsEmptySequenceForNullAttributes()
        {
            var result = _service.GetFeatureLookup(null);

            Assert.Empty(result);
        }

        // NAD83 / New York Long Island (ft) coordinates of -73.9600952918919, 40.63098618196636
        [InlineData(995326.2642537679, 169164.19580966546, "803 East 17th Street", "BK")]
        [Theory(DisplayName = "Get Geospatial Point Lookup")]
        public void Get_Geospatial_Point_Lookup(double x, double y, string expected, object expected2)
        {
            var actual = _service.GetFeatureLookup(x, y, Datum.Nad83);

            Assert.NotNull(actual);
            Assert.Equal(expected, actual.Address);
            Assert.Equal(expected2, actual.BoroName);
        }

        [InlineData(-73.920786, 40.644343, "Pieter Claesen Wyckoff House", "BK")]
        [Theory(DisplayName = "Get Geospatial Point Lookup - WGS84")]
        public void Get_Geospatial_Point_Lookup_Wgs84(double x, double y, string expected, object expected2)
        {
            var sut = _service.GetFeatureLookup(x, y, Datum.Wgs84);

            Assert.NotNull(sut);
            Assert.Equal(expected, sut.AreaName);
            Assert.Equal(expected2, sut.BoroName);
        }

        [InlineData("3066920018", "Free-standing House", "803 East 17th Street")]
        [Theory(DisplayName = "Get Feature Attribute Lookup")]
        public void Get_Feature_Attribute_Lookup(object value1, object value2, string expected)
        {
            // Arrange
            var attributes = new List<KeyValuePair<string, object>>
            {
                new("Bbl", value1),
                new("BuildType", value2),
            };

            // Act
            var sut = _service.GetFeatureLookup(attributes);
            var value = sut?.Single();

            // Assert
            Assert.NotNull(sut);
            Assert.Equal(expected, value?.Address);

            Assert.NotNull(value!.Geometry);

            Assert.NotNull(sut);
        }

        [InlineData("3066920018", "Free-standing House", "803 East 17th Street")]
        [Theory(DisplayName = "GetFeatureCollection returns expected feature collection")]
        public void GetFeatureCollection_ValidInput_ReturnsExpectedFeature(string value1, string value2, string expected)
        {
            // Arrange
            var attributes = new List<KeyValuePair<string, object>>
            {
                new("Bbl", value1),
                new("BuildType", value2),
            };

            // Act
            var sut = _service.GetFeatureCollection(attributes);
            var typedSut = Assert.IsType<FeatureCollection>(sut);
            var result = typedSut.Single();

            // Assert
            Assert.NotNull(sut);
            Assert.IsType<FeatureCollection>(sut);
            Assert.NotNull(result);

            Assert.Equal(expected, result.Attributes["Address"]);
        }

        [InlineData("LP-00001", "3079170009", "Pieter Claesen Wyckoff House")]
        [InlineData("LP-00010", "1005450040", "428 Lafayette Street Building (a part of La Grange Terrace)")]
        [Theory(DisplayName = "GetFeatureCollection Mapped Input returns expected feature collection")]
        public void GetFeatureCollection_Mapped_ValidInput_ReturnsExpectedFeature(string value1, string expected, string expected2)
        {
            // Arrange
            var attributes = new List<KeyValuePair<string, object>>
            {
                new("LPNumber", value1),
            };

            // Act
            var sut = _service.GetFeatureCollection(attributes);
            var typedSut = Assert.IsType<FeatureCollection>(sut);
            var result = typedSut.Single();

            // Assert
            Assert.NotNull(sut);
            Assert.IsType<FeatureCollection>(sut);
            Assert.NotNull(result);

            Assert.Equal(expected, result.Attributes["Bbl"]);
            Assert.Equal(expected2, result.Attributes["AreaName"]);
            Assert.True(NycWgs84Bounds.Contains(result.Geometry.EnvelopeInternal), "GeoJSON geometry must be WGS84 longitude/latitude.");
        }

        [InlineData(1006187, 732036)]
        [Theory(DisplayName = "Get Geospatial Point Lookup Not Found")]
        public void Get_Geospatial_Point_Lookup_Not_Found(double x, double y)
        {
            var sut = _service.GetFeatureLookup(x, y, Datum.Nad83);

            Assert.Null(sut);
        }

        [Fact(DisplayName = "Get Feature List")]
        public void Get_Feature_List()
        {
            var sut = _service.GetFeatureList().ToList();
            Assert.NotNull(sut);
            Assert.NotEmpty(sut);
        }

        [InlineData("LP-99999")]
        [Theory(DisplayName = "GetFeatureCollection Mapped Input returns null when no landmark site matches")]
        public void GetFeatureCollection_Mapped_UnknownLPNumber_ReturnsNull(string value1)
        {
            // Arrange
            var attributes = new List<KeyValuePair<string, object>>
            {
                new("LPNumber", value1),
            };

            // Act
            var sut = _service.GetFeatureCollection(attributes);

            // Assert
            Assert.Null(sut);
        }

    }
}
