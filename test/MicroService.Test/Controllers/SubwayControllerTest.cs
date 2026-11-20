using MicroService.Test.Extensions;
using System.IO.Compression;
using System.Net;
using System.Text.Json;
using Xunit;

namespace MicroService.Test.Controllers
{
    /// <summary>
    /// /api/v1/Subway through the real Web API (Program.cs) in memory.
    /// </summary>
    [Trait("Category", "Integration")]
    public class SubwayControllerTest : IClassFixture<CorsPipelineTest.ApiFactory>
    {
        private const string AllManhattanLines =
            "/api/v1/Subway/lines?services=1,2,3,4,5,6,7,A,B,C,D,E,F,J,L,M,N,Q,R,S,W,Z&borough=MN&simplify=5";

        private readonly CorsPipelineTest.ApiFactory _factory;
        private readonly ITestOutputHelper _output;

        public SubwayControllerTest(CorsPipelineTest.ApiFactory factory, ITestOutputHelper output)
        {
            _factory = factory;
            _output = output;
        }

        [Fact(DisplayName = "Subway API - services with colors, cacheable for a day")]
        public async Task Services()
        {
            using var response = await GetAsync("/api/v1/Subway/services");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("public, max-age=86400", response.Headers.CacheControl?.ToString());

            using var document = await ReadJsonAsync(response);
            var n = document.RootElement.EnumerateArray().Single(s => s.GetProperty("service").GetString() == "N");
            Assert.Equal("Broadway Express", n.GetProperty("name").GetString());
            Assert.Equal("#FCCC0A", n.GetProperty("color").GetString());
            Assert.Equal("#000000", n.GetProperty("textColor").GetString());
        }

        [Fact(DisplayName = "Subway API - lines are GeoJSON, cacheable for a day")]
        public async Task Lines()
        {
            using var response = await GetAsync(AllManhattanLines);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("application/geo+json", response.Content.Headers.ContentType?.MediaType);
            Assert.Equal("public, max-age=86400", response.Headers.CacheControl?.ToString());

            var body = await response.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken);
            _output.WriteLine($"{AllManhattanLines}: {body.Length:N0} bytes, {Gzip(body):N0} gzipped");

            using var document = JsonDocument.Parse(body);
            var services = document.RootElement.GetProperty("features").EnumerateArray()
                .Select(f => f.GetProperty("properties").GetProperty("service").GetString())
                .ToList();

            // S is the 42 Street Shuttle in Manhattan; the Franklin Avenue and Rockaway Park shuttles are clipped away
            Assert.Equal(["1", "2", "3", "4", "5", "6", "7", "A", "B", "C", "D", "E", "F", "J", "L", "M", "N", "Q", "R", "ST", "W", "Z"], services);
        }

        [Fact(DisplayName = "Subway API - every service when none are named")]
        public async Task Lines_EveryService()
        {
            using var response = await GetAsync("/api/v1/Subway/lines");
            using var document = await ReadJsonAsync(response);

            Assert.Equal(27, document.RootElement.GetProperty("features").GetArrayLength());
        }

        [Theory(DisplayName = "Subway API - bad lines input is a 400 that names the problem")]
        [InlineData("/api/v1/Subway/lines?services=1,X,9", "services", "Unknown service codes: X, 9")]
        [InlineData("/api/v1/Subway/lines?borough=Manhattan", "borough", "Unknown borough 'Manhattan'")]
        [InlineData("/api/v1/Subway/lines?borough=1", "borough", "Unknown borough '1'")]
        [InlineData("/api/v1/Subway/lines?simplify=51", "simplify", "0 to 50")]
        [InlineData("/api/v1/Subway/lines?simplify=-1", "simplify", "0 to 50")]
        public async Task Lines_BadInput(string uri, string field, string message)
        {
            await AssertBadRequest(uri, field, message);
        }

        [Fact(DisplayName = "Subway API - stations near points and their services")]
        public async Task Stations()
        {
            using var response = await GetAsync("/api/v1/Subway/stations?near=40.7527,-73.9772;40.70771,-74.011717&radius=60");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("public, max-age=86400", response.Headers.CacheControl?.ToString());

            using var document = await ReadJsonAsync(response);
            var points = document.RootElement.GetProperty("points");
            Assert.Equal(2, points.GetArrayLength());

            var wallStreet = Assert.Single(points[1].GetProperty("stations").EnumerateArray());
            Assert.Equal("Wall St", wallStreet.GetProperty("stopName").GetString());
            Assert.Equal(["4", "5"], wallStreet.GetProperty("routes").EnumerateArray().Select(r => r.GetString()));
            Assert.True(wallStreet.GetProperty("distance").GetDouble() <= 60);
            Assert.Contains("5", document.RootElement.GetProperty("services").EnumerateArray().Select(s => s.GetString()));
        }

        [Theory(DisplayName = "Subway API - bad stations input is a 400 that names the problem")]
        [InlineData("/api/v1/Subway/stations", "near", "near is required")]
        [InlineData("/api/v1/Subway/stations?near=40.75", "near", "is not lat,lon")]
        [InlineData("/api/v1/Subway/stations?near=40.75,-73.98&radius=5", "radius", "10 to 500")]
        [InlineData("/api/v1/Subway/stations?near=40.75,-73.98&radius=501", "radius", "10 to 500")]
        public async Task Stations_BadInput(string uri, string field, string message)
        {
            await AssertBadRequest(uri, field, message);
        }

        [Fact(DisplayName = "Subway API - more than 300 points is a 400")]
        public async Task Stations_TooManyPoints()
        {
            var near = string.Join(';', Enumerable.Repeat("40.75,-73.98", 301));

            await AssertBadRequest($"/api/v1/Subway/stations?near={near}", "near", "the most is 300");
        }

        private async Task AssertBadRequest(string uri, string field, string message)
        {
            using var response = await GetAsync(uri);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Null(response.Headers.CacheControl);

            using var document = await ReadJsonAsync(response);
            var errors = document.RootElement.GetProperty("errors").GetProperty(field).EnumerateArray().Select(e => e.GetString());
            Assert.Contains(errors, e => e!.Contains(message, StringComparison.Ordinal));
        }

        private async Task<HttpResponseMessage> GetAsync(string uri)
        {
            using var client = _factory.CreateClient();
            return await client.GetAsync(uri, TestContext.Current.CancellationToken);
        }

        private static async Task<JsonDocument> ReadJsonAsync(HttpResponseMessage response) =>
            await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(TestContext.Current.CancellationToken), cancellationToken: TestContext.Current.CancellationToken);

        private static long Gzip(byte[] body)
        {
            using var output = new MemoryStream();
            using (var gzip = new GZipStream(output, CompressionLevel.Optimal, leaveOpen: true))
            {
                gzip.Write(body);
            }

            return output.Length;
        }
    }
}
