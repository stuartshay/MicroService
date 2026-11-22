using MicroService.Test.Extensions;
using System.Net;
using System.Text.Json;
using Xunit;

namespace MicroService.Test.Controllers
{
    /// <summary>
    /// /api/v1/Layers through the real Web API (Program.cs) in memory.
    /// </summary>
    [Trait("Category", "Integration")]
    public class LayersControllerTest : IClassFixture<CorsPipelineTest.ApiFactory>
    {
        private readonly CorsPipelineTest.ApiFactory _factory;

        public LayersControllerTest(CorsPipelineTest.ApiFactory factory)
        {
            _factory = factory;
        }

        [Fact(DisplayName = "Layers API - the list has every layer with its color, source date and feature count")]
        public async Task List()
        {
            using var response = await GetAsync("/api/v1/Layers");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("public, max-age=86400", response.Headers.CacheControl?.ToString());
            using var document = await ReadJsonAsync(response);
            var layers = document.RootElement.EnumerateArray().ToList();
            Assert.Equal(7, layers.Count);
            var libraries = layers.Single(l => l.GetProperty("key").GetString() == "Libraries");
            Assert.Equal("point", libraries.GetProperty("geometry").GetString());
            Assert.Equal("2016-06-22", libraries.GetProperty("dataUpdated").GetString());
            Assert.Equal(216, libraries.GetProperty("features").GetInt32());
        }

        [Fact(DisplayName = "Layers API - a layer's features in a box are GeoJSON, cacheable for a day")]
        public async Task Features()
        {
            using var response = await GetAsync("/api/v1/Layers/historicdistricts?bbox=-73.999,40.693,-73.993,40.697");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("application/geo+json", response.Content.Headers.ContentType?.MediaType);
            Assert.Equal("public, max-age=86400", response.Headers.CacheControl?.ToString());
            using var document = await ReadJsonAsync(response);
            Assert.Contains(document.RootElement.GetProperty("features").EnumerateArray(),
                f => f.GetProperty("properties").GetProperty("title").GetString() == "Brooklyn Heights Historic District");
        }

        [Fact(DisplayName = "Layers API - an unknown layer is 404, a bad box 400 with the field")]
        public async Task Errors()
        {
            using var unknown = await GetAsync("/api/v1/Layers/Subway?bbox=-74,40.7,-73.9,40.8");
            using var bad = await GetAsync("/api/v1/Layers/Libraries?bbox=-74,40.7");
            using var missing = await GetAsync("/api/v1/Layers/Libraries");

            Assert.Equal(HttpStatusCode.NotFound, unknown.StatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, bad.StatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, missing.StatusCode);
            Assert.Null(bad.Headers.CacheControl);
            using var problem = await ReadJsonAsync(bad);
            Assert.True(problem.RootElement.GetProperty("errors").TryGetProperty("bbox", out _));
        }

        private async Task<HttpResponseMessage> GetAsync(string uri)
        {
            using var client = _factory.CreateClient();
            return await client.GetAsync(uri, TestContext.Current.CancellationToken);
        }

        private static async Task<JsonDocument> ReadJsonAsync(HttpResponseMessage response) =>
            await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(TestContext.Current.CancellationToken), cancellationToken: TestContext.Current.CancellationToken);
    }
}
