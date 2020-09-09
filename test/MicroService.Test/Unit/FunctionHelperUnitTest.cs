using System;
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

        [Fact]
        [Trait("Category", "Unit")]
        public void Projection_Transform_ESRI102718_to_WGS84()
        {
            double x = 1016637;
            double y = 187747;

            var result = GeoTransformationHelper.Nad83TransformWgs84(x, y);
            var latitude = result.Item2;
            var longitude = result.Item1;

            var result2 = GeoTransformationHelper.Wgs84TransformNad83(latitude, longitude);
            var x1 = result2.Item1;
            var y1 = result2.Item2;

            Assert.NotNull(x1);
            Assert.NotNull(y1);

            Assert.Equal((decimal)x, Math.Round((decimal)x1));
            Assert.Equal((decimal)y, Math.Round((decimal)y1));
        }

    }
}
