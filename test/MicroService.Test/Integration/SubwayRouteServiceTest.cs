using MicroService.Data.Enum;
using MicroService.Service.Helpers;
using MicroService.Service.Services;
using MicroService.Service.Services.Subway;
using MicroService.Test.Fixture;
using Microsoft.Extensions.DependencyInjection;
using NetTopologySuite.Geometries;
using System.Text.Json;
using Xunit;

namespace MicroService.Test.Integration
{
    [Trait("Category", "Integration")]
    public class SubwayRouteServiceTest : IClassFixture<ShapeServiceFixture>, IDisposable
    {
        private readonly ShapeServiceFixture _fixture;
        private readonly SubwayLineCache _cache = new();
        private readonly SubwayRouteService _service;

        public SubwayRouteServiceTest(ShapeServiceFixture fixture)
        {
            _fixture = fixture;
            _service = new SubwayRouteService(
                fixture.SubwayServiceLinesService,
                fixture.ServiceProvider.GetRequiredService<BoroughBoundariesService>(),
                _cache);
        }

        public void Dispose()
        {
            _cache.Dispose();
            GC.SuppressFinalize(this);
        }

        [Fact(DisplayName = "Subway services - every service once, 1-7 then letters")]
        public void GetServices_ListsEveryServiceInRouteOrder()
        {
            var services = _service.GetServices().Select(s => s.Service).ToList();

            // 29 features: A and C have two each
            Assert.Equal(29, _fixture.SubwayServiceLinesService.GetFeatures().Count);
            Assert.Equal(
                ["1", "2", "3", "4", "5", "5 Peak", "6", "7", "A", "B", "C", "D", "E", "F", "G", "J", "L", "M", "N", "Q", "R", "SF", "SIR", "SR", "ST", "W", "Z"],
                services);
        }

        [Theory(DisplayName = "Subway services - MTA colors, black text only on the yellow lines")]
        [InlineData("1", "#EE352E", "#FFFFFF", "Broadway / 7AV Local")]
        [InlineData("3", "#EE352E", "#FFFFFF", "7 Avenue Express")]
        [InlineData("5 Peak", "#00933C", "#FFFFFF", "Lexington Av Express")]
        [InlineData("7", "#B933AD", "#FFFFFF", "Flushing Local")]
        [InlineData("A", "#0039A6", "#FFFFFF", "8 Avenue Express")]
        [InlineData("M", "#FF6319", "#FFFFFF", "6 Avenue Local")]
        [InlineData("G", "#6CBE45", "#FFFFFF", "Brooklyn-Queens Crosstown Local")]
        [InlineData("Z", "#996633", "#FFFFFF", "Nassau Street Express")]
        [InlineData("L", "#A7A9AC", "#FFFFFF", "14 St-Canarsie Local")]
        [InlineData("N", "#FCCC0A", "#000000", "Broadway Express")]
        [InlineData("W", "#FCCC0A", "#000000", "Broadway Local")]
        [InlineData("ST", "#808183", "#FFFFFF", "42 Street Shuttle")]
        [InlineData("SIR", "#0039A6", "#FFFFFF", "Staten Island Railway")]
        public void GetServices_HasMtaColors(string service, string color, string textColor, string name)
        {
            var route = Assert.Single(_service.GetServices(), s => s.Service == service);

            Assert.Equal(color, route.Color);
            Assert.Equal(textColor, route.TextColor);
            Assert.Equal(name, route.Name);
        }

        [Fact(DisplayName = "Subway lines - codes match regardless of case, S means every shuttle, unknown codes are named")]
        public void ResolveServices_NormalizesCodesAndNamesUnknownOnes()
        {
            var (services, unknown) = _service.ResolveServices(["a", " 1 ", "S", "5 peak", "X", "9", "x"]);

            Assert.Equal(["1", "5 Peak", "A", "SF", "SR", "ST"], services);
            Assert.Equal(["X", "9"], unknown);

            var (all, none) = _service.ResolveServices(null);
            Assert.Equal(27, all.Count);
            Assert.Empty(none);
        }

        [Fact(DisplayName = "Subway lines - one feature per requested service with its properties")]
        public void GetLines_ReturnsOneFeaturePerService()
        {
            var features = Features(_service.GetLinesGeoJson(["1", "2", "3"], null, 5));

            Assert.Equal(["1", "2", "3"], features.Select(f => f.GetProperty("properties").GetProperty("service").GetString()));
            var one = features[0];
            Assert.Equal("MultiLineString", one.GetProperty("geometry").GetProperty("type").GetString());
            Assert.Equal("Broadway / 7AV Local", one.GetProperty("properties").GetProperty("name").GetString());
            Assert.Equal("#EE352E", one.GetProperty("properties").GetProperty("color").GetString());
            Assert.Equal("#FFFFFF", one.GetProperty("properties").GetProperty("textColor").GetString());
        }

        [Fact(DisplayName = "Subway lines - A, whose two features are merged into one")]
        public void GetLines_MergesAServiceWithTwoFeatures()
        {
            var feature = Assert.Single(Features(_service.GetLinesGeoJson(["A"], null, 0)));

            Assert.Equal("A", feature.GetProperty("properties").GetProperty("service").GetString());
        }

