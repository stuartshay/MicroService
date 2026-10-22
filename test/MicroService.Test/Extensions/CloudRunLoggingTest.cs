using Microsoft.Extensions.Configuration;
using Xunit;

namespace MicroService.Test.Extensions
{
    public class CloudRunLoggingTest
    {
        [Fact(DisplayName = "Logging - Cloud Run logs at Information, not Debug")]
        [Trait("Category", "Unit")]
        public void CloudRun_Default_LogLevel_Is_Information()
        {
            // The test project has its own appsettings.CloudRun.json, so read the Web API's from the source tree
            var webApiDirectory = Path.Join(FindRepositoryRoot(), "src", "MicroService.WebApi");
            var configuration = new ConfigurationBuilder()
                .AddJsonFile(Path.Join(webApiDirectory, "appsettings.json"))
                .AddJsonFile(Path.Join(webApiDirectory, "appsettings.CloudRun.json"))
                .Build();

            Assert.Equal("Information", configuration["Logging:LogLevel:Default"]);
        }

        private static string FindRepositoryRoot()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null && !File.Exists(Path.Join(directory.FullName, "MicroService.sln")))
            {
                directory = directory.Parent;
            }

            return directory?.FullName ?? throw new DirectoryNotFoundException("MicroService.sln not found above the test output directory.");
        }
    }
}
