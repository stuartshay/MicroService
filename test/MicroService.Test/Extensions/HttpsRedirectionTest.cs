using MicroService.WebApi.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace MicroService.Test.Extensions
{
    public class HttpsRedirectionTest
    {
        [Theory(DisplayName = "HTTPS redirection - skipped on Cloud Run, which terminates TLS in front of the container")]
        [Trait("Category", "Unit")]
        [InlineData("Development", StatusCodes.Status307TemporaryRedirect)]
        [InlineData("Docker", StatusCodes.Status307TemporaryRedirect)]
        [InlineData("CloudRun", StatusCodes.Status404NotFound)]
        public async Task Http_Request_Is_Redirected_Except_On_CloudRun(string environmentName, int expectedStatusCode)
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
            services.AddHttpsRedirection(options => options.HttpsPort = 443);
            await using var provider = services.BuildServiceProvider();

            var app = new ApplicationBuilder(provider);
            app.UseHttpsRedirectionOutsideCloudRun(new TestHostEnvironment(environmentName));
            var pipeline = app.Build();

            var context = new DefaultHttpContext { RequestServices = provider };
            context.Request.Scheme = "http";
            context.Request.Host = new HostString("microservice.example");
            context.Request.Path = "/api/v1/FeatureService";

            // Act
            await pipeline(context);

            // Assert
            Assert.Equal(expectedStatusCode, context.Response.StatusCode);
        }

        private sealed class TestHostEnvironment(string environmentName) : IHostEnvironment
        {
            public string EnvironmentName { get; set; } = environmentName;

            public string ApplicationName { get; set; } = "MicroService.WebApi";

            public string ContentRootPath { get; set; } = AppContext.BaseDirectory;

            public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        }
    }
}
