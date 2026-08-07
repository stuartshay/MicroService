using MicroService.Service.Interfaces;
using MicroService.WebApi.V1.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace MicroService.Test.Controllers
{
    public class PercentileControllerTest
    {
        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenCalculationServiceIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new PercentileController(null!));
        }

        [Fact]
        public async Task GetPercentile_ReturnsOkResult_WhenResultIsAtLeastFifteen()
        {
            var calculationServiceMock = new Mock<ICalculationService>();
            calculationServiceMock.Setup(s => s.CalculatePercentile(It.IsAny<double>())).ReturnsAsync(42d);

            var controller = new PercentileController(calculationServiceMock.Object);

            var result = await controller.GetPercentile();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(42d, okResult.Value);
        }

        [Fact]
        public async Task GetPercentile_ReturnsNotFound_WhenResultIsBelowFifteen()
        {
            var calculationServiceMock = new Mock<ICalculationService>();
            calculationServiceMock.Setup(s => s.CalculatePercentile(It.IsAny<double>())).ReturnsAsync(1d);

            var controller = new PercentileController(calculationServiceMock.Object);

            var result = await controller.GetPercentile();

            Assert.IsType<NotFoundResult>(result.Result);
        }
    }
}
