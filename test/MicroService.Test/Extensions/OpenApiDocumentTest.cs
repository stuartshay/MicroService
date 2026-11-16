using System.Text.Json.Nodes;
using Xunit;

namespace MicroService.Test.Extensions
{
    /// <summary>
    /// Starts the real Web API and reads the OpenAPI document that Microsoft.AspNetCore.OpenApi
    /// generates, checking what the Swashbuckle document used to provide.
    /// </summary>
    public class OpenApiDocumentTest : IClassFixture<CorsPipelineTest.ApiFactory>
    {
        private readonly CorsPipelineTest.ApiFactory _factory;

        public OpenApiDocumentTest(CorsPipelineTest.ApiFactory factory)
        {
            _factory = factory;
        }

        private async Task<JsonNode> GetDocumentAsync(string uri)
        {
            using var client = _factory.CreateClient();
            using var response = await client.GetAsync(uri, TestContext.Current.CancellationToken);
            response.EnsureSuccessStatusCode();
            return JsonNode.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken))!;
        }

        [Theory(DisplayName = "OpenAPI - the document is served on the new and the Swashbuckle-era route")]
        [Trait("Category", "Integration")]
        [InlineData("/openapi/v1.json")]
        [InlineData("/swagger/v1/swagger.json")]
        public async Task Document_Lists_Every_Endpoint(string uri)
        {
            var document = await GetDocumentAsync(uri);

            Assert.Equal("3.1.2", document["openapi"]!.GetValue<string>());
            Assert.Equal("MicroService.WebApi", document["info"]!["title"]!.GetValue<string>());
            Assert.Equal("1.0", document["info"]!["version"]!.GetValue<string>());

            var paths = document["paths"]!.AsObject().Select(path => path.Key).ToList();
            Assert.Equal(10, paths.Count);
            Assert.Contains("/api/v1/FeatureService/{id}", paths);
            Assert.Contains("/api/v1/FlatFileService/{id}", paths);
            Assert.Contains("/api/v1/TransformationService/ConvertWgs84ToNad83", paths);
        }

        [Fact(DisplayName = "OpenAPI - enums are listed by name, as the controllers write them")]
        [Trait("Category", "Integration")]
        public async Task Enums_Are_Strings_With_Names()
        {
            var schemas = (await GetDocumentAsync("/openapi/v1.json"))["components"]!["schemas"]!;

            var shapes = schemas["ShapeProperties"]!;
            Assert.Equal("string", shapes["type"]!.GetValue<string>());
            Assert.Contains("BoroughBoundaries", shapes["enum"]!.AsArray().Select(value => value!.GetValue<string>()));
            Assert.Equal(16, shapes["enum"]!.AsArray().Count);

            Assert.Equal(["Wgs84", "Nad83"], schemas["Datum"]!["enum"]!.AsArray().Select(value => value!.GetValue<string>()));
        }

        [Fact(DisplayName = "OpenAPI - the response and request examples are kept")]
        [Trait("Category", "Integration")]
        public async Task Examples_Are_Present()
        {
            var paths = (await GetDocumentAsync("/openapi/v1.json"))["paths"]!;

            var shapeTypes = paths["/api/v1/FeatureService"]!["get"]!["responses"]!["200"]!["content"]!["application/json"]!["example"]!;
            Assert.Equal("BoroughBoundaries", shapeTypes[0]!["key"]!.GetValue<string>());

            var shapeProperties = paths["/api/v1/FeatureService/{id}"]!["get"]!["responses"]!["200"]!["content"]!["application/json"]!["example"]!;
            Assert.Equal(1517, shapeProperties["numRecords"]!.GetValue<int>());

            var request = paths["/api/v1/FeatureService/attributelookup"]!["post"]!["requestBody"]!["content"]!["application/json"]!["example"]!;
            Assert.Equal("IndividualLandmarkHistoricDistricts", request["key"]!.GetValue<string>());
        }

        [Fact(DisplayName = "OpenAPI - the Bearer scheme and every response description are set")]
        [Trait("Category", "Integration")]
        public async Task Security_And_Descriptions()
        {
            var document = await GetDocumentAsync("/openapi/v1.json");

            Assert.Equal("Authorization", document["components"]!["securitySchemes"]!["Bearer"]!["name"]!.GetValue<string>());

            var descriptions = document["paths"]!.AsObject()
                .SelectMany(path => path.Value!.AsObject())
                .SelectMany(operation => operation.Value!["responses"]!.AsObject())
                .Select(response => response.Value!["description"]?.GetValue<string>());
            Assert.All(descriptions, description => Assert.False(string.IsNullOrEmpty(description)));
        }

        [Fact(DisplayName = "OpenAPI - Swagger UI is served at /swagger and the root redirects to it")]
        [Trait("Category", "Integration")]
        public async Task Swagger_Ui_Is_Served()
        {
            using var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

            using var ui = await client.GetAsync("/swagger/index.html", TestContext.Current.CancellationToken);
            Assert.Equal(System.Net.HttpStatusCode.OK, ui.StatusCode);

            using var root = await client.GetAsync("/", TestContext.Current.CancellationToken);
            Assert.Equal("swagger", root.Headers.Location?.OriginalString.TrimStart('/'));
        }
    }
}
