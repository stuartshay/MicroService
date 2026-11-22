using MicroService.Data.Enum;
using MicroService.Service.Services.Streets;
using MicroService.Service.Services.Subway;
using Microsoft.Extensions.Caching.Memory;
using System.Net;
using System.Text.Json;
using Xunit;

namespace MicroService.Test.Unit
{
    public class StreetLineServiceTest
    {
        private readonly FakeCenterline _fake = new();
        private readonly StreetCenterlineClient _client;
        private readonly StreetLineService _service;

        public StreetLineServiceTest()
        {
            _client = new StreetCenterlineClient(new HttpClient(_fake) { BaseAddress = new Uri("https://example.test/") }, new MemoryCache(new MemoryCacheOptions()));
            _service = new StreetLineService(_client);
        }

        [Fact(DisplayName = "Street centerline - asks for a street's segments by each spelling in the borough, and keeps them a day")]
        public async Task GetSegments_QueriesAndCaches()
        {
            var segments = await _client.GetSegmentsAsync("W 42 ST", Borough.MN, TestContext.Current.CancellationToken);
            await _client.GetSegmentsAsync("W 42 ST", Borough.MN, TestContext.Current.CancellationToken);

            Assert.Single(segments.Segments);
            Assert.Equal("W  42 ST", segments.Spelling);
            var query = Assert.Single(_fake.Queries);
            Assert.Contains("$where=boroughcode='1' AND full_street_name in('W  42 ST','WEST 42 ST')", query, StringComparison.Ordinal);
        }

        [Fact(DisplayName = "Street centerline - only the first spelling found is used, as two spellings can be two streets")]
        public async Task GetSegments_UsesTheFirstSpellingFound()
        {
            _fake.Streets["EAST DR"] = [FakeCenterline.Segment(0, 100)];
            _fake.Streets["E  DR"] = [FakeCenterline.Segment(0, 100), FakeCenterline.Segment(100, 200)];

            var east = await _client.GetSegmentsAsync("EAST DR", Borough.MN, TestContext.Current.CancellationToken);
            var e = await _client.GetSegmentsAsync("E DR", Borough.MN, TestContext.Current.CancellationToken);
            var none = await _client.GetSegmentsAsync("MADISON AVE", Borough.MN, TestContext.Current.CancellationToken);

            Assert.Equal(("EAST DR", 1), (east.Spelling, east.Segments.Count));
            Assert.Equal(("E  DR", 2), (e.Spelling, e.Segments.Count));
            Assert.Null(none.Spelling);
            Assert.Empty(none.Segments);
        }

        [Fact(DisplayName = "Street centerline - rows without a name or a usable shape are skipped")]
        public async Task GetSegments_SkipsRowsWithoutAShape()
        {
            _fake.ExtraSegmentRows.Add(new { full_street_name = "5 AVE" });
            _fake.ExtraSegmentRows.Add(new { full_street_name = "5 AVE", the_geom = default(object) });
            _fake.ExtraSegmentRows.Add(new { the_geom = new { type = "Point", coordinates = new[] { -73.98, 40.75 } } });
            _fake.ExtraSegmentRows.Add(new { full_street_name = "5 AVE", the_geom = new { type = "MultiLineString", coordinates = Array.Empty<double[][]>() } });

            Assert.Equal(2, (await _client.GetSegmentsAsync("5 AVE", Borough.MN, TestContext.Current.CancellationToken)).Segments.Count);
        }

        [Fact(DisplayName = "Street centerline - the client and the service need their dependencies")]
        public void Constructors_CheckTheirArguments()
        {
            using var cache = new MemoryCache(new MemoryCacheOptions());
            using var http = new HttpClient();
            Assert.Throws<ArgumentNullException>(() => new StreetCenterlineClient(null!, cache));
            Assert.Throws<ArgumentNullException>(() => new StreetCenterlineClient(http, null!));
            Assert.Throws<ArgumentNullException>(() => new StreetLineService(null!));
        }

        [Fact(DisplayName = "Street lines - a street too short to survive rounding is left out")]
        public async Task Lines_LeaveOutAStreetThatRoundsAway()
        {
            _fake.Streets["TINY ST"] = [[[FakeCenterline.Longitude, FakeCenterline.Latitude], [FakeCenterline.Longitude + 1e-8, FakeCenterline.Latitude]]];

            using var document = JsonDocument.Parse(await _service.GetLinesGeoJsonAsync(["TINY ST"], Borough.MN, 0, null, TestContext.Current.CancellationToken));

            Assert.Empty(document.RootElement.GetProperty("features").EnumerateArray());
        }

        [Fact(DisplayName = "Street centerline - a quote in a name is doubled in the query")]
        public async Task GetSegments_QuotesTheName()
        {
            await _client.GetSegmentsAsync("ST JOHN'S PL", Borough.BK, TestContext.Current.CancellationToken);

            Assert.Contains("full_street_name in('ST JOHN''S PL')", Assert.Single(_fake.Queries), StringComparison.Ordinal);
        }

