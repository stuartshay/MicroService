using MicroService.Data.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace MicroService.Text.Fixture
{
    public class TestDataFixture : FixtureConfig
    {
        public TestDataFixture()
        {
            var serviceProvider = new ServiceCollection()
                .AddSingleton<ITestDataRepository>(new TestDataRepository(DbConnection))
                .BuildServiceProvider();

            TestDataRepository = serviceProvider.GetRequiredService<ITestDataRepository>();
        }

        public ITestDataRepository TestDataRepository { get; }

        public override void Dispose()
        {
        }

    }
}
