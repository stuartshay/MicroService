using MicroService.Service.Services.Layers;
using Xunit;

namespace MicroService.Test.Unit
{
    public class LayerBoxTest
    {
        [Fact(DisplayName = "Layer box - west,south,east,north in degrees")]
        public void Parse_ReadsTheBox()
        {
            var box = LayerBox.Parse(" -74.01, 40.70 ,-73.99,40.72", out var error);

            Assert.Null(error);
            Assert.Equal(new LayerBox(-74.01, 40.70, -73.99, 40.72), box);
        }

        [Theory(DisplayName = "Layer box - malformed, inverted, outside NYC or too big is an error")]
        [InlineData(null, "west,south,east,north")]
        [InlineData("-74,40.7,-73.9", "west,south,east,north")]
        [InlineData("a,b,c,d", "west,south,east,north")]
        [InlineData("-73.9,40.7,-74.0,40.8", "less than")]
        [InlineData("-73.9,40.8,-73.8,40.7", "less than")]
        [InlineData("-118.3,34.0,-118.2,34.1", "New York City area")]
        [InlineData("-74.2,40.5,-73.6,40.9", "at most 0.5")]
        [InlineData("NaN,40.7,-73.9,40.8", "west,south,east,north")]
        public void Parse_RejectsBadBoxes(string? text, string message)
        {
            Assert.Null(LayerBox.Parse(text, out var error));
            Assert.Contains(message, error, StringComparison.Ordinal);
        }

        [Fact(DisplayName = "Layer catalog - finds a layer in any case, and nothing else")]
        public void Catalog_Find()
        {
            Assert.Equal("Libraries", LayerCatalog.Find(" libraries ")?.Key);
            Assert.Null(LayerCatalog.Find("Subway"));
            Assert.Null(LayerCatalog.Find(null));
            Assert.Equal(7, LayerCatalog.All.Count);
            Assert.Equal(7, LayerCatalog.All.Select(l => l.Color).Distinct().Count());
        }
    }
}
