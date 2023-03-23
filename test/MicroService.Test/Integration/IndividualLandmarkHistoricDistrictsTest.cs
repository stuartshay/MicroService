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
    public class IndividualLandmarkHistoricDistrictsTest : IClassFixture<ShapeServiceFixture>, IShapeTest
    {
        public IShapeService<IndividualLandmarkHistoricDistrictsShape> _service;

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

        [InlineData(987615.655217366, 211953.9590513381, "Hotel Martinique", "MN")]
        [Theory(DisplayName = "Get Geospatial Point Lookup")]
        public void Get_Geospatial_Point_Lookup(double x, double y, string expected, object expected2)
        {
            var sut = _service.GetFeatureLookup(x, y, Datum.Nad83);

            Assert.NotNull(sut);
            Assert.Equal(expected, sut.AreaName);
            Assert.Equal(expected2, sut.BoroName);
        }

        [InlineData(-73.920786, 40.644343, "Pieter Claesen Wyckoff House", "BK")]
        [Theory(Skip = "TODO: FIX", DisplayName = "Get Geospatial Point Lookup")]
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
            var result = sut.Single();

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
            var result = sut.Single();

            // Assert
            Assert.NotNull(sut);
            Assert.IsType<FeatureCollection>(sut);
            Assert.NotNull(result);

            Assert.Equal(expected, result.Attributes["Bbl"]);
            Assert.Equal(expected2, result.Attributes["AreaName"]);
        }

        // TODO: Map Historic Districts to Landmark Preservation Commission
        [InlineData("LP-01559", "3079170009", "Gramercy Park Historic District Extension")]
        //[InlineData("LP-00099", "3079170009", "Brooklyn Heights Historic District")]
        [Theory(Skip = "TODO:FIX THIS", DisplayName = "GetFeatureCollection Mapped Historic District returns expected feature collection")]
        public void GetFeatureCollection_Mapped_HistoricDistrict_ValidInput_ReturnsExpectedFeature(string value1, string expected, string expected2)
        {
            // Arrange
            var attributes = new List<KeyValuePair<string, object>>
            {
                new("LPNumber", value1),
            };

            // Act
            var sut = _service.GetFeatureCollection(attributes);
            var result = sut.Single();

            // Assert
            Assert.NotNull(sut);
            Assert.IsType<FeatureCollection>(sut);
            Assert.NotNull(result);

            Assert.Equal(expected, result.Attributes["Bbl"]);
            Assert.Equal(expected2, result.Attributes["AreaName"]);
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
            var sut = _service.GetFeatureList();
            Assert.NotNull(sut);
        }

    }
}
