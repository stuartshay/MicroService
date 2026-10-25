using MicroService.WebApi.Extensions.Swagger;
using MicroService.WebApi.Extensions.Swagger.Examples;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using Xunit;

namespace MicroService.Test.Unit
{
    public class SwaggerDocumentFilterTest
    {
        [Fact]
        public void Apply_AddsRoutingApiTag()
        {
            var filter = new SwaggerDocumentFilter();
            var document = new OpenApiDocument();

            filter.Apply(document, CreateContext());

            Assert.Contains(document.Tags!, t => t.Name == "RoutingApi");
        }

        [Fact]
        public void Apply_ThrowsArgumentNullException_WhenDocumentIsNull()
        {
            var filter = new SwaggerDocumentFilter();

            Assert.Throws<ArgumentNullException>(() => filter.Apply(null!, CreateContext()));
        }

        [Fact]
        public void GetShapeTypesExample_ReturnsNonEmptyExamples()
        {
            var provider = new GetShapeTypesExample();

            var examples = provider.GetExamples().ToList();

            Assert.Equal(2, examples.Count);
        }

        [Fact]
        public void GetShapePropertiesExample_ReturnsNonNullExample()
        {
            var provider = new GetShapePropertiesExample();

            var example = provider.GetExamples();

            Assert.NotNull(example);
        }

        [Fact]
        public void SwaggerFeatureRequestExamples_ReturnsExpectedDefaultExample()
        {
            var provider = new SwaggerFeatureRequestExamples();

            var example = provider.GetExamples();

            Assert.NotNull(example);
            Assert.Equal("IndividualLandmarkHistoricDistricts", example.Key);
            Assert.NotNull(example.Attributes);
            Assert.Single(example.Attributes);
        }

        private static DocumentFilterContext CreateContext() => new(
            new List<ApiDescription>(),
            Moq.Mock.Of<ISchemaGenerator>(),
            new SchemaRepository());
    }
}
