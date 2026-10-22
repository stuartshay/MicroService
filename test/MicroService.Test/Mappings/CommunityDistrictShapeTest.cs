using MicroService.Service.Mappings;
using MicroService.Service.Models;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using Xunit;

namespace MicroService.Test.Mappings
{
    public class CommunityDistrictShapeTest
    {
        [InlineData(401, 401, 1, 4, "QN", "Queens")]
        [InlineData("312", 312, 12, 3, "BK", "Brooklyn")]
        [Theory(DisplayName = "Validate Mappings")]
        public void TestShapeMapping(object value1, int expected1, int expectedCd, int expectedBoroCode, string expectedBorough, string expectedBoroName)
        {
            // Arrange: the profile maps from a shapefile Feature, keyed by the .dbf column names
            var feature = CreateFeature(value1);

            // Act
            var shape = CommunityDistrictShapeProfile.Map(feature);

            // Assert
            Assert.IsType<CommunityDistrictShape>(shape);
            Assert.Equal(expected1, shape.BoroCd);
            Assert.Equal(expectedCd, shape.Cd);
            Assert.Equal(expectedBoroCode, shape.BoroCode);
            Assert.Equal(expectedBorough, shape.Borough);
            Assert.Equal(expectedBoroName, shape.BoroName);
        }

        [InlineData("Manhattan")]
        [Theory(DisplayName = "Validate Mappings - Invalid")]
        public void TestShapeMapping_Invalid(string value1)
        {
            // Arrange
            var feature = CreateFeature(value1);

            // Act & Assert
            Assert.Throws<FormatException>(() => CommunityDistrictShapeProfile.Map(feature));
        }

        private static Feature CreateFeature(object boroCd) =>
            new(new Point(0, 0), new AttributesTable(new Dictionary<string, object>
            {
                { "BoroCD", boroCd },
                { "Shape_Area", 1.5 },
                { "Shape_Leng", 2.5 },
            }));
    }
}
