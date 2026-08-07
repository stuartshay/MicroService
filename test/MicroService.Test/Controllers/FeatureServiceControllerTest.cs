using MicroService.Service.Interfaces;
using MicroService.Service.Models;
using MicroService.Service.Models.Base;
using MicroService.Service.Models.Enum;
using MicroService.Service.Models.Enum.Attributes;
using MicroService.WebApi.Models;
using MicroService.WebApi.V1.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using System.Text;
using Xunit;


namespace MicroService.Test.Controllers
{
    public class FeatureServiceControllerTests
    {
        public static TheoryData<string, List<ShapeBase>> ValidRequests =>
            new()
            {
                { "BoroughBoundaries", new List<ShapeBase> { new BoroughBoundaryShape(), new CommunityDistrictShape() } },
                { "CommunityDistricts", new List<ShapeBase> { new CommunityDistrictShape(), new CommunityDistrictShape() } }
            };

        [Theory]
        [MemberData(nameof(ValidRequests))]
        public async Task GetFeatureList_WithValidRequest_ReturnsOkResult(string key, List<ShapeBase> expectedResults)
        {
            // Arrange
            var request = new FeatureAttributeRequestModel { Key = System.Enum.Parse<ShapeProperties>(key) };

            var shapeServiceMock = new Mock<IShapeService<ShapeBase>>();
            shapeServiceMock.Setup(s => s.GetFeatureList()).Returns(expectedResults);

            var shapeServiceResolver = new Mock<ShapeServiceResolver>();
            shapeServiceResolver.Setup(r => r(request.Key.ToString())).Returns(shapeServiceMock.Object);

            var controller = GetFeatureServiceController(shapeServiceResolver.Object);

            // Act
            var result = await controller.GetFeatureList(request);

            // Assert
            Assert.IsType<OkObjectResult>(result.Result);
            var okResult = (OkObjectResult)result.Result;
            Assert.Equal(expectedResults, okResult.Value);
        }

        [Fact]
        public async Task GetFeatureList_WithInvalidRequest_ReturnsBadRequestResult()
        {
            // Arrange: an out-of-range enum value, since Enum.Parse throws for unparsable
            // strings rather than returning an undefined value.
            var request = new FeatureAttributeRequestModel { Key = (ShapeProperties)9999 };

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
            var shapesResult = Assert.IsType<IEnumerable<object>>(okResult.Value, exactMatch: false);

            Assert.All(shapesResult, Assert.NotNull);
        }

        [Fact]
        public void Get_LogsResult_WhenInformationLoggingIsEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<FeatureServiceController>>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);

            var controller = new FeatureServiceController(new Mock<ShapeServiceResolver>().Object, loggerMock.Object);

            // Act
            var sut = controller.Get();

