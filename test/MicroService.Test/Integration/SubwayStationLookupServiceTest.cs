using MicroService.Service.Services.FlatFileService;
using MicroService.Service.Services.Subway;
using MicroService.Test.Fixture;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace MicroService.Test.Integration
{
    [Trait("Category", "Integration")]
    public class SubwayStationLookupServiceTest : IDisposable
    {
        private readonly MemoryCache _cache = new(new MemoryCacheOptions());
        private readonly SubwayStationLookupService _service;

        public SubwayStationLookupServiceTest()
        {
            _service = new SubwayStationLookupService(new StationFlatFileService(RepositoryFiles.Options()), _cache);
        }

        public void Dispose()
        {
            _cache.Dispose();
            GC.SuppressFinalize(this);
        }

        [Fact(DisplayName = "Subway stations - Times Square has the 1 2 3 7 N Q R W and the shuttle")]
        public void FindNear_TimesSquare()
        {
            var result = _service.FindNear([(40.755278, -73.9875)], 150);

            var point = Assert.Single(result.Points);
            Assert.NotEmpty(point.Stations);
            Assert.Equal(point.Stations.OrderBy(s => s.Distance).Select(s => s.StationId), point.Stations.Select(s => s.StationId));
            Assert.All(point.Stations, s => Assert.InRange(s.Distance, 0, 150));
            Assert.Equal(["1", "2", "3", "7", "N", "Q", "R", "S", "W"], result.Services);
        }

        [Fact(DisplayName = "Subway stations - Wall St on Lexington Av has only the 4 and 5")]
        public void FindNear_WallStreetIrt()
        {
            var result = _service.FindNear([(40.70771, -74.011717)], 60);

            var station = Assert.Single(Assert.Single(result.Points).Stations);
            Assert.Equal("Wall St", station.StopName);
            Assert.Equal(["4", "5"], station.Routes);
            Assert.Equal("Subway", station.Structure);
            Assert.InRange(station.Distance, 0, 60);
            Assert.Equal(["4", "5"], result.Services);
        }

        [Fact(DisplayName = "Subway stations - one result per point, in request order, services across all of them")]
        public void FindNear_SeveralPoints()
        {
            var result = _service.FindNear([(40.755278, -73.9875), (40.70771, -74.011717), (40.6, -73.6)], 150);

            Assert.Equal(3, result.Points.Count);
            Assert.Equal(40.70771, result.Points[1].Latitude);
            Assert.Empty(result.Points[2].Stations);
            Assert.Contains("4", result.Services);
            Assert.Contains("7", result.Services);
        }

        [Theory(DisplayName = "Subway stations - routes 1-7 then letters")]
        [InlineData("N W R", new[] { "N", "R", "W" })]
        [InlineData("M J Z", new[] { "J", "M", "Z" })]
        [InlineData("S 7 1", new[] { "1", "7", "S" })]
        [InlineData("SIR", new[] { "SIR" })]
        [InlineData("", new string[0])]
        public void ParseRoutes_OrdersRoutes(string routes, string[] expected)
        {
            Assert.Equal(expected, SubwayStationLookupService.ParseRoutes(routes));
        }

        [Fact(DisplayName = "Subway stations - near takes lat,lon points separated by ;")]
        public void TryParsePoints_ParsesPoints()
        {
            Assert.True(SubwayStationLookupService.TryParsePoints("40.7527,-73.9772; 40.7066,-74.0110;", out var points, out _));

            Assert.Equal([(40.7527, -73.9772), (40.7066, -74.0110)], points);
        }

        [Theory(DisplayName = "Subway stations - bad near input has a clear message")]
        [InlineData(null, "near is required")]
        [InlineData(" ", "near is required")]
        [InlineData(";", "near is required")]
        [InlineData("40.75", "'40.75' is not lat,lon")]
        [InlineData("40.75,-73.98,1", "'40.75,-73.98,1' is not lat,lon")]
        [InlineData("north,-73.98", "'north,-73.98' is not lat,lon")]
        [InlineData("NaN,-73.98", "'NaN,-73.98' is not lat,lon")]
        [InlineData("-73.98,40.75;91,0", "'91,0' is out of range")]
        [InlineData("40.75,-181", "'40.75,-181' is out of range")]
        public void TryParsePoints_RejectsBadInput(string? near, string message)
        {
            Assert.False(SubwayStationLookupService.TryParsePoints(near, out _, out var error));
            Assert.Contains(message, error);
        }

        [Fact(DisplayName = "Subway stations - at most 300 points")]
        public void TryParsePoints_RejectsMoreThan300Points()
        {
            var near = string.Join(';', Enumerable.Repeat("40.75,-73.98", 301));

            Assert.False(SubwayStationLookupService.TryParsePoints(near, out _, out var error));
            Assert.Equal("near has 301 points; the most is 300.", error);
            Assert.True(SubwayStationLookupService.TryParsePoints(string.Join(';', Enumerable.Repeat("40.75,-73.98", 300)), out _, out _));
        }

        [Fact(DisplayName = "Subway stations - haversine distance")]
        public void Distance_IsInMetres()
        {
            // One degree of latitude is about 111.2 km
            Assert.InRange(SubwayStationLookupService.Distance(40, -74, 41, -74), 111_000, 111_300);
            Assert.Equal(0, SubwayStationLookupService.Distance(40.7, -74, 40.7, -74));
        }
    }
}