        [Fact(DisplayName = "Street centerline - search lists names single-spaced with their borough, skipping unknown boroughs")]
        public async Task Search()
        {
            var names = await _client.SearchAsync("W 42", Borough.MN, TestContext.Current.CancellationToken);
            await _client.SearchAsync("W 42", Borough.MN, TestContext.Current.CancellationToken);

            // Rows without a name or a known borough are skipped; a row without a count has 0
            Assert.Equal([new StreetName("5 AVE", "MN", 139), new StreetName("W 42 ST", "MN", 11), new StreetName("MADISON AVE", "MN", 0)], names);
            var query = Assert.Single(_fake.Queries);
            Assert.Contains("upper(full_street_name) like '%W  42%' OR upper(full_street_name) like '%WEST 42%'", query, StringComparison.Ordinal);
            Assert.Contains("AND boroughcode='1'", query, StringComparison.Ordinal);
        }

        [Fact(DisplayName = "Street centerline - an error reply or one that is not a list fails")]
        public async Task Errors()
        {
            _fake.Status = HttpStatusCode.ServiceUnavailable;
            await Assert.ThrowsAsync<HttpRequestException>(() => _client.GetSegmentsAsync("5 AVE", Borough.MN, TestContext.Current.CancellationToken));

            _fake.Status = HttpStatusCode.OK;
            _fake.Body = """{"error": true}""";
            await Assert.ThrowsAsync<HttpRequestException>(() => _client.SearchAsync("5 AVE", null, TestContext.Current.CancellationToken));
        }

        [Fact(DisplayName = "Street lines - a street's segments are merged into one line with its name and borough")]
        public async Task Lines_MergeTheSegments()
        {
            using var document = await LinesAsync(["5 AVE", "MADISON AVE"], null);

            var feature = Assert.Single(document.RootElement.GetProperty("features").EnumerateArray());
            Assert.Equal("5 AVE", feature.GetProperty("properties").GetProperty("name").GetString());
            Assert.Equal("MN", feature.GetProperty("properties").GetProperty("borough").GetString());
            Assert.False(feature.GetProperty("properties").TryGetProperty("places", out _));

            var lines = feature.GetProperty("geometry").GetProperty("coordinates");
            var line = Assert.Single(lines.EnumerateArray());

            // Simplified to its two ends: a straight line
            Assert.Equal(2, line.GetArrayLength());
            Assert.Equal(5000, NorthOf(line[1]) - NorthOf(line[0]), 0);
        }

        [Fact(DisplayName = "Street lines - names that find the same centerline street draw it once; streets that only look alike both draw")]
        public async Task Lines_DrawEachCenterlineStreetOnce()
        {
            _fake.Streets["EAST DR"] = [FakeCenterline.Segment(0, 100)];
            _fake.Streets["E  DR"] = [FakeCenterline.Segment(200, 300)];

            using var document = await LinesAsync(["W 42 ST", "WEST 42 ST", "EAST DR", "E DR"], null);

            var names = document.RootElement.GetProperty("features").EnumerateArray()
                .Select(f => f.GetProperty("properties").GetProperty("name").GetString());
            Assert.Equal(["W 42 ST", "EAST DR", "E DR"], names);
        }

        [Fact(DisplayName = "Street lines - with places, a street keeps the stretch along them plus the margin, and counts them")]
        public async Task Lines_KeepTheStretch()
        {
            var stretch = new SubwayStretch([FakeCenterline.North(1000), FakeCenterline.North(3000), (40.70, -73.90)], 100, 100, 100);
            using var document = await LinesAsync(["5 AVE"], stretch);

            var feature = Assert.Single(document.RootElement.GetProperty("features").EnumerateArray());
            Assert.Equal(2, feature.GetProperty("properties").GetProperty("places").GetInt32());

            var points = feature.GetProperty("geometry").GetProperty("coordinates").EnumerateArray().SelectMany(l => l.EnumerateArray()).Select(NorthOf).ToList();
            Assert.Equal(900, points.Min(), 0);
            Assert.Equal(3100, points.Max(), 0);
        }

        [Fact(DisplayName = "Street lines - a street with no place along it is left out")]
        public async Task Lines_LeaveOutAStreetWithoutPlaces()
        {
            var stretch = new SubwayStretch([(40.70, -73.90)], 100, 100, 100);
            using var document = await LinesAsync(["5 AVE"], stretch);

            Assert.Empty(document.RootElement.GetProperty("features").EnumerateArray());
        }

        private async Task<JsonDocument> LinesAsync(string[] names, SubwayStretch? stretch) =>
            JsonDocument.Parse(await _service.GetLinesGeoJsonAsync(names, Borough.MN, StreetLineService.DefaultSimplifyMetres, stretch, TestContext.Current.CancellationToken));

        private static double NorthOf(JsonElement point) => (point[1].GetDouble() - FakeCenterline.Latitude) * FakeCenterline.MetresPerDegree;
    }
}