            // Assert
            Assert.IsType<OkObjectResult>(sut.Result);
            loggerMock.Verify(l => l.IsEnabled(LogLevel.Information), Times.AtLeastOnce);
        }

        [Fact]
        public void GetShapeProperties_ReturnsOkResult()
        {
            // Arrange
            var id = ShapeProperties.BoroughBoundaries;
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

            var shapeServiceResolver = new Mock<ShapeServiceResolver>();
            shapeServiceResolver.Setup(r => r(id.ToString())).Returns(shapeServiceMock.Object);

            var controller = GetFeatureServiceController(shapeServiceResolver.Object);

            // Act
            var sut = controller.GetShapeProperties(id);

            // Assert
            Assert.IsType<OkObjectResult>(sut.Result);
        }

        [Fact]
        public void GetShapeProperties_ReturnsOkResult_WithFieldsListPopulated()
        {
            // Arrange
            var id = ShapeProperties.BoroughBoundaries;
            var databaseProperties = new DbaseFileHeader
            {
                NumRecords = 5,
                Encoding = Encoding.UTF32,
                LastUpdateDate = new DateTime(2022, 01, 01),
            };
            databaseProperties.AddColumn("BoroCode", 'N', 10, 0);

            var shapeServiceMock = new Mock<IShapeService<BoroughBoundaryShape>>();
            shapeServiceMock.Setup(s => s.GetShapeDatabaseProperties()).Returns(databaseProperties);

            shapeServiceMock.Setup(x => x.GetShapeProperties()).Returns(new ShapefileHeader
            {
                Bounds = new Envelope(1, 2, 3, 4),
                ShapeType = ShapeGeometryType.Polygon,
            });

            var shapeServiceResolver = new Mock<ShapeServiceResolver>();
            shapeServiceResolver.Setup(r => r(id.ToString())).Returns(shapeServiceMock.Object);

            var controller = GetFeatureServiceController(shapeServiceResolver.Object);

            // Act
            var sut = controller.GetShapeProperties(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(sut.Result);
            var fieldsListProperty = okResult.Value!.GetType().GetProperty("FieldsList")!;
            var fieldsList = ((System.Collections.IEnumerable)fieldsListProperty.GetValue(okResult.Value)!).Cast<object>().ToList();
            Assert.Single(fieldsList);
        }

        [Fact]
        public void GetShapeProperties_ReturnsNotFoundResult_WhenDatabasePropertiesAreNull()
        {
            // Arrange
            var id = ShapeProperties.BoroughBoundaries;
            var shapeServiceMock = new Mock<IShapeService<BoroughBoundaryShape>>();
            shapeServiceMock.Setup(s => s.GetShapeDatabaseProperties()).Returns((DbaseFileHeader)null!);
            shapeServiceMock.Setup(x => x.GetShapeProperties()).Returns(new ShapefileHeader
            {
                Bounds = new Envelope(1, 2, 3, 4),
                ShapeType = ShapeGeometryType.Polygon,
            });

            var shapeServiceResolver = new Mock<ShapeServiceResolver>();
            shapeServiceResolver.Setup(r => r(id.ToString())).Returns(shapeServiceMock.Object);

            var controller = GetFeatureServiceController(shapeServiceResolver.Object);

            // Act
            var sut = controller.GetShapeProperties(id);

            // Assert
            Assert.IsType<NotFoundResult>(sut.Result);
        }


        [Fact]
        public void GetShapeProperties_ReturnsBadRequestResult()
        {
            //Arrange
            var controller = GetFeatureServiceController();

            // Act: an out-of-range enum value, since Enum.Parse throws for unparsable
            // strings rather than returning an undefined value.
            var sut = controller.GetShapeProperties((ShapeProperties)9999);

            // Assert
            Assert.IsType<BadRequestResult>(sut.Result);
        }

        [Fact]
        public async Task GetGeospatialLookup_ReturnsOkResult_WhenRequestIsValid()
        {
            // Arrange
            var id = "BoroughBoundaries";
            var request = new FeatureGeoRequestModel
            {
                Type = ShapeProperties.BoroughBoundaries,
                X = -74.0064,
                Y = 40.7142
            };
            var shapeServiceMock = new Mock<IShapeService<BoroughBoundaryShape>>();
            var expected = new BoroughBoundaryShape { BoroCode = 1 };
            shapeServiceMock.Setup(s => s.GetFeatureLookup(request.X, request.Y, request.Datum)).Returns(expected);

            var shapeServiceResolver = new Mock<ShapeServiceResolver>();
            shapeServiceResolver.Setup(r => r(id)).Returns(shapeServiceMock.Object);

            var controller = GetFeatureServiceController(shapeServiceResolver.Object);

            // Act
            var result = await controller.GetGeospatialLookup(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(expected, okResult.Value);
        }

        [Fact]
        public async Task GetGeospatialLookup_ReturnsNoContent_WhenBoroughBoundariesLookupIsNull()
        {
            // Arrange: the controller always validates against BoroughBoundaries first,
            // regardless of the requested Type.
            var request = new FeatureGeoRequestModel
            {
                Type = ShapeProperties.BoroughBoundaries,
                X = 1006187,
                Y = 732036
            };

            var shapeServiceMock = new Mock<IShapeService<BoroughBoundaryShape>>();
            shapeServiceMock.Setup(s => s.GetFeatureLookup(request.X, request.Y, request.Datum)).Returns((BoroughBoundaryShape?)null);

            var shapeServiceResolver = new Mock<ShapeServiceResolver>();
            shapeServiceResolver.Setup(r => r("BoroughBoundaries")).Returns(shapeServiceMock.Object);

            var controller = GetFeatureServiceController(shapeServiceResolver.Object);

            // Act
            var result = await controller.GetGeospatialLookup(request);

            // Assert
            Assert.IsType<NoContentResult>(result.Result);
        }

        [Fact]
        public async Task GetGeospatialLookup_ReturnsNotFound_WhenRequestedTypeLookupIsNull()
        {
            // Arrange: BoroughBoundaries validation succeeds, but the requested type
            // (a different shape) has no match at the given coordinates.
            var request = new FeatureGeoRequestModel
            {
                Type = ShapeProperties.CommunityDistricts,
                X = -74.0064,
                Y = 40.7142
            };

            var boroughServiceMock = new Mock<IShapeService<BoroughBoundaryShape>>();
            boroughServiceMock.Setup(s => s.GetFeatureLookup(request.X, request.Y, request.Datum))
                .Returns(new BoroughBoundaryShape { BoroCode = 1 });

            var communityServiceMock = new Mock<IShapeService<CommunityDistrictShape>>();
            communityServiceMock.Setup(s => s.GetFeatureLookup(request.X, request.Y, request.Datum)).Returns((CommunityDistrictShape?)null);

            var shapeServiceResolver = new Mock<ShapeServiceResolver>();
            shapeServiceResolver.Setup(r => r("BoroughBoundaries")).Returns(boroughServiceMock.Object);
            shapeServiceResolver.Setup(r => r("CommunityDistricts")).Returns(communityServiceMock.Object);

            var controller = GetFeatureServiceController(shapeServiceResolver.Object);

            // Act
            var result = await controller.GetGeospatialLookup(request);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetFeatureLookup_ReturnsBadRequestResult()
        {
            // Arrange: an out-of-range enum value; the default (unset) Type is 0,
            // which is itself a defined ShapeProperties member and wouldn't trigger the guard.
            var request = new FeatureGeoRequestModel
            {
                Type = (ShapeProperties)9999,
                X = -74.0064,
                Y = 40.7142
            };

            var controller = GetFeatureServiceController();

            // Act
            var sut = await controller.GetGeospatialLookup(request);

            // Assert
            Assert.IsType<BadRequestResult>(sut.Result);
        }

        [Fact]
        public async Task GetAttributeLookup_WithMissingAttributes_ReturnsBadRequestResult()
        {
            // Arrange
            var request = new FeatureAttributeLookupRequestModel { Key = "BoroughBoundaries", Attributes = null };

            var controller = GetFeatureServiceController();

            // Act
            var result = await controller.GetAttributeLookup(request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetAttributeLookup_WithInvalidKey_ReturnsBadRequestResult()
        {
            // Arrange
            var request = new FeatureAttributeLookupRequestModel { Key = "NotARealShapeKey", Attributes = null };

            var controller = GetFeatureServiceController();

            // Act
            var result = await controller.GetAttributeLookup(request);

            // Assert
            Assert.IsType<BadRequestResult>(result.Result);
        }

        [Fact]
        public async Task GetAttributeLookup_WithEmptyAttributes_ReturnsBadRequestResult()
        {
            // Arrange
            var request = new FeatureAttributeLookupRequestModel { Key = "BoroughBoundaries", Attributes = new List<KeyValuePair<string, object>>() };

            var controller = GetFeatureServiceController();

            // Act
            var result = await controller.GetAttributeLookup(request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetLookupFeatureGeoJson_WithMissingAttributes_ReturnsBadRequestResult()
        {
            // Arrange
            var request = new FeatureAttributeLookupRequestModel { Key = "BoroughBoundaries", Attributes = null };

            var controller = GetFeatureServiceController();

            // Act
            var result = await controller.GetLookupFeatureGeoJson(request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetLookupFeatureGeoJson_WithInvalidKey_ReturnsBadRequestResult()
        {
            // Arrange
            var request = new FeatureAttributeLookupRequestModel { Key = "NotARealShapeKey", Attributes = null };

            var controller = GetFeatureServiceController();

            // Act
            var result = await controller.GetLookupFeatureGeoJson(request);

            // Assert
            Assert.IsType<BadRequestResult>(result.Result);
        }

        [Fact]
        public async Task GetAttributeLookup_WithInvalidAttributeKey_ReturnsBadRequestResult()
        {
            // Arrange: "NotARealAttribute" has no [FeatureName]/[MappingKey] on BoroughBoundaryShape.
            var request = new FeatureAttributeLookupRequestModel
            {
                Key = "BoroughBoundaries",
                Attributes = new List<KeyValuePair<string, object>> { new("NotARealAttribute", 1) },
            };

            var shapeServiceMock = new Mock<IShapeService<BoroughBoundaryShape>>();
            var shapeServiceResolver = new Mock<ShapeServiceResolver>();
            shapeServiceResolver.Setup(r => r(request.Key)).Returns(shapeServiceMock.Object);

            var controller = GetFeatureServiceController(shapeServiceResolver.Object);

            // Act
            var result = await controller.GetAttributeLookup(request);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Contains("NotARealAttribute", badRequest.Value!.ToString());
        }

        [Fact]
        public async Task GetAttributeLookup_WithValidAttributes_ReturnsOkResult()
        {
            // Arrange
            var request = new FeatureAttributeLookupRequestModel
            {
                Key = "BoroughBoundaries",
                Attributes = new List<KeyValuePair<string, object>> { new("BoroCode", 1) },
            };
            var expected = new List<BoroughBoundaryShape> { new() { BoroCode = 1, BoroName = "Manhattan" } };

            var shapeServiceMock = new Mock<IShapeService<BoroughBoundaryShape>>();
            shapeServiceMock.Setup(s => s.GetFeatureLookup(request.Attributes)).Returns(expected);

            var shapeServiceResolver = new Mock<ShapeServiceResolver>();
            shapeServiceResolver.Setup(r => r(request.Key)).Returns(shapeServiceMock.Object);

            var controller = GetFeatureServiceController(shapeServiceResolver.Object);

            // Act
            var result = await controller.GetAttributeLookup(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var results = Assert.IsType<List<object>>(okResult.Value);
            Assert.Single(results);
        }

        [Fact]
        public async Task GetAttributeLookup_WithValidMappingKeyAttribute_ReturnsOkResult()
        {
            // Arrange: LPNumber is declared via [MappingKey] rather than [FeatureName],
            // so this exercises the mappingFields branch of the field-validation union.
            var request = new FeatureAttributeLookupRequestModel
            {
                Key = "IndividualLandmarkHistoricDistricts",
                Attributes = new List<KeyValuePair<string, object>> { new("LPNumber", "LP-00001") },
            };
            var expected = new List<IndividualLandmarkHistoricDistrictsShape> { new() { LPNumber = "LP-00001" } };

            var shapeServiceMock = new Mock<IShapeService<IndividualLandmarkHistoricDistrictsShape>>();
            shapeServiceMock.Setup(s => s.GetFeatureLookup(request.Attributes)).Returns(expected);

            var shapeServiceResolver = new Mock<ShapeServiceResolver>();
            shapeServiceResolver.Setup(r => r(request.Key)).Returns(shapeServiceMock.Object);

            var controller = GetFeatureServiceController(shapeServiceResolver.Object);

            // Act
            var result = await controller.GetAttributeLookup(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var results = Assert.IsType<List<object>>(okResult.Value);
            Assert.Single(results);
        }

        [Fact]
        public async Task GetAttributeLookup_WithNullLookupResult_ReturnsNotFoundResult()
        {
            // Arrange
            var request = new FeatureAttributeLookupRequestModel
            {
                Key = "BoroughBoundaries",
                Attributes = new List<KeyValuePair<string, object>> { new("BoroCode", 1) },
            };

            var shapeServiceMock = new Mock<IShapeService<BoroughBoundaryShape>>();
            shapeServiceMock.Setup(s => s.GetFeatureLookup(request.Attributes)).Returns((IEnumerable<BoroughBoundaryShape>)null!);

            var shapeServiceResolver = new Mock<ShapeServiceResolver>();
            shapeServiceResolver.Setup(r => r(request.Key)).Returns(shapeServiceMock.Object);

            var controller = GetFeatureServiceController(shapeServiceResolver.Object);

            // Act
            var result = await controller.GetAttributeLookup(request);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetAttributeLookup_WithNoMatches_ReturnsNotFoundResult()
        {
            // Arrange
            var request = new FeatureAttributeLookupRequestModel
            {
                Key = "BoroughBoundaries",
                Attributes = new List<KeyValuePair<string, object>> { new("BoroCode", 999) },
            };

            var shapeServiceMock = new Mock<IShapeService<BoroughBoundaryShape>>();
            shapeServiceMock.Setup(s => s.GetFeatureLookup(request.Attributes)).Returns(new List<BoroughBoundaryShape>());

            var shapeServiceResolver = new Mock<ShapeServiceResolver>();
            shapeServiceResolver.Setup(r => r(request.Key)).Returns(shapeServiceMock.Object);

            var controller = GetFeatureServiceController(shapeServiceResolver.Object);

            // Act
            var result = await controller.GetAttributeLookup(request);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetLookupFeatureGeoJson_WithInvalidAttributeKey_ReturnsBadRequestResult()
        {
            // Arrange
            var request = new FeatureAttributeLookupRequestModel
            {
                Key = "BoroughBoundaries",
                Attributes = new List<KeyValuePair<string, object>> { new("NotARealAttribute", 1) },
            };

            var shapeServiceMock = new Mock<IShapeService<BoroughBoundaryShape>>();
            var shapeServiceResolver = new Mock<ShapeServiceResolver>();
            shapeServiceResolver.Setup(r => r(request.Key)).Returns(shapeServiceMock.Object);

            var controller = GetFeatureServiceController(shapeServiceResolver.Object);

            // Act
            var result = await controller.GetLookupFeatureGeoJson(request);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Contains("NotARealAttribute", badRequest.Value!.ToString());
        }

        [Fact]
        public async Task GetLookupFeatureGeoJson_WithNoMatches_ReturnsNotFoundResult()
        {
            // Arrange
            var request = new FeatureAttributeLookupRequestModel
            {
                Key = "BoroughBoundaries",
                Attributes = new List<KeyValuePair<string, object>> { new("BoroCode", 999) },
            };

            var shapeServiceMock = new Mock<IShapeService<BoroughBoundaryShape>>();
            shapeServiceMock.Setup(s => s.GetFeatureCollection(request.Attributes)).Returns((FeatureCollection?)null);

            var shapeServiceResolver = new Mock<ShapeServiceResolver>();
            shapeServiceResolver.Setup(r => r(request.Key)).Returns(shapeServiceMock.Object);

            var controller = GetFeatureServiceController(shapeServiceResolver.Object);

            // Act
            var result = await controller.GetLookupFeatureGeoJson(request);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetLookupFeatureGeoJson_WithValidAttributes_ReturnsOkResult()
        {
            // Arrange
            var request = new FeatureAttributeLookupRequestModel
            {
                Key = "BoroughBoundaries",
                Attributes = new List<KeyValuePair<string, object>> { new("BoroCode", 1) },
            };

            var point = new Point(-74.0064, 40.7142);
            var attributesTable = new AttributesTable(new Dictionary<string, object> { { "BoroCode", 1 } });
            var featureCollection = new FeatureCollection { new Feature(point, attributesTable) };

            var shapeServiceMock = new Mock<IShapeService<BoroughBoundaryShape>>();
            shapeServiceMock.Setup(s => s.GetFeatureCollection(request.Attributes)).Returns(featureCollection);

            var shapeServiceResolver = new Mock<ShapeServiceResolver>();
            shapeServiceResolver.Setup(r => r(request.Key)).Returns(shapeServiceMock.Object);

            var controller = GetFeatureServiceController(shapeServiceResolver.Object);

            // Act
            var result = await controller.GetLookupFeatureGeoJson(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var resultCollection = Assert.IsType<FeatureCollection>(okResult.Value);
            Assert.Single(resultCollection);
        }

        [Fact]
        public async Task GetLookupFeatureGeoJson_WithValidMappingKeyAttribute_ReturnsOkResult()
        {
            // Arrange: LPNumber is declared via [MappingKey] rather than [FeatureName],
            // so this exercises the mappingFields branch of the field-validation union.
            var request = new FeatureAttributeLookupRequestModel
            {
                Key = "IndividualLandmarkHistoricDistricts",
                Attributes = new List<KeyValuePair<string, object>> { new("LPNumber", "LP-00001") },
            };

            var point = new Point(-74.0064, 40.7142);
            var attributesTable = new AttributesTable(new Dictionary<string, object> { { "LPNumber", "LP-00001" } });
            var featureCollection = new FeatureCollection { new Feature(point, attributesTable) };

            var shapeServiceMock = new Mock<IShapeService<IndividualLandmarkHistoricDistrictsShape>>();
            shapeServiceMock.Setup(s => s.GetFeatureCollection(request.Attributes)).Returns(featureCollection);

            var shapeServiceResolver = new Mock<ShapeServiceResolver>();
            shapeServiceResolver.Setup(r => r(request.Key)).Returns(shapeServiceMock.Object);

            var controller = GetFeatureServiceController(shapeServiceResolver.Object);

            // Act
            var result = await controller.GetLookupFeatureGeoJson(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var resultCollection = Assert.IsType<FeatureCollection>(okResult.Value);
            Assert.Single(resultCollection);
        }

        private static FeatureServiceController GetFeatureServiceController(ShapeServiceResolver? resolver = null)
        {

            ILogger<FeatureServiceController> logger = new Mock<ILogger<FeatureServiceController>>().Object;
            resolver ??= new Mock<ShapeServiceResolver>().Object;


            return new FeatureServiceController(resolver, logger);
        }
    }
}
