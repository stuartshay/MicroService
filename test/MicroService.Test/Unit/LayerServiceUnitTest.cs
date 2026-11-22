using MicroService.Service.Helpers;
using MicroService.Service.Services.Layers;
using Moq;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using System.Text.Json;
using Xunit;

namespace MicroService.Test.Unit
{
    public class LayerServiceUnitTest
    {
        private static readonly GeometryFactory Factory = new();

        [Fact(DisplayName = "Layer service - needs its readers")]
        public void Constructor_NeedsReaders() => Assert.Throws<ArgumentNullException>(() => new LayerService(null!));

        [Fact(DisplayName = "Layer service - a missing or empty detail is left out, and the title is trimmed")]
        public void Properties_SkipMissingAndEmptyDetails()
        {
            var feature = new Feature(Factory.CreatePoint(new Coordinate(-73.99, 40.73)),
                new AttributesTable { { "name", "  Jefferson Market " }, { "system", "" }, { "url", null } });
            var reader = new Mock<IShapefileDataReaderService>();
            reader.Setup(r => r.GetFeatures()).Returns(new List<Feature> { feature });
            var service = new LayerService(_ => reader.Object);

            using var document = JsonDocument.Parse(service.GetGeoJson(LayerCatalog.Find("Libraries")!, new LayerBox(-74.0, 40.72, -73.98, 40.74)));

            var properties = Assert.Single(document.RootElement.GetProperty("features").EnumerateArray()).GetProperty("properties");
            Assert.Equal("Jefferson Market", properties.GetProperty("title").GetString());
            Assert.False(properties.TryGetProperty("details", out _));
            Assert.Equal(1, service.Count(LayerCatalog.Find("Libraries")!));
        }
    }
}
