using MicroService.Service.Interfaces;
using MicroService.Service.Models.Base;
using MicroService.Test.Fixture;
using MicroService.WebApi.Models;
using MicroService.WebApi.V1.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;
using Xunit;

namespace MicroService.Test.Controllers
{
    /// <summary>
    /// Pins the attribute lookup contract the CoreDataStore API and UI rely on:
    /// POST /api/v1/FeatureService/attributelookup with
    /// { "key": "HistoricDistricts", "attributes": [{ "Key": "LPNumber", "Value": "LP-..." }] }
    /// returns a JSON array of features, or 404 when no feature matches.
    /// </summary>
    public class FeatureServiceControllerContractTest : IClassFixture<ShapeServiceFixture>
    {
        // Mirrors the JSON options configured in Program.cs.
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
        };

        private readonly FeatureServiceController _controller;

        public FeatureServiceControllerContractTest(ShapeServiceFixture fixture)
        {
            _controller = new FeatureServiceController(fixture.ShapeServiceResolver, NullLogger<FeatureServiceController>.Instance);
        }

        [Fact]
        public async Task AttributeLookup_HistoricDistrictsByLPNumber_ReturnsFeatureArray()
        {
            // Act
            var result = await _controller.GetAttributeLookup(CreateRequest("HistoricDistricts", "LP-00099"));

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            using var json = JsonDocument.Parse(JsonSerializer.Serialize(okResult.Value, JsonOptions));
            Assert.Equal(JsonValueKind.Array, json.RootElement.ValueKind);

            var feature = Assert.Single(json.RootElement.EnumerateArray());
            Assert.Equal("LP-00099", feature.GetProperty("lpNumber").GetString());
            Assert.Equal("Brooklyn Heights Historic District", feature.GetProperty("areaName").GetString());
            Assert.Equal("BK", feature.GetProperty("boroName").GetString());
            Assert.Equal(3, feature.GetProperty("boroCode").GetInt32());
            Assert.True(feature.GetProperty("shapeArea").GetDouble() > 0);
            Assert.True(feature.GetProperty("shapeLength").GetDouble() > 0);

            var centre = feature.GetProperty("boundingBox").GetProperty("centre");
            Assert.InRange(centre.GetProperty("x").GetDouble(), -74.01, -73.98);
            Assert.InRange(centre.GetProperty("y").GetDouble(), 40.68, 40.71);
        }

        [Theory]
        [InlineData("HistoricDistricts")]
        [InlineData("IndividualLandmarkSite")]
        public async Task AttributeLookup_UnknownLPNumber_ReturnsNotFound(string key)
        {
            // Act
            var result = await _controller.GetAttributeLookup(CreateRequest(key, "LP-99999"));

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        private static FeatureAttributeLookupRequestModel CreateRequest(string key, string lpNumber) => new()
        {
            Key = key,
            Attributes = new List<KeyValuePair<string, object>> { new("LPNumber", lpNumber) },
        };
    }
}
