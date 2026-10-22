using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace MicroService.Test.Extensions
{
    /// <summary>
    /// Starts the real Web API (Program.cs) in memory and checks the CORS headers browsers see.
    /// </summary>
    public class CorsPipelineTest : IClassFixture<CorsPipelineTest.ApiFactory>
    {
        private const string FeatureServiceUri = "/api/v1/FeatureService";

        private readonly ApiFactory _factory;

        public CorsPipelineTest(ApiFactory factory)
        {
            _factory = factory;
        }

        [Fact(DisplayName = "CORS pipeline - the site's JSON POST preflight is allowed")]
        [Trait("Category", "Integration")]
        public async Task Preflight_From_Allowed_Origin_Returns_Cors_Headers()
        {
            using var client = _factory.CreateClient();
            using var request = new HttpRequestMessage(HttpMethod.Options, $"{FeatureServiceUri}/attributelookup");
            request.Headers.Add("Origin", "https://coredatastore.com");
            request.Headers.Add("Access-Control-Request-Method", "POST");
            request.Headers.Add("Access-Control-Request-Headers", "content-type");

            using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.True(response.Headers.TryGetValues("Access-Control-Allow-Origin", out var origins));
            Assert.Equal("https://coredatastore.com", Assert.Single(origins));
        }

        [Fact(DisplayName = "CORS pipeline - another origin gets no Access-Control-Allow-Origin")]
        [Trait("Category", "Integration")]
        public async Task Request_From_Other_Origin_Has_No_Cors_Headers()
        {
            using var client = _factory.CreateClient();
            using var request = new HttpRequestMessage(HttpMethod.Get, FeatureServiceUri);
            request.Headers.Add("Origin", "https://evil.example");

            using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.True(response.IsSuccessStatusCode);
            Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
        }

        public sealed class ApiFactory : WebApplicationFactory<Program>
        {
            protected override void ConfigureWebHost(IWebHostBuilder builder)
            {
                // Program.cs reads these before the host is built, so pass them as host settings
                builder.UseEnvironment("Docker");
                builder.UseSetting("ConnectionStrings:PostgreSql", "Host=localhost;Database=unused;Username=unused;Password=unused");
                builder.UseSetting("ShapeConfiguration:ShapeRootDirectory", Path.Combine(FindRepositoryRoot(), "files"));
            }

            private static string FindRepositoryRoot()
            {
                var directory = new DirectoryInfo(AppContext.BaseDirectory);
                while (directory != null && !File.Exists(Path.Combine(directory.FullName, "MicroService.sln")))
                {
                    directory = directory.Parent;
                }

                return directory?.FullName ?? throw new DirectoryNotFoundException("MicroService.sln not found above the test output directory.");
            }
        }
    }
}
