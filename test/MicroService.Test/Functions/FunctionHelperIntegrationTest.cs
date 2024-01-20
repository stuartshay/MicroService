using MicroService.Data.Repository;
using MicroService.Service.Helpers;
using MicroService.Test.Fixture;
using Xunit;
using Xunit.Abstractions;

namespace MicroService.Test.Functions
{
    public class FunctionHelperIntegrationTest : IClassFixture<TestDataFixture>
    {
        private readonly ITestDataRepository _testDataRepository;

        private readonly ITestOutputHelper _output;

        public FunctionHelperIntegrationTest(TestDataFixture config, ITestOutputHelper output)
        {
            _testDataRepository = config.TestDataRepository;
            _output = output;
        }

        [Fact(Skip = "Deprecated", DisplayName = "Calculate_Percentile - Integration")]
        [Trait("Category", "Integration")]
        public async Task Can_Calculate_Percentile()
        {
            // Arrange
            double result = 9949.9563797144219;
            var results = await _testDataRepository.FindAll();
            var array = results.Select(x => x.Data).ToArray();

            // Act
            var sut = FunctionHelper.Percentile(array, 0.995);

            // Assert
            _output.WriteLine($"Result:{sut}");
            Assert.NotNull(results);
            Assert.Equal(sut, result);
        }
    }
}
