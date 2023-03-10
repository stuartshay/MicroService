using MicroService.Service.Constants;
using MicroService.Service.Helpers;
using Xunit;

namespace MicroService.Test.Unit
{
    public class FunctionHelperUnitTest
    {
        [Fact(DisplayName = "Calculate_Percentile - Unit")]
        [Trait("Category", "Unit")]
        public void Can_Calculate_Percentile()
        {
            double result1 = 3.4d;
            var calculated1 = FunctionHelper.Percentile(new double[] { 1, 2, 3, 4 }, 0.8);

            double result2 = 8.05d;
            var calculated2 = FunctionHelper.Percentile(new double[] { 7, 8, 9, 20 }, 0.35);

            double result3 = 1.9d;
            var calculated3 = FunctionHelper.Percentile(new double[] { 1, 2, 3, 4 }, 0.3);

            // Assert
            Assert.Equal(result1, Math.Round(calculated1, DataConstants.PercentilePrecision));
            Assert.Equal(result2, Math.Round(calculated2, DataConstants.PercentilePrecision));
            Assert.Equal(result3, Math.Round(calculated3, DataConstants.PercentilePrecision));
        }

        [InlineData(1016637, 187747, 40.681939660888951, -73.8832294373166)]
        [Theory]
        [Trait("Category", "Unit")]
        public void Projection_Transform_ESRI102718_to_WGS84(double x, double y, double latitude, double longitude)
        {
            var result = GeoTransformationHelper.ConvertNad83ToWgs84(x, y);

            // Assert
            Assert.Equal(latitude, result!.Item2!.Value, 6);
            Assert.Equal(longitude, result.Item1!.Value, 6);
        }

        [InlineData(40.681939660888951, -73.8832294373166, 1016637, 187747.0295)]
        [Theory]
        [Trait("Category", "Unit")]
        public void Projection_Transform_WGS84_to_ESRI102718(double latitude, double longitude, double x, double y)
        {
            var result = GeoTransformationHelper.ConvertWgs84ToNad83(latitude, longitude);

            // Assert
            Assert.Equal(x, result!.Item2!.Value, 4);
            Assert.Equal(y, result.Item1!.Value, 4);
        }

        [Fact]
        public void ConvertNad83ToWgs84_WhenLatitudeIsNull_ReturnsNullValues()
        {
            // Arrange
            double? x = 123.456;
            double? y = null;

            // Act
            var result = GeoTransformationHelper.ConvertNad83ToWgs84(x, y);

            // Assert
            Assert.Null(result.Item1);
            Assert.Null(result.Item2);
        }

        [Fact]
        public void ConvertWgs84ToNad83_WithNullValues_ReturnsNull()
        {
            // Arrange
            double? latitude = null;
            double? longitude = null;

            // Act
            var result = GeoTransformationHelper.ConvertWgs84ToNad83(latitude, longitude);

            // Assert
            Assert.Null(result.Item1);
            Assert.Null(result.Item2);
        }


        [InlineData(40.681939660888951, -73.8832294373166, 1016637, 187747.0295)]
        [Theory]
        [Trait("Category", "Unit")]
        public void Projection_Convert_Wgs84_To_Nad83(double latitude, double longitude, double x, double y)
        {
            var result = GeoTransformationHelper.ConvertWgs84ToNad83(new[] { longitude, latitude });

            Assert.Equal(y, result[0], 4);
            Assert.Equal(x, result[1], 4);
        }

        [InlineData(1016637, 187747, 40.681939660888951, -73.8832294373166)]
        [Theory]
        [Trait("Category", "Unit")]
        public void Projection_Convert_Nad83_To_Wgs84(double x, double y, double latitude, double longitude)
        {
            var result = GeoTransformationHelper.ConvertNad83ToWgs84(new[] { x, y });

            Assert.Equal(latitude, result[1], 4);
            Assert.Equal(longitude, result[0], 4);
        }





    }
}
