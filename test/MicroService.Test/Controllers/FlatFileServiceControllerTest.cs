using MicroService.Service.Models.Enum;
using MicroService.Service.Models.FlatFileModels;
using MicroService.Service.Services.FlatFileService;
using MicroService.WebApi.V1.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace MicroService.Test.Controllers
{
    public class FlatFileServiceControllerTest
    {
        [Fact]
        public void Get_ReturnsAvailableFlatFiles()
        {
            var controller = GetController();

            var result = controller.Get();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var items = Assert.IsType<IEnumerable<object>>(okResult.Value, exactMatch: false).ToList();

            var expectedKeys = System.Enum.GetNames<FlatFileProperties>();
            var actualKeys = items.Select(GetKey).ToList();

            Assert.Equal(expectedKeys.Length, items.Count);
            foreach (var expectedKey in expectedKeys)
            {
                Assert.Contains(expectedKey, actualKeys);
            }

            Assert.All(items, item =>
            {
                Assert.False(string.IsNullOrEmpty(GetKey(item)));
                Assert.False(string.IsNullOrEmpty((string)item.GetType().GetProperty("fileName")!.GetValue(item)!));
                Assert.False(string.IsNullOrEmpty((string)item.GetType().GetProperty("directory")!.GetValue(item)!));
            });
        }

        private static string GetKey(object item) => (string)item.GetType().GetProperty("key")!.GetValue(item)!;

        [Fact]
        public void GetFlatFile_ReturnsBadRequest_WhenIdIsNotDefined()
        {
            var controller = GetController();

            var result = controller.GetFlatFile("NotARealFlatFile");

            Assert.IsType<BadRequestResult>(result.Result);
        }

        [Fact]
        public void GetFlatFile_ReturnsOkResult_ForStationFlatFile()
        {
            var expected = new List<StationFlatFile> { new() { StationId = 1 } };
            var serviceMock = new Mock<IFlatFileService>();
            serviceMock.Setup(s => s.GetAll()).Returns(expected);

            var resolverMock = new Mock<FlatFileResolver>();
            resolverMock.Setup(r => r(nameof(MicroService.Service.Models.Enum.FlatFileProperties.SubwayStationLocations)))
                .Returns(serviceMock.Object);

            var controller = GetController(resolverMock.Object);

            var result = controller.GetFlatFile(nameof(MicroService.Service.Models.Enum.FlatFileProperties.SubwayStationLocations));

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(expected, okResult.Value);
        }

        [Fact]
        public void GetFlatFile_ReturnsOkResult_ForStationComplexFlatFile()
        {
            var expected = new List<StationComplexFlatFile> { new() { ComplexId = 1 } };
            var serviceMock = new Mock<IFlatFileService>();
            serviceMock.Setup(s => s.GetAll()).Returns(expected);

            var resolverMock = new Mock<FlatFileResolver>();
            resolverMock.Setup(r => r(nameof(MicroService.Service.Models.Enum.FlatFileProperties.SubwayStationComplex)))
                .Returns(serviceMock.Object);

            var controller = GetController(resolverMock.Object);

            var result = controller.GetFlatFile(nameof(MicroService.Service.Models.Enum.FlatFileProperties.SubwayStationComplex));

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(expected, okResult.Value);
        }

        private static FlatFileServiceController GetController(FlatFileResolver? resolver = null)
        {
            resolver ??= new Mock<FlatFileResolver>().Object;

            return new FlatFileServiceController(resolver);
        }
    }
}
