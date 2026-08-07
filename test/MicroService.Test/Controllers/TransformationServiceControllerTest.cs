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

            Assert.IsType<OkObjectResult>(result.Result);
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

            Assert.IsType<OkObjectResult>(result.Result);
        }
    }
}
