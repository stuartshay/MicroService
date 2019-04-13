using System.Linq;
using System.Threading.Tasks;
using MicroService.Data.Repository;
using MicroService.Service.Helpers;
using MicroService.Text.Fixture;
using Xunit;
using Xunit.Abstractions;

namespace MicroService.Text.Integration
{
    public class FunctionHelperIntegrationTest : IClassFixture<TestDataFixture>
    {
        private readonly ITestDataRepository _testDataRepository;

        private readonly ITestOutputHelper _output;

        public FunctionHelperIntegrationTest(TestDataFixture config, ITestOutputHelper output)
        {
            _testDataRepository = config.TestDataRepository;
        }

        [Fact(DisplayName = "Calculate_Percentile_1 - Integration")]
        [Trait("Category", "Integration")]
        public async Task Can_Calculate_Percentile_1()
        {
            // Arrange 
            double result = 9949.9563797144219;
            var results = await _testDataRepository.FindAll();
            var array = results.Select(x => x.Data).ToArray();

            // Act
            var sut = FunctionHelper.Percentile1(array, 0.995);

            // Assert
            Assert.NotNull(results);
            Assert.Equal(sut, result);
        }

        [Fact(DisplayName = "Calculate_Percentile_2 - Integration")]
        [Trait("Category", "Integration")]
        public async Task Can_Calculate_Percentile_2()
        {
            // Arrange 
            double result = 9949.9563797144219;
            var results = await _testDataRepository.FindAll();
            var array = results.Select(x => x.Data).ToArray();

            // Act
            var sut = FunctionHelper.Percentile2(array, 0.995);

            // Assert
            Assert.NotNull(results);
            Assert.Equal(sut, result);
        }

        [Fact(DisplayName = "Calculate_Percentile_3 - Integration")]
        [Trait("Category", "Integration")]
        public async Task Can_Calculate_Percentile_3()
        {
            // Arrange 
            double result = 9949.9563797144219;
            var results = await _testDataRepository.FindAll();
            var array = results.Select(x => x.Data).ToArray();

            // Act
            var sut = FunctionHelper.Percentile3(array, 0.995);

            // Assert
            Assert.NotNull(results);
            Assert.Equal(sut, result);
        }
    }
}
