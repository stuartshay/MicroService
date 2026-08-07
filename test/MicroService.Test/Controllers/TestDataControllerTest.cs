using MicroService.Data.Models;
using MicroService.Data.Repository;
using MicroService.WebApi.V1.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace MicroService.Test.Controllers
{
    public class TestDataControllerTest
    {
        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenRepositoryIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new TestDataController(null!));
        }

        [Fact]
        public async Task Get_ReturnsOkResult_WithData()
        {
            var expected = new List<TestData> { new() { Id = 1, Data = 1d } };
            var repositoryMock = new Mock<ITestDataRepository>();
            repositoryMock.Setup(r => r.FindAll()).ReturnsAsync(expected);

            var controller = new TestDataController(repositoryMock.Object);

            var result = await controller.Get();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(expected, okResult.Value);
        }

        [Fact]
        public async Task Get_ReturnsNotFound_WhenRepositoryReturnsNull()
        {
            IEnumerable<TestData>? nullResult = null;
            var repositoryMock = new Mock<ITestDataRepository>();
            repositoryMock.Setup(r => r.FindAll()).ReturnsAsync(nullResult!);

            var controller = new TestDataController(repositoryMock.Object);

            var result = await controller.Get();

            Assert.IsType<NotFoundResult>(result.Result);
        }
    }
}
