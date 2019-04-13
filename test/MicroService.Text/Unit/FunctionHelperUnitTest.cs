using MicroService.Service.Helpers;
using Xunit;

namespace MicroService.Text.Unit
{
    public class FunctionHelperUnitTest
    {
        [Fact(DisplayName = "Calculate_Percentile_1 - Unit")]
        [Trait("Category", "Unit")]
        public void Can_Calculate_Percentile_1()
        {
            double result1 = 3.4d;
            var calculated1 = FunctionHelper.Percentile1(new double[] { 1, 2, 3, 4 }, 0.8);

            double result2 = 8.05d;
            var calculated2 = FunctionHelper.Percentile1(new double[] { 7, 8, 9, 20 }, 0.35);

            double result3 = 1.9d;
            var calculated3 = FunctionHelper.Percentile1(new double[] { 1, 2, 3, 4 }, 0.3);

            // Assert
            // TODO Fix Percision
            //Assert.Equal(result1, calculated1);

            // Assert
            //Assert.Equal(result2, calculated2);
            Assert.Equal(result3, calculated3);
        }

        [Fact(DisplayName = "Calculate_Percentile_2 - Unit")]
        [Trait("Category", "Unit")]
        public void Can_Calculate_Percentile_2()
        {
            double result1 = 3.4d;
            var calculated1 = FunctionHelper.Percentile2(new double[] { 1, 2, 3, 4 }, 0.8);

            double result2 = 8.05d;
            var calculated2 = FunctionHelper.Percentile2(new double[] { 7, 8, 9, 20 }, 0.35);

            double result3 = 1.9d;
            var calculated3 = FunctionHelper.Percentile2(new double[] { 1, 2, 3, 4 }, 0.3);

            // Assert
            // TODO Fix Percision
            //Assert.Equal(result1, calculated1);

            // Assert
            Assert.Equal(result2, calculated2);
            Assert.Equal(result3, calculated3);
        }

        [Fact(DisplayName = "Calculate_Percentile_3 - Unit")]
        [Trait("Category", "Unit")]
        public void Can_Calculate_Percentile_3()
        {
            double result1 = 3.4d;
            var calculated1 = FunctionHelper.Percentile3(new double[] { 1, 2, 3, 4 }, 0.8);

            double result2 = 8.05d;
            var calculated2 = FunctionHelper.Percentile3(new double[] { 7, 8, 9, 20 }, 0.35);

            double result3 = 1.9d;
            var calculated3 = FunctionHelper.Percentile3(new double[] { 1, 2, 3, 4 }, 0.3);

            // Assert
            // TODO Fix Percision
            //Assert.Equal(result1, calculated1);

            // Assert
            Assert.Equal(result2, calculated2);
            Assert.Equal(result3, calculated3);
        }
    }
}