        [Fact(DisplayName = "Subway lines - the 1 clipped to Manhattan stays inside Manhattan")]
        public void GetLines_ClipsToTheBorough()
        {
            var manhattan = ManhattanOutline();
            var fullLine = Coordinates(Assert.Single(Features(_service.GetLinesGeoJson(["1"], null, 0)))).ToList();
            var clipped = Coordinates(Assert.Single(Features(_service.GetLinesGeoJson(["1"], Borough.MN, 0)))).ToList();

            // The 1 runs on to Van Cortlandt Park-242 St in the Bronx
            Assert.Contains(fullLine, c => manhattan.Distance(new Point(c)) > 0.001);
            Assert.True(fullLine.Max(c => c.Y) > 40.885);

            // Inside Manhattan (Marble Hill included), within the 6-decimal rounding
            Assert.NotEmpty(clipped);
            Assert.All(clipped, c => Assert.True(manhattan.Distance(new Point(c)) < 1e-5, $"{c} is outside Manhattan"));
            Assert.True(clipped.Max(c => c.Y) < 40.88);
            Assert.True(clipped.Min(c => c.Y) > 40.70);
        }

        [Fact(DisplayName = "Subway lines - a service with nothing left in the borough is left out")]
        public void GetLines_LeavesOutAServiceOutsideTheBorough()
        {
            var features = Features(_service.GetLinesGeoJson(["1", "SIR"], Borough.SI, 5));

            Assert.Equal("SIR", Assert.Single(features).GetProperty("properties").GetProperty("service").GetString());
        }

        [Fact(DisplayName = "Subway lines - simplifying removes vertices")]
        public void GetLines_SimplifyReducesVertexCount()
        {
            int Vertices(double simplify) => Coordinates(Assert.Single(Features(_service.GetLinesGeoJson(["A"], null, simplify)))).Count();

            var none = Vertices(0);
            var five = Vertices(5);
            var fifty = Vertices(50);

            Assert.True(five < none, $"5 m: {five}, none: {none}");
            Assert.True(fifty < five, $"50 m: {fifty}, 5 m: {five}");
        }

        [Fact(DisplayName = "Subway lines - coordinates have at most 6 decimals")]
        public void GetLines_RoundsCoordinates()
        {
            var coordinates = Coordinates(Assert.Single(Features(_service.GetLinesGeoJson(["7"], Borough.QN, 5)))).ToList();

            Assert.All(coordinates, c =>
            {
                Assert.Equal(Math.Round(c.X, 6), c.X);
                Assert.Equal(Math.Round(c.Y, 6), c.Y);
            });
        }

        [Fact(DisplayName = "Subway lines - a request is cached")]
        public void GetLines_CachesTheResult()
        {
            var first = _service.GetLinesGeoJson(["L"], Borough.BK, 5);
            var second = _service.GetLinesGeoJson(["L"], Borough.BK, 5);

            Assert.Same(first, second);
            Assert.NotSame(first, _service.GetLinesGeoJson(["L"], Borough.BK, 10));
        }

        [Fact(DisplayName = "Subway lines - every borough option keeps a service there and drops the rest")]
        public void GetLines_EachBoroughKeepsItsServices()
        {
            string[] Services(Borough? borough) => Features(_service.GetLinesGeoJson(["1", "7", "G", "SIR"], borough, 5))
                .Select(f => f.GetProperty("properties").GetProperty("service").GetString()!)
                .ToArray();

            Assert.Equal(["1", "7", "G", "SIR"], Services(null));
            Assert.Equal(["1", "7"], Services(Borough.MN));
            Assert.Equal(["1"], Services(Borough.BX));
            Assert.Equal(["G"], Services(Borough.BK));
            Assert.Equal(["7", "G"], Services(Borough.QN));
            Assert.Equal(["SIR"], Services(Borough.SI));
        }

        [Fact(DisplayName = "Subway lines - no services gives an empty FeatureCollection")]
        public void GetLines_NoServices()
        {
            Assert.Empty(Features(_service.GetLinesGeoJson([], Borough.MN, 5)));
        }

        [Fact(DisplayName = "Subway lines - a code not in the service lines is skipped")]
        public void GetLines_SkipsUnresolvedCodes()
        {
            var features = Features(_service.GetLinesGeoJson(["X", "L"], null, 5));

            Assert.Equal("L", Assert.Single(features).GetProperty("properties").GetProperty("service").GetString());
        }

