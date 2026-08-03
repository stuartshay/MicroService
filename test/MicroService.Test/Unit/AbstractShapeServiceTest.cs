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

            var result = _service.Match(value, 40.7128);

            Assert.Equal(value, result);
        }

        [Fact]
        public void MatchAttributeValue_RejectsDoubleOutsideTolerance()
        {
            var result = _service.Match(40.712800002, 40.7128);

            Assert.Null(result);
        }

        [Theory]
        [InlineData(42, 43)]
        [InlineData(42, 43d)]
        [InlineData(42d, 43)]
        public void MatchAttributeValue_ReturnsNullForNumericMismatch(object value, object expectedValue)
        {
            var result = _service.Match(value, expectedValue);

            Assert.Null(result);
        }

        [Fact]
        public void GetFeatureLookup_ReturnsEmptySequenceForNullAttributes()
        {
            var result = _service.GetFeatureLookup(null);

            Assert.Empty(result);
        }

        private sealed class TestShapeService : AbstractShapeService<BoroughBoundaryShape, BoroughBoundaryShapeProfile>
        {
            public TestShapeService()
                : base(Moq.Mock.Of<ILogger>(), Moq.Mock.Of<IMapper>())
            {
            }

            public object? Match(object value, object expectedValue)
            {
                return MatchAttributeValue(value, expectedValue);
            }
        }
    }
}
