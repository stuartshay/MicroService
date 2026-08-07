using AutoMapper;
using MicroService.Service.Mappings;
using MicroService.Service.Models;
using MicroService.Service.Services.Base;
using Microsoft.Extensions.Logging;
using Xunit;

namespace MicroService.Test.Unit
{
    public class AbstractShapeServiceTest
    {
        private readonly TestShapeService _service = new();

        [Fact]
        public void MatchAttributeValue_MatchesDoubleWithinTolerance()
        {
            const double value = 40.7128000005;

            var result = TestShapeService.Match(value, 40.7128);

            Assert.Equal(value, result);
        }

        [Fact]
        public void MatchAttributeValue_RejectsDoubleOutsideTolerance()
        {
            var result = TestShapeService.Match(40.712800002, 40.7128);

            Assert.Null(result);
        }

        [Theory]
        [InlineData(42, 43)]
        [InlineData(42, 43d)]
        [InlineData(42d, 43)]
        public void MatchAttributeValue_ReturnsNullForNumericMismatch(object value, object expectedValue)
        {
            var result = TestShapeService.Match(value, expectedValue);

            Assert.Null(result);
        }

        [Fact]
        public void MatchAttributeValue_MatchesIntAgainstEqualInt()
        {
            var result = TestShapeService.Match(42, 42);

            Assert.Equal(42, result);
        }

        [Fact]
        public void MatchAttributeValue_MatchesIntAgainstEqualDouble()
        {
            var result = TestShapeService.Match(42, 42d);

            Assert.Equal(42, result);
        }

        [Fact]
        public void GetFeatureLookup_ReturnsEmptySequenceForNullAttributes()
        {
            var result = _service.GetFeatureLookup(null);

            Assert.Empty(result);
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new TestShapeService(logger: null!, mapper: Moq.Mock.Of<IMapper>()));
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenMapperIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new TestShapeService(logger: Moq.Mock.Of<ILogger>(), mapper: null!));
        }

        [Fact]
        public void ValidateFeatureKey_ReturnsNull_ForNullAttributes()
        {
            var result = _service.ValidateFeatureKey(null);

            Assert.Null(result);
        }

        [Fact]
        public void ValidateFeatureKey_SkipsUnknownProperty()
        {
            var attributes = new List<KeyValuePair<string, object>>
            {
                new("NotARealProperty", "value"),
            };

            var result = _service.ValidateFeatureKey(attributes);

            Assert.NotNull(result);
            Assert.Equal("NotARealProperty", result[0].Key);
        }

        [Fact]
        public void ValidateFeatureKey_ConvertsMismatchedIntValue()
        {
            var attributes = new List<KeyValuePair<string, object>>
            {
                new("BoroCode", "205"),
            };

            var result = _service.ValidateFeatureKey(attributes);

            Assert.NotNull(result);
            Assert.Equal(205, result[0].Value);
        }

        [Fact]
        public void ValidateFeatureKey_KeepsValueUnchanged_WhenTypeAlreadyMatches()
        {
            var attributes = new List<KeyValuePair<string, object>>
            {
                new("BoroCode", 205),
            };

            var result = _service.ValidateFeatureKey(attributes);

            Assert.NotNull(result);
            Assert.Equal(205, result[0].Value);
        }

        [Fact]
        public void ValidateFeatureKey_ConvertsMismatchedDoubleValue()
        {
            var attributes = new List<KeyValuePair<string, object>>
            {
                new("ShapeArea", "12.5"),
            };

            var result = _service.ValidateFeatureKey(attributes);

            Assert.NotNull(result);
            Assert.Equal(12.5, result[0].Value);
        }

        [Fact]
        public void ValidateFeatureKey_ThrowsNotSupportedException_WhenTargetTypeIsUnsupported()
        {
            var attributes = new List<KeyValuePair<string, object>>
            {
                new("BoundingBox", "not-a-bounding-box"),
            };

            Assert.Throws<NotSupportedException>(() => _service.ValidateFeatureKey(attributes));
        }

        [Fact]
        public void ValidateFeatureKey_ConvertsMismatchedStringValue()
        {
            var attributes = new List<KeyValuePair<string, object>>
            {
                new("BoroName", 42),
            };

            var result = _service.ValidateFeatureKey(attributes);

            Assert.NotNull(result);
            Assert.Equal("42", result[0].Value);
        }

        [Fact]
        public void ValidateFeatureKey_ThrowsFormatException_WhenIntConversionFails()
        {
            var attributes = new List<KeyValuePair<string, object>>
            {
                new("BoroCode", "not-a-number"),
            };

            Assert.Throws<FormatException>(() => _service.ValidateFeatureKey(attributes));
        }

        [Fact]
        public void ValidateFeatureKey_ThrowsFormatException_WhenDoubleConversionFails()
        {
            var attributes = new List<KeyValuePair<string, object>>
            {
                new("ShapeArea", "not-a-number"),
            };

            Assert.Throws<FormatException>(() => _service.ValidateFeatureKey(attributes));
        }

        [Fact]
        public void GetFeatureName_ReturnsNull_WhenPropertyHasNoFeatureNameAttribute()
        {
            var result = _service.GetFeatureName(nameof(BoroughBoundaryShape.ShapeArea));

            Assert.Null(result);
        }

        [Fact]
        public void MatchAttributeValue_ReturnsNull_WhenIntComparedAgainstNonNumericExpectedValue()
        {
            var result = TestShapeService.Match(1, "not-a-number");

            Assert.Null(result);
        }

        [Fact]
        public void MatchAttributeValue_ReturnsNull_WhenDoubleComparedAgainstNonNumericExpectedValue()
        {
            var result = TestShapeService.Match(1.5, "not-a-number");

            Assert.Null(result);
        }

        private sealed class TestShapeService : AbstractShapeService<BoroughBoundaryShape, BoroughBoundaryShapeProfile>
        {
            public TestShapeService()
                : base(Moq.Mock.Of<ILogger>(), Moq.Mock.Of<IMapper>())
            {
            }

            public TestShapeService(ILogger logger, IMapper mapper)
                : base(logger, mapper)
            {
            }

            public static object? Match(object value, object expectedValue)
            {
                return MatchAttributeValue(value, expectedValue);
            }
        }
    }
}
