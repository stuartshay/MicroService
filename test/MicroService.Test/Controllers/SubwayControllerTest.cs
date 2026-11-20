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

        // The 28 places of the stations map (Manhattan Subway and Railway Stations on the National Register)
        private const string StationsMapPlaces =
            "40.735274,-73.991055;40.746667,-73.994167;40.743333,-73.984444;40.746081,-73.982076;" +
            "40.768056,-73.982500;40.778510,-73.981930;40.783827,-73.980001;40.779444,-73.956111;" +
            "40.803889,-73.966944;40.807778,-73.964444;40.815581,-73.958372;40.820278,-73.936667;" +
            "40.840556,-73.940556;40.849505,-73.933596;40.850891,-73.938289;40.858889,-73.934722;" +
            "40.730102,-73.990852;40.726389,-73.995278;40.704088,-74.014523;40.713056,-74.004444;" +
            "40.712657,-74.004136;40.715556,-74.009722;40.712600,-74.006700;40.860593,-73.926345;" +
            "40.752710,-73.977240;40.755278,-73.987500;40.707710,-74.011717;40.731667,-74.001389";

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
        [InlineData("/api/v1/Subway/lines?simplify=NaN", "simplify", "0 to 50")]
        [InlineData("/api/v1/Subway/lines?simplify=five", "simplify", "not valid")]
        public async Task Lines_BadInput(string uri, string field, string message)
        {
            await AssertBadRequest(uri, field, message);
        }

        [Fact(DisplayName = "Subway API - every bad lines parameter is reported at once")]
        public async Task Lines_ReportsEveryProblem()
        {
            using var response = await GetAsync("/api/v1/Subway/lines?services=1,Q9&borough=XX&simplify=99");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            using var document = await ReadJsonAsync(response);
            var errors = document.RootElement.GetProperty("errors");
            Assert.Contains("Q9", errors.GetProperty("services")[0].GetString());
            Assert.Contains("'XX'", errors.GetProperty("borough")[0].GetString());
            Assert.Contains("0 to 50", errors.GetProperty("simplify")[0].GetString());
        }

        [Theory(DisplayName = "Subway API - borough and service codes match regardless of case; blanks mean the defaults")]
        [InlineData("/api/v1/Subway/lines?services=l,s&borough=bk&simplify=0", new[] { "L", "SF" })]
        [InlineData("/api/v1/Subway/lines?services=SIR&borough=%20SI%20", new[] { "SIR" })]
        [InlineData("/api/v1/Subway/lines?services=,&borough=&simplify=50", null)]
        [InlineData("/api/v1/Subway/lines?services=1&borough=SI", new string[0])]
        public async Task Lines_CodesAndDefaults(string uri, string[]? expected)
        {
            using var response = await GetAsync(uri);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            using var document = await ReadJsonAsync(response);
            var services = document.RootElement.GetProperty("features").EnumerateArray()
                .Select(f => f.GetProperty("properties").GetProperty("service").GetString())
                .ToList();

            if (expected is null)
            {
                Assert.Equal(27, services.Count);
            }
            else
            {
                Assert.Equal(expected, services);
            }
        }

        [Fact(DisplayName = "Subway API - the stations map's lines trimmed to the stretch between its places")]
        public async Task Lines_Stretch()
        {
            var uri = $"{AllManhattanLines}&stations={StationsMapPlaces}&radius=150&stub=400";
            using var whole = await GetAsync(AllManhattanLines);
            using var response = await GetAsync(uri);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("application/geo+json", response.Content.Headers.ContentType?.MediaType);
            Assert.Equal("public, max-age=86400", response.Headers.CacheControl?.ToString());

            var wholeBody = await whole.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken);
            var body = await response.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken);
            _output.WriteLine($"stretch: {body.Length:N0} bytes, {Gzip(body):N0} gzipped; whole: {wholeBody.Length:N0} bytes, {Gzip(wholeBody):N0} gzipped");
            Assert.True(body.Length < wholeBody.Length);

            using var document = JsonDocument.Parse(body);
            var stations = document.RootElement.GetProperty("features").EnumerateArray()
                .ToDictionary(
                    f => f.GetProperty("properties").GetProperty("service").GetString()!,
                    f => f.GetProperty("properties").GetProperty("stations").GetInt32());

            // Every service has a place on it in Manhattan
            Assert.Equal(["1", "2", "3", "4", "5", "6", "7", "A", "B", "C", "D", "E", "F", "J", "L", "M", "N", "Q", "R", "ST", "W", "Z"], stations.Keys);
            Assert.All(stations.Values, count => Assert.InRange(count, 1, 28));

            // With one place each, the E, L and 42 Street Shuttle are stubs
            Assert.Equal(["E", "L", "ST"], stations.Where(s => s.Value == 1).Select(s => s.Key));
        }

        [Fact(DisplayName = "Subway API - without stations, or with them blank, lines are whole and have no stations property")]
        public async Task Lines_WithoutStations()
        {
            using var response = await GetAsync("/api/v1/Subway/lines?services=1&borough=MN&radius=10&stub=0");
            using var blank = await GetAsync("/api/v1/Subway/lines?services=1&borough=MN&stations=%20");

            var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            Assert.Equal(body, await blank.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));

            using var document = JsonDocument.Parse(body);
            var properties = Assert.Single(document.RootElement.GetProperty("features").EnumerateArray()).GetProperty("properties");
            Assert.False(properties.TryGetProperty("stations", out _));
        }

        [Theory(DisplayName = "Subway API - bad stretch input is a 400 that names the problem")]
        [InlineData("/api/v1/Subway/lines?stations=x", "stations", "stations point 'x' is not lat,lon")]
        [InlineData("/api/v1/Subway/lines?stations=40.75", "stations", "is not lat,lon")]
        [InlineData("/api/v1/Subway/lines?stations=40.75,-200", "stations", "stations point '40.75,-200' is out of range")]
        [InlineData("/api/v1/Subway/lines?stations=;;", "stations", "stations is required")]
        [InlineData("/api/v1/Subway/lines?stations=40.75,-73.98&radius=9", "radius", "10 to 500")]
        [InlineData("/api/v1/Subway/lines?stations=40.75,-73.98&radius=501", "radius", "10 to 500")]
        [InlineData("/api/v1/Subway/lines?stations=40.75,-73.98&radius=NaN", "radius", "10 to 500")]
        [InlineData("/api/v1/Subway/lines?radius=wide", "radius", "not valid")]
        [InlineData("/api/v1/Subway/lines?stations=40.75,-73.98&stub=-1", "stub", "0 to 2000")]
        [InlineData("/api/v1/Subway/lines?stations=40.75,-73.98&stub=2001", "stub", "0 to 2000")]
        [InlineData("/api/v1/Subway/lines?stub=Infinity", "stub", "0 to 2000")]
        public async Task Lines_BadStretchInput(string uri, string field, string message)
        {
            await AssertBadRequest(uri, field, message);
        }

        [Fact(DisplayName = "Subway API - every bad stretch parameter is reported at once; more than 300 stations is a 400")]
        public async Task Lines_ReportsEveryStretchProblem()
        {
            var stations = string.Join(';', Enumerable.Repeat("40.75,-73.98", 301));
            using var response = await GetAsync($"/api/v1/Subway/lines?services=Q9&stations={stations}&radius=1&stub=5000");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            using var document = await ReadJsonAsync(response);
            var errors = document.RootElement.GetProperty("errors");
            Assert.Contains("Q9", errors.GetProperty("services")[0].GetString());
            Assert.Contains("the most is 300", errors.GetProperty("stations")[0].GetString());
            Assert.Contains("10 to 500", errors.GetProperty("radius")[0].GetString());
            Assert.Contains("0 to 2000", errors.GetProperty("stub")[0].GetString());
        }

        [Fact(DisplayName = "Subway API - every bad stations parameter is reported at once")]
        public async Task Stations_ReportsEveryProblem()
        {
            using var response = await GetAsync("/api/v1/Subway/stations?near=x&radius=1");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            using var document = await ReadJsonAsync(response);
            var errors = document.RootElement.GetProperty("errors");
            Assert.Contains("'x' is not lat,lon", errors.GetProperty("near")[0].GetString());
            Assert.Contains("10 to 500", errors.GetProperty("radius")[0].GetString());
        }

        [Fact(DisplayName = "Subway API - the default radius is 100 metres")]
        public async Task Stations_DefaultRadius()
        {
            // Wall St (4 5) is about 30 m from this point and Wall St (2 3) about 190 m
            using var response = await GetAsync("/api/v1/Subway/stations?near=40.70771,-74.011717");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            using var document = await ReadJsonAsync(response);
            var stations = document.RootElement.GetProperty("points")[0].GetProperty("stations").EnumerateArray().ToList();
            Assert.NotEmpty(stations);
            Assert.All(stations, s => Assert.True(s.GetProperty("distance").GetDouble() <= 100));
            Assert.DoesNotContain("2", document.RootElement.GetProperty("services").EnumerateArray().Select(s => s.GetString()));
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
        [InlineData("/api/v1/Subway/stations?near=40.75,-73.98&radius=Infinity", "radius", "10 to 500")]
        [InlineData("/api/v1/Subway/stations?near=40.75,-73.98&radius=wide", "radius", "not valid")]
        [InlineData("/api/v1/Subway/stations?near=40.75,-200", "near", "is out of range")]
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
