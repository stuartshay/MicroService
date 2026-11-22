using MicroService.Service.Services.Streets;
using MicroService.Test.Unit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text.Json;
using Xunit;

namespace MicroService.Test.Controllers
{
    /// <summary>
    /// /api/v1/Streets through the real Web API (Program.cs) in memory, with a made-up Street Centerline.
    /// </summary>
    [Trait("Category", "Integration")]
    public class StreetsControllerTest : IClassFixture<StreetsControllerTest.ApiFactory>
    {
        private readonly ApiFactory _factory;

        public StreetsControllerTest(ApiFactory factory)
        {
            _factory = factory;
        }

        [Fact(DisplayName = "Streets API - lines are GeoJSON with common spellings converted, cacheable for a day")]
        public async Task Lines()
        {
            using var response = await GetAsync(_factory, "/api/v1/Streets/lines?names=Fifth Avenue,5th Ave,West 42nd Street&borough=mn");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("application/geo+json", response.Content.Headers.ContentType?.MediaType);
            Assert.Equal("public, max-age=86400", response.Headers.CacheControl?.ToString());

            using var document = await ReadJsonAsync(response);
            var names = document.RootElement.GetProperty("features").EnumerateArray()
                .Select(f => f.GetProperty("properties").GetProperty("name").GetString());
            Assert.Equal(["5 AVE", "WEST 42 ST"], names);
        }

        [Fact(DisplayName = "Streets API - with places, each street keeps the stretch along them and counts them")]
        public async Task Lines_WithPlaces()
        {
            var (lat1, lon1) = FakeCenterline.North(1000);
            var (lat2, lon2) = FakeCenterline.North(3000);
            using var response = await GetAsync(_factory, $"/api/v1/Streets/lines?names=5 AVE&borough=MN&places={lat1:F6},{lon1:F6};{lat2:F6},{lon2:F6}&margin=200");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            using var document = await ReadJsonAsync(response);
            var feature = Assert.Single(document.RootElement.GetProperty("features").EnumerateArray());
            Assert.Equal(2, feature.GetProperty("properties").GetProperty("places").GetInt32());
        }

        [Theory(DisplayName = "Streets API - bad lines requests are 400 with the field in error")]
        [InlineData("/api/v1/Streets/lines?borough=MN", "names")]
        [InlineData("/api/v1/Streets/lines?names=1,2,3,4,5,6,7,8,9,10,11&borough=MN", "names")]
        [InlineData("/api/v1/Streets/lines?names=5 AVE%25&borough=MN", "names")]
        [InlineData("/api/v1/Streets/lines?names=5 AVE", "borough")]
        [InlineData("/api/v1/Streets/lines?names=5 AVE&borough=NJ", "borough")]
        [InlineData("/api/v1/Streets/lines?names=5 AVE&borough=MN&simplify=51", "simplify")]
        [InlineData("/api/v1/Streets/lines?names=5 AVE&borough=MN&simplify=-1", "simplify")]
        [InlineData("/api/v1/Streets/lines?names=5 AVE&borough=MN&simplify=NaN", "simplify")]
        [InlineData("/api/v1/Streets/lines?names=5 AVE&borough=MN&places=40.75,-73.98&margin=-1", "margin")]
        [InlineData("/api/v1/Streets/lines?names=5 AVE&borough=MN&places=40.75,-73.98&margin=NaN", "margin")]
        [InlineData("/api/v1/Streets/lines?names=5 AVE&borough=MN&places=40.75,-73.98&radius=NaN", "radius")]
        [InlineData("/api/v1/Streets/lines?names=5 AVE&borough=MN&places=x", "places")]
        [InlineData("/api/v1/Streets/lines?names=5 AVE&borough=MN&places=40.75,-73.98&radius=5", "radius")]
        [InlineData("/api/v1/Streets/lines?names=5 AVE&borough=MN&places=40.75,-73.98&margin=2001", "margin")]
        public async Task Lines_BadRequest(string uri, string field)
        {
            using var response = await GetAsync(_factory, uri);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Null(response.Headers.CacheControl);
            using var document = await ReadJsonAsync(response);
            Assert.True(document.RootElement.GetProperty("errors").TryGetProperty(field, out _), $"No error for {field}");
        }

        [Fact(DisplayName = "Streets API - search lists matching names with their borough")]
        public async Task Search()
        {
            using var response = await GetAsync(_factory, "/api/v1/Streets/search?q=fifth avenue&borough=MN");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("public, max-age=86400", response.Headers.CacheControl?.ToString());
            using var document = await ReadJsonAsync(response);
            var first = document.RootElement[0];
            Assert.Equal("5 AVE", first.GetProperty("name").GetString());
            Assert.Equal("MN", first.GetProperty("borough").GetString());
            Assert.Equal(139, first.GetProperty("segments").GetInt32());
        }

        [Theory(DisplayName = "Streets API - bad searches are 400")]
        [InlineData("/api/v1/Streets/search?q=5", "q")]
        [InlineData("/api/v1/Streets/search", "q")]
        [InlineData("/api/v1/Streets/search?q=5 AVE;", "q")]
        [InlineData("/api/v1/Streets/search?q=5 AVE&borough=XX", "borough")]
        public async Task Search_BadRequest(string uri, string field)
        {
            using var response = await GetAsync(_factory, uri);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            using var document = await ReadJsonAsync(response);
            Assert.True(document.RootElement.GetProperty("errors").TryGetProperty(field, out _), $"No error for {field}");
        }

        [Fact(DisplayName = "Streets API - 502 when NYC Open Data does not answer")]
        public async Task CenterlineDown()
        {
            await using var factory = new ApiFactory();
            factory.Centerline.Status = HttpStatusCode.ServiceUnavailable;

            using var lines = await GetAsync(factory, "/api/v1/Streets/lines?names=5 AVE&borough=MN");
            using var search = await GetAsync(factory, "/api/v1/Streets/search?q=5 AVE");

            Assert.Equal(HttpStatusCode.BadGateway, lines.StatusCode);
            Assert.Equal(HttpStatusCode.BadGateway, search.StatusCode);
            Assert.Null(lines.Headers.CacheControl);
        }

        private static async Task<HttpResponseMessage> GetAsync(ApiFactory factory, string uri)
        {
            using var client = factory.CreateClient();
            return await client.GetAsync(uri, TestContext.Current.CancellationToken);
        }

        private static async Task<JsonDocument> ReadJsonAsync(HttpResponseMessage response) =>
            await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(TestContext.Current.CancellationToken), cancellationToken: TestContext.Current.CancellationToken);

        public sealed class ApiFactory : WebApplicationFactory<Program>
        {
            public FakeCenterline Centerline { get; } = new();

            protected override void ConfigureWebHost(IWebHostBuilder builder)
            {
                builder.UseEnvironment("Docker");
                builder.UseSetting("ShapeConfiguration:ShapeRootDirectory", Path.Join(FindRepositoryRoot(), "files"));
                builder.ConfigureTestServices(services =>
                    services.AddHttpClient<StreetCenterlineClient>().ConfigurePrimaryHttpMessageHandler(() => Centerline));
            }

            private static string FindRepositoryRoot()
            {
                var directory = new DirectoryInfo(AppContext.BaseDirectory);
                while (directory != null && !File.Exists(Path.Join(directory.FullName, "MicroService.slnx")))
                {
                    directory = directory.Parent;
                }

                return directory?.FullName ?? throw new DirectoryNotFoundException("MicroService.slnx not found above the test output directory.");
            }
        }
    }
}
