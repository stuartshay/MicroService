using MicroService.Service.Mappings.Base;
using MicroService.Service.Models.Base;
using NetTopologySuite.Features;
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
    }
}
