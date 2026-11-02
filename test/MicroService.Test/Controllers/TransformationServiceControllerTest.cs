using MicroService.WebApi.V1.Controllers;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace MicroService.Test.Controllers
{
    public class TransformationServiceControllerTest
    {
        private readonly TransformationServiceController _controller = new();

        [Fact]
        public void ConvertWgs84ToNad83_ReturnsNoContent_WhenLatitudeIsZero()
        {
            var result = _controller.ConvertWgs84ToNad83(0, -73.8832294373166);

            Assert.IsType<NoContentResult>(result.Result);
        }

        [Fact]
        public void ConvertWgs84ToNad83_ReturnsNoContent_WhenLongitudeIsZero()
        {
            var result = _controller.ConvertWgs84ToNad83(40.681939660888951, 0);

            Assert.IsType<NoContentResult>(result.Result);
        }

        [Fact]
        public void ConvertWgs84ToNad83_ReturnsOkResult_WithTransformedPoint()
        {
            var result = _controller.ConvertWgs84ToNad83(40.681939660888951, -73.8832294373166);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var (x, y) = GetXY(okResult.Value);
            Assert.NotNull(x);
            Assert.NotNull(y);
            Assert.NotEqual(0, x!.Value);
            Assert.NotEqual(0, y!.Value);
        }

        [Fact]
        public void ConvertNad83ToWgs84_ReturnsNoContent_WhenXIsZero()
        {
            var result = _controller.ConvertNad83ToWgs84(0, 187747.02946839959);

            Assert.IsType<NoContentResult>(result.Result);
        }

        [Fact]
        public void ConvertNad83ToWgs84_ReturnsNoContent_WhenYIsZero()
        {
            var result = _controller.ConvertNad83ToWgs84(1016636.9999607186, 0);

            Assert.IsType<NoContentResult>(result.Result);
        }

        [Fact]
        public void ConvertNad83ToWgs84_ReturnsOkResult_WithTransformedPoint()
        {
            var result = _controller.ConvertNad83ToWgs84(1016636.9999607186, 187747.02946839959);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var (x, y) = GetXY(okResult.Value);
            Assert.NotNull(x);
            Assert.NotNull(y);
            Assert.NotEqual(0, x!.Value);
            Assert.NotEqual(0, y!.Value);
        }

        [Theory]
        [InlineData(-0.0, -74.0060)]
        [InlineData(40.7128, -0.0)]
        public void ConvertWgs84ToNad83_ReturnsNoContent_WhenCoordinateIsNegativeZero(double latitude, double longitude)
        {
            var result = _controller.ConvertWgs84ToNad83(latitude, longitude);

            Assert.IsType<NoContentResult>(result.Result);
        }

        [Fact]
        public void ConvertWgs84ToNad83_ReturnsOkResult_ForNewYorkCoordinate()
        {
            var result = _controller.ConvertWgs84ToNad83(40.7128, -74.0060);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var (x, y) = GetXY(okResult.Value);
            Assert.NotNull(x);
            Assert.NotNull(y);
            Assert.NotEqual(0, x!.Value);
            Assert.NotEqual(0, y!.Value);
        }

        [Theory]
        [InlineData(-0.0, 187747.02946839959)]
        [InlineData(1016636.9999607186, -0.0)]
        public void ConvertNad83ToWgs84_ReturnsNoContent_WhenCoordinateIsNegativeZero(double x, double y)
        {
            var result = _controller.ConvertNad83ToWgs84(x, y);

            Assert.IsType<NoContentResult>(result.Result);
        }

        [Fact]
        public void ConvertNad83ToWgs84_ReturnsOkResult_ForNewYorkCoordinate()
        {
            // NAD83 / New York Long Island (feet) for roughly 40.7128, -74.0060
            var result = _controller.ConvertNad83ToWgs84(982000, 197000);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var (x, y) = GetXY(okResult.Value);
            Assert.NotNull(x);
            Assert.NotNull(y);
            Assert.InRange(x!.Value, -74.1, -73.9);
            Assert.InRange(y!.Value, 40.6, 40.8);
        }

        private static (double? X, double? Y) GetXY(object? value)
        {
            var type = value!.GetType();
            var x = (double?)type.GetProperty("X")!.GetValue(value);
            var y = (double?)type.GetProperty("Y")!.GetValue(value);
            return (x, y);
        }
    }
}
