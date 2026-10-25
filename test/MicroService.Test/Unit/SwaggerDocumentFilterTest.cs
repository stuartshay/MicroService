using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using MicroService.Common.Logging;
using MicroService.WebApi.Extensions.Swagger;
using MicroService.WebApi.Extensions.Swagger.Examples;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;
using Serilog;
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
        public void ConfigureSwaggerOptions_AppendsDeprecatedNotice_WhenVersionIsDeprecated()
        {
            var providerMock = new Moq.Mock<IApiVersionDescriptionProvider>();
            providerMock
                .SetupGet(p => p.ApiVersionDescriptions)
                .Returns(
                [
                    new ApiVersionDescription(new ApiVersion(1, 0), "v1", true),
                ]);
            var configureOptions = new ConfigureSwaggerOptions(providerMock.Object);
            var swaggerGenOptions = new SwaggerGenOptions();

            configureOptions.Configure(swaggerGenOptions);

            Assert.True(swaggerGenOptions.SwaggerGeneratorOptions.SwaggerDocs.TryGetValue("v1", out var info));
            Assert.Contains("deprecated", info!.Description);
        }

        [Fact]
        public void Logging_ConfigureLogger_AddsGrafanaLokiSink_WhenEnabled()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["GrafanaLokiConfiguration:Uri"] = "http://localhost:3100",
                    ["GrafanaLokiConfiguration:Enabled"] = "true",
                })
                .Build();
            var hostEnvironmentMock = new Moq.Mock<IHostEnvironment>();
            hostEnvironmentMock.SetupGet(e => e.ApplicationName).Returns("MicroService.WebApi");
            hostEnvironmentMock.SetupGet(e => e.EnvironmentName).Returns("Test");
            var hostContext = new HostBuilderContext(new Dictionary<object, object>())
            {
                Configuration = configuration,
                HostingEnvironment = hostEnvironmentMock.Object,
            };
            var loggerConfiguration = new LoggerConfiguration();

            Logging.ConfigureLogger(hostContext, loggerConfiguration);
            using var logger = loggerConfiguration.CreateLogger();

            Assert.NotNull(logger);
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
