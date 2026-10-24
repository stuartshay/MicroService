using MicroService.Service.Helpers;
using Microsoft.Extensions.Caching.Memory;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Esri;
using Xunit;

namespace MicroService.Test.Unit
{
    [Trait("Category", "Unit")]
    public sealed class ShapefileSourceTest : IDisposable
    {
        private readonly string _root = Path.Join(Path.GetTempPath(), "shape-reader-" + Guid.NewGuid().ToString("N"));

        [Fact]
        public void NumericValuesRemainDouble_WhileMetadataKeepsTheDeclaredIntegerType()
        {
            var path = WritePoint();
            var (_, database) = ShapefileSource.ReadMetadata(path);
            var field = Assert.Single(database.Fields);
            Assert.Equal(typeof(int), field.Type);
            var feature = Assert.Single(ShapefileSource.ReadFeatures(path));
            Assert.Equal(42d, Assert.IsType<double>(feature.Attributes["number"]));
        }

        [Fact]
        public void EvictedCache_ReopensTheReaderAndReturnsEveryRecordAgain()
        {
            var path = WritePoint();
            using var cache = new MemoryCache(new MemoryCacheOptions());
            var reader = new CachedShapefileDataReader(cache, "sample", path);
            var first = reader.GetFeatures();
            Assert.Same(first, reader.GetFeatures());
            cache.Remove("sample");
            var refreshed = reader.GetFeatures();
            Assert.NotSame(first, refreshed);
            var feature = Assert.Single(refreshed);
            Assert.Equal(new Point(1, 2), feature.Geometry);
        }

        private string WritePoint()
        {
            Directory.CreateDirectory(_root);
            var path = Path.Join(_root, "sample.shp");
            Shapefile.WriteAllFeatures([new Feature(new Point(1, 2), new AttributesTable { { "number", 42 } })], path);
            return path;
        }

        public void Dispose() => Directory.Delete(_root, recursive: true);
    }
}
