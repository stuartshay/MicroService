using MicroService.Service.Mappings;
using MicroService.Service.Mappings.Base;
using MicroService.Service.Models;
using MicroService.Service.Models.Base;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using Xunit;

namespace MicroService.Test.Mappings.Base
{
    public class ShapeProfileTest
    {
        private sealed class TestShapeProfile : ShapeProfile<ShapeBase>
        {
            public static string CallGetString(Feature src, string key) => GetString(src, key);

            public static double CallGetDouble(Feature src, string key) => GetDouble(src, key);

            public static string? CallGetSanitizedString(Feature src, string key) => GetSanitizedString(src, key);

            public static BoundingBox CallMapBoundingBox(Envelope? envelope) => MapBoundingBox(envelope);
        }

        [Fact]
        public void GetString_ReturnsEmpty_WhenAttributeIsNull()
        {
            var feature = new Feature(null, new AttributesTable(new Dictionary<string, object?> { { "key", null } }!));

            var result = TestShapeProfile.CallGetString(feature, "key");

            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void GetString_ReturnsValue_WhenAttributeIsPresent()
        {
            var feature = new Feature(null, new AttributesTable(new Dictionary<string, object> { { "key", "value" } }));

            var result = TestShapeProfile.CallGetString(feature, "key");

            Assert.Equal("value", result);
        }

        [Fact]
        public void GetDouble_ReturnsZero_WhenAttributeIsNull()
        {
            var feature = new Feature(null, new AttributesTable(new Dictionary<string, object?> { { "key", null } }!));

            var result = TestShapeProfile.CallGetDouble(feature, "key");

            Assert.Equal(0, result);
        }

        [Fact]
        public void GetDouble_ReturnsParsedValue_WhenAttributeIsPresent()
        {
            var feature = new Feature(null, new AttributesTable(new Dictionary<string, object> { { "key", "12.5" } }));

            var result = TestShapeProfile.CallGetDouble(feature, "key");

            Assert.Equal(12.5, result);
        }

        [Fact]
        public void GetSanitizedString_ReturnsNull_WhenAttributeIsNull()
        {
            var feature = new Feature(null, new AttributesTable(new Dictionary<string, object?> { { "key", null } }!));

            var result = TestShapeProfile.CallGetSanitizedString(feature, "key");

            Assert.Null(result);
        }

        [Fact]
        public void GetSanitizedString_StripsNullCharacters()
        {
            var value = "abc" + '\u0000' + "def";
            var feature = new Feature(null, new AttributesTable(new Dictionary<string, object> { { "key", value } }));

            var result = TestShapeProfile.CallGetSanitizedString(feature, "key");

            Assert.Equal("abcdef", result);
        }

        [Fact]
        public void ToDictionary_IncludesPublicPropertiesAndExcludesFeatureCollectionExcludeProperties()
        {
            var shape = new BoroughBoundaryShape
            {
                BoroCode = 1,
                BoroName = "Manhattan",
                ShapeArea = 100.5,
                ShapeLength = 50.25,
                BoundingBox = new BoundingBox { MinX = 1, MaxX = 2, MinY = 3, MaxY = 4 },
                Geometry = new Point(1, 2),
                Feature = new FeatureCollection(),
            };

            var dictionary = BoroughBoundaryShapeProfile.ToDictionary(shape);

            Assert.Equal(1, dictionary[nameof(BoroughBoundaryShape.BoroCode)]);
            Assert.Equal("Manhattan", dictionary[nameof(BoroughBoundaryShape.BoroName)]);
            Assert.Equal(100.5, dictionary[nameof(BoroughBoundaryShape.ShapeArea)]);
            Assert.Equal(50.25, dictionary[nameof(BoroughBoundaryShape.ShapeLength)]);
            Assert.False(dictionary.ContainsKey(nameof(BoroughBoundaryShape.BoundingBox)));
            Assert.False(dictionary.ContainsKey(nameof(BoroughBoundaryShape.Geometry)));
            Assert.False(dictionary.ContainsKey(nameof(BoroughBoundaryShape.Feature)));
        }

        [Fact]
        public void ToDictionary_ThrowsArgumentNullException_WhenShapeIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => BoroughBoundaryShapeProfile.ToDictionary(null!));
        }

        [Fact]
        public void MapBoundingBox_ReturnsNull_WhenEnvelopeIsNull()
        {
            Assert.Null(TestShapeProfile.CallMapBoundingBox(null));
        }

        [Fact]
        public void MapBoundingBox_MapsEnvelopeAndCentre()
        {
            var envelope = new Envelope(-74.0, -73.9, 40.7, 40.8);

            var box = TestShapeProfile.CallMapBoundingBox(envelope);

            Assert.NotNull(box);
            Assert.Equal(envelope.MinX, box.MinX);
            Assert.Equal(envelope.MaxX, box.MaxX);
            Assert.Equal(envelope.MinY, box.MinY);
            Assert.Equal(envelope.MaxY, box.MaxY);
            Assert.Equal(envelope.Area, box.Area);
            Assert.Equal(envelope.Diameter, box.Diameter);
            Assert.Equal(envelope.MinExtent, box.MinExtent);
            Assert.Equal(envelope.MaxExtent, box.MaxExtent);
            Assert.NotNull(box.Centre);
            Assert.Equal(envelope.Centre.X, box.Centre.X);
            Assert.Equal(envelope.Centre.Y, box.Centre.Y);
        }
    }
}