        [Theory(DisplayName = "Subway lines - simplify outside 0 to 50 metres is an error")]
        [InlineData(-0.1)]
        [InlineData(50.1)]
        public void GetLines_RejectsSimplifyOutOfRange(double simplify)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _service.GetLinesGeoJson(["1"], null, simplify));
        }

        [Fact(DisplayName = "Subway lines - with simplify 0, lines keep the source's vertices, only rounded and deduplicated")]
        public void GetLines_SimplifyZeroKeepsTheSourceVertices()
        {
            var source = _fixture.SubwayServiceLinesService.GetFeatures()
                .Where(f => (string)f.Attributes["service"] == "L")
                .Sum(f => f.Geometry.NumPoints);

            var served = Coordinates(Assert.Single(Features(_service.GetLinesGeoJson(["L"], null, 0)))).Count();

            // Merging pieces shares their end points, and rounding can merge neighboring points, so never more
            Assert.InRange(served, source * 9 / 10, source);
        }

        [Fact(DisplayName = "Subway lines - S on its own resolves to the three shuttles, which draw in their boroughs")]
        public void GetLines_ShuttleCode()
        {
            var (services, unknown) = _service.ResolveServices(["s"]);
            Assert.Equal(["SF", "SR", "ST"], services);
            Assert.Empty(unknown);

            var features = Features(_service.GetLinesGeoJson(services, null, 5));
            Assert.Equal(["SF", "SR", "ST"], features.Select(f => f.GetProperty("properties").GetProperty("service").GetString()));
            Assert.All(features, f => Assert.Equal("#808183", f.GetProperty("properties").GetProperty("color").GetString()));
            Assert.Equal(["ST"], Features(_service.GetLinesGeoJson(services, Borough.MN, 5)).Select(f => f.GetProperty("properties").GetProperty("service").GetString()));
        }

        [Fact(DisplayName = "Subway lines - blank codes are ignored, and only blanks means every service")]
        public void ResolveServices_IgnoresBlankCodes()
        {
            var (services, unknown) = _service.ResolveServices(["", " ", "Z", "z"]);
            Assert.Equal(["Z"], services);
            Assert.Empty(unknown);

            Assert.Equal(27, _service.ResolveServices(["", " "]).Services.Count);
        }

        [Fact(DisplayName = "Subway lines - the service list is read once and then cached")]
        public void GetServices_IsCached()
        {
            Assert.Same(_service.GetServices(), _service.GetServices());
        }

        [Fact(DisplayName = "Subway service lines shape - attribute lookup as a FeatureCollection")]
        public void ServiceLinesShape_GetFeatureCollection()
        {
            var collection = _fixture.SubwayServiceLinesService.GetFeatureCollection([new("Service", "A")]);

            // A has two features in the source
            Assert.Equal(2, collection.Count);
            Assert.All(collection, f =>
            {
                Assert.Equal("A", f.Attributes["Service"]);
                Assert.Equal("8 Avenue Express", f.Attributes["ServiceName"]);
                Assert.Equal("blue", f.Attributes["ShapeColor"]);
                Assert.IsType<MultiLineString>(f.Geometry, exactMatch: false);
            });
        }

        [Fact(DisplayName = "Subway service lines shape - every feature maps")]
        public void ServiceLinesShape_MapsEveryFeature()
        {
            var shapes = _fixture.SubwayServiceLinesService.GetFeatureList().ToList();

            Assert.Equal(29, shapes.Count);
            Assert.Equal(Enumerable.Range(1, 31).Except([28, 29]), shapes.Select(s => s.ObjectId).Order());
            Assert.All(shapes, s =>
            {
                Assert.False(string.IsNullOrEmpty(s.Service));
                Assert.False(string.IsNullOrEmpty(s.ServiceName));
                Assert.True(s.ShapeLength > 0);
                Assert.NotNull(s.BoundingBox);
            });
        }

        [Fact(DisplayName = "Subway route service - its dependencies are required")]
        public void Constructor_RequiresDependencies()
        {
            var lines = _fixture.SubwayServiceLinesService;
            var boroughs = _fixture.ServiceProvider.GetRequiredService<BoroughBoundariesService>();

            Assert.Throws<ArgumentNullException>(() => new SubwayRouteService(null!, boroughs, _cache));
            Assert.Throws<ArgumentNullException>(() => new SubwayRouteService(lines, null!, _cache));
            Assert.Throws<ArgumentNullException>(() => new SubwayRouteService(lines, boroughs, null!));
            Assert.Throws<ArgumentNullException>(() => _service.GetLinesGeoJson(null!, null, 5));
        }

        private Geometry ManhattanOutline()
        {
            var feature = _fixture.BoroughBoundariesService.GetFeatures().Single(f => Convert.ToInt32(f.Attributes["BoroCode"]) == 1);
            return GeoTransformationHelper.CopyNad83ToWgs84(feature.Geometry);
        }

        private static List<JsonElement> Features(string geoJson)
        {
            using var document = JsonDocument.Parse(geoJson);
            Assert.Equal("FeatureCollection", document.RootElement.GetProperty("type").GetString());
            return document.RootElement.GetProperty("features").EnumerateArray().Select(f => f.Clone()).ToList();
        }

        private static IEnumerable<Coordinate> Coordinates(JsonElement feature) =>
            feature.GetProperty("geometry").GetProperty("coordinates").EnumerateArray()
                .SelectMany(line => line.EnumerateArray())
                .Select(position => new Coordinate(position[0].GetDouble(), position[1].GetDouble()));
    }
}
