using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using Xunit;

namespace MicroService.Test.Unit
{
    public class ShapeFileTest
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public ShapeFileTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [InlineData("NYPD_Sectors/NYPD_Sectors")]
        [Theory(DisplayName = "Shape OutputFeatures")]
        public void Shape_OutputFeatures(string shapeFilePath)
        {
            string shapeDirectory = $"../../../../../files/{shapeFilePath}";
            string shapePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), shapeDirectory));

            var (shape, database) = MicroService.Service.Helpers.ShapefileSource.ReadMetadata(shapePath);
            Assert.Equal("Polygon", shape.ShapeType);
            Assert.True(database.NumFields > 0);
            var features = MicroService.Service.Helpers.ShapefileSource.ReadFeatures(shapePath);

            Assert.NotEmpty(features);
            Assert.All(features, feature =>
            {
                Assert.NotNull(feature.Geometry);
                Assert.NotNull(feature.Attributes);
            });

            var containingFeature = Assert.Single(features, feature =>
                feature.Geometry.Contains(new Point(1032999, 217570)));
            Assert.Contains("pct", containingFeature.Attributes.GetNames());
            var precinct = containingFeature.Attributes["pct"];
            Assert.NotNull(precinct);
            Assert.NotEqual(DBNull.Value, precinct);
        }

        [Fact]
        public void Test_WKTReader()
        {
            WktReaderFunctions.GeometryContainsPoint();
            Assert.True(true);
        }

    }
}
