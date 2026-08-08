using MicroService.WebApi.Extensions.Swagger;
using Microsoft.OpenApi;
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

            filter.Apply(document, null!);

            Assert.Contains(document.Tags!, t => t.Name == "RoutingApi");
        }

        [Fact]
        public void Apply_ThrowsArgumentNullException_WhenDocumentIsNull()
        {
            var filter = new SwaggerDocumentFilter();

            Assert.Throws<ArgumentNullException>(() => filter.Apply(null!, null!));
        }
    }
}
