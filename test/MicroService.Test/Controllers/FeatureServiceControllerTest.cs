using MicroService.Service.Interfaces;
using MicroService.Service.Models;
using MicroService.Service.Models.Base;
using MicroService.Service.Models.Enum;
using MicroService.WebApi.Models;
using MicroService.WebApi.V1.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using System.Text;
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
            shapeServiceMock.Setup(s => s.GetFeatureList()).Returns(expectedResults);

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

        [Fact]
        public void Get_ReturnsAvailableShapes()
        {
            // Arrange
            var controller = GetFeatureServiceController();

            // Act
            var sut = controller.Get();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(sut.Result);
            var shapesResult = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);

            Assert.All(shapesResult, Assert.NotNull);
        }

        [Fact]
        public void GetShapeProperties_ReturnsOkResult()
        {
            // Arrange
            var id = "BoroughBoundaries";
            var shapeServiceMock = new Mock<IShapeService<BoroughBoundaryShape>>();
            shapeServiceMock.Setup(s => s.GetShapeDatabaseProperties()).Returns(new DbaseFileHeader
            {
                NumFields = 4,
                NumRecords = 5,
                Encoding = Encoding.UTF32,
                LastUpdateDate = new DateTime(2022, 01, 01),
            });

            shapeServiceMock.Setup(x => x.GetShapeProperties()).Returns(new ShapefileHeader
            {
                Bounds = new Envelope(1, 2, 3, 4),
                ShapeType = ShapeGeometryType.Polygon,
            });

            var shapeServiceResolver = new Mock<ShapeServiceResolver?>();
            shapeServiceResolver.Setup(r => r!(id)).Returns(shapeServiceMock.Object);

            var controller = GetFeatureServiceController(shapeServiceResolver.Object, shapeServiceMock.Object);

            // Act
            var sut = controller.GetShapeProperties(id);

            // Assert
            Assert.IsType<OkObjectResult>(sut.Result);
        }


        [InlineData("InvalidKey")]
        [Theory]
        public void GetShapeProperties_ReturnsBadRequestResult(string key)
        {
            //Arrange
            var controller = GetFeatureServiceController(null, null);

            // Act
            var sut = controller.GetShapeProperties(key);

            // Assert
            Assert.IsType<BadRequestResult>(sut.Result);
        }

        [Fact]
        public async Task GetGeospatialLookup_ReturnsOkResult_WhenRequestIsValid()
        {
            // Arrange
            var id = "BoroughBoundaries";
            var shapeServiceMock = new Mock<IShapeService<BoroughBoundaryShape>>();
            shapeServiceMock.Setup(s => s.GetFeatureLookup(1, 1)).Returns(new BoroughBoundaryShape { BoroCode = 1 });

            var shapeServiceResolver = new Mock<ShapeServiceResolver?>();
            shapeServiceResolver.Setup(r => r!(id)).Returns(shapeServiceMock.Object);

            var controller = GetFeatureServiceController(shapeServiceResolver.Object, shapeServiceMock.Object);
            var request = new FeatureRequestModel
            {
                Key = ShapeProperties.BoroughBoundaries.ToString(),
                X = -74.0064,
                Y = 40.7142
            };

            // Act
            var result = await controller.GetGeospatialLookup(request);

            // Assert
            //var okResult = Assert.IsType<OkObjectResult>(result.Result);
            //var shapeResult = Assert.IsType<ShapeBase>(okResult.Value);
            //Assert.NotNull(shapeResult);
        }

        [Fact]
        public async Task GetFeatureLookup_ReturnsBadRequestResult()
        {
            var request = new FeatureRequestModel
            {
                Key = "InvalidRequest",
                X = -74.0064,
                Y = 40.7142
            };

            var controller = GetFeatureServiceController();

            // Act
            var sut = await controller.GetGeospatialLookup(request);

            // Assert
            Assert.IsType<BadRequestResult>(sut.Result);
        }

        private static FeatureServiceController GetFeatureServiceController(ShapeServiceResolver? resolver = null,
            IShapeService<ShapeBase>? shapeService = null)
        {

            ILogger<FeatureServiceController> logger = new Mock<ILogger<FeatureServiceController>>().Object;
            resolver ??= new Mock<ShapeServiceResolver?>().Object;


            return new FeatureServiceController(resolver, logger);
        }
    }
}