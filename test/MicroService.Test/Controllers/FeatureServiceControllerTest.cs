using MicroService.Service.Interfaces;
using MicroService.Service.Models;
using MicroService.Service.Models.Base;
using MicroService.WebApi.Models;
using MicroService.WebApi.V1.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MicroService.Test.Controllers
{
    public class FeatureServiceControllerTests
    {
        public static IEnumerable<object[]> ValidRequests =>
            new List<object[]>
            {
            new object[] { "BoroughBoundaries", new List<ShapeBase> { new BoroughBoundaryShape(), new CommunityDistrictShape() } },
            new object[] { "CommunityDistricts", new List<ShapeBase> { new CommunityDistrictShape(), new CommunityDistrictShape() } }
            };

        [Theory]
        [MemberData(nameof(ValidRequests))]
        public async Task GetFeatureList_WithValidRequest_ReturnsOkResult(string key, List<ShapeBase> expectedResults)
        {
            // Arrange
            var request = new FeatureAttributeRequestModel { Key = key };

            var shapeServiceMock = new Mock<IShapeService<ShapeBase>>();
            shapeServiceMock.Setup(s => s.GetFeatureAttributes()).Returns(expectedResults);

            var shapeServiceResolver = new Mock<ShapeServiceResolver?>();
            shapeServiceResolver.Setup(r => r!(request.Key)).Returns(shapeServiceMock.Object);

            var controller = GetFeatureServiceController(shapeServiceResolver.Object, shapeServiceMock.Object);

            // Act
            var result = await controller.GetFeatureList(request);

            // Assert
            Assert.IsType<OkObjectResult>(result.Result);
            var okResult = (OkObjectResult)result.Result;
            Assert.Equal(expectedResults, okResult.Value);
        }

        [InlineData("InvalidKey")]
        [Theory]
        public async Task GetFeatureList_WithInvalidRequest_ReturnsBadRequestResult(string key)
        {
            // Arrange
            var request = new FeatureAttributeRequestModel { Key = key };

            // Act
            var controller = GetFeatureServiceController();
            var result = await controller.GetFeatureList(request);

            // Assert
            Assert.IsType<BadRequestResult>(result.Result);
        }

        private FeatureServiceController GetFeatureServiceController(ShapeServiceResolver? resolver = null,
            IShapeService<ShapeBase>? shapeService = null)
        {

            ILogger<FeatureServiceController> logger = new Mock<ILogger<FeatureServiceController>>().Object;
            resolver ??= new Mock<ShapeServiceResolver?>().Object;


            return new FeatureServiceController(resolver, logger);
        }
    }
}


