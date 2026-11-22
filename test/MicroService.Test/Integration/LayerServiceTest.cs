using MicroService.Service.Services.Base;
using MicroService.Service.Services.Layers;
using MicroService.Test.Fixture;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using Xunit;

namespace MicroService.Test.Integration
{
    [Trait("Category", "Integration")]
    public class LayerServiceTest : IClassFixture<ShapeServiceFixture>
    {
        private readonly ShapeServiceFixture _fixture;
        private readonly LayerService _service;

        public LayerServiceTest(ShapeServiceFixture fixture)
        {
            _fixture = fixture;
            _service = new LayerService(fixture.ServiceProvider.GetRequiredService<ShapefileDataReaderResolver>());
        }

        public static TheoryData<string> Layers => new(LayerCatalog.All.Select(l => l.Key));

        [Theory(DisplayName = "Layers - every layer is a registered shape whose title and detail fields exist")]
        [MemberData(nameof(Layers))]
        public void EveryLayer_ReadsFieldsTheShapeHas(string key)
        {
            var layer = LayerCatalog.Find(key)!;
            var feature = _fixture.ServiceProvider.GetRequiredService<ShapefileDataReaderResolver>()(key).GetFeatures().First();

            Assert.True(feature.Attributes.Exists(layer.TitleField), $"{key} has no {layer.TitleField}");
            Assert.All(layer.Details, d => Assert.True(feature.Attributes.Exists(d.Field), $"{key} has no {d.Field}"));
            Assert.Matches("^#[0-9A-F]{6}$", layer.Color);
            // The service draws shapes as they are stored, so a layer must be WGS84
            Assert.Equal(MicroService.Service.Models.Enum.Attributes.Datum.Wgs84, ShapeRegistry.GetByKey(layer.Shape).Datum);
            Assert.True(_service.Count(layer) > 0);
        }

        [Fact(DisplayName = "Layers - a box returns the features that touch it, with a title and labelled details")]
        public void HistoricDistricts_InABox()
        {
            using var document = Features("HistoricDistricts", new LayerBox(-73.9990, 40.6930, -73.9930, 40.6970));

            var brooklynHeights = document.RootElement.GetProperty("features").EnumerateArray()
                .Single(f => f.GetProperty("properties").GetProperty("title").GetString() == "Brooklyn Heights Historic District");
            Assert.Equal("LP-00099", brooklynHeights.GetProperty("properties").GetProperty("details").GetProperty("LP number").GetString());
            Assert.Equal("Polygon", brooklynHeights.GetProperty("geometry").GetProperty("type").GetString());
        }

        [Fact(DisplayName = "Layers - points come back as points, rounded to 6 decimals, and only inside the box")]
        public void Libraries_InABox()
        {
            using var document = Features("Libraries", new LayerBox(-74.0010, 40.7330, -73.9970, 40.7360));

            var feature = Assert.Single(document.RootElement.GetProperty("features").EnumerateArray());
            Assert.Equal("Jefferson Market", feature.GetProperty("properties").GetProperty("title").GetString());
            Assert.Equal("NYPL", feature.GetProperty("properties").GetProperty("details").GetProperty("System").GetString());
            var coordinates = feature.GetProperty("geometry").GetProperty("coordinates");
            Assert.Equal(Math.Round(coordinates[0].GetDouble(), 6), coordinates[0].GetDouble());
        }

        [Fact(DisplayName = "Layers - polygons are simplified, and a feature without details has none")]
        public void Boroughs_AreSimplified()
        {
            using var document = Features("BoroughBoundaries", new LayerBox(-74.02, 40.70, -73.93, 40.88));

            var manhattan = document.RootElement.GetProperty("features").EnumerateArray()
                .Single(f => f.GetProperty("properties").GetProperty("title").GetString() == "Manhattan");
            Assert.False(manhattan.GetProperty("properties").TryGetProperty("details", out _));
            var drawn = manhattan.GetProperty("geometry").GetProperty("coordinates").EnumerateArray().Sum(p => p.EnumerateArray().Sum(r => r.GetArrayLength()));
            var stored = _fixture.BoroughBoundariesService.GetFeatures().Single(f => Convert.ToInt32(f.Attributes["BoroCode"]) == 1).Geometry.NumPoints;
            Assert.InRange(drawn, 1, stored - 1);
        }

        private JsonDocument Features(string key, LayerBox box) => JsonDocument.Parse(_service.GetGeoJson(LayerCatalog.Find(key)!, box));
    }
}
