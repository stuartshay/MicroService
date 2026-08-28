using MicroService.WebApi.Extensions.Swagger;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi;
using Moq;
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

        private static DocumentFilterContext CreateContext() => new(
            new List<ApiDescription>(),
            Moq.Mock.Of<ISchemaGenerator>(),
            new SchemaRepository());
    }
}
