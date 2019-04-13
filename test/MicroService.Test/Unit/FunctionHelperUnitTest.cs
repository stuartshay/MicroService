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
    }
}
