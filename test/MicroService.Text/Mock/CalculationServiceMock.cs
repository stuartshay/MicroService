using System.Collections.Generic;
using System.Threading.Tasks;
using MicroService.Data.Models;
using MicroService.Data.Repository;
using MicroService.Service.Services;
using Moq;
using Xunit;

namespace MicroService.Text.Mock
{
    public class CalculationServiceMock
    {
        [Fact(DisplayName = "Calculate_Percentile - Mock")]
        [Trait("Category", "Mock")]
        public async Task Can_Get_Calculate_Percentile()
        {
            var dataSet = new List<TestData>
            {
                new TestData { Data = 1d, Id = 1 },
                new TestData { Data = 2d, Id = 2 },
                new TestData { Data = 3d, Id = 3 },
                new TestData { Data = 4d, Id = 4 },
            };

            var testDataRepository = new Mock<ITestDataRepository>();
            testDataRepository.Setup(b => b.FindAll())
                .ReturnsAsync(dataSet);

            var service = GetCalculationService(testDataRepository.Object);

            // Act
            var excelPercentile = 0.3;
            var sut = await service.CalculatePercentile(excelPercentile).ConfigureAwait(false);

            // Assert
            double result = 1.9d;
            Assert.Equal(result, sut);
        }

        private CalculationService GetCalculationService(ITestDataRepository testDataRepository = null)
        {
            testDataRepository = testDataRepository ?? new Mock<ITestDataRepository>().Object;

            return new CalculationService(testDataRepository);
        }
    }
}
