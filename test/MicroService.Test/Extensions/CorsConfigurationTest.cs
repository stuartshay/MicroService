using MicroService.WebApi.Extensions;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MicroService.Test.Extensions
{
    public class CorsConfigurationTest
    {
        [Theory(DisplayName = "CORS - appsettings.json allows the site, the UI smoke test and local UI development")]
        [Trait("Category", "Unit")]
        [InlineData("https://coredatastore.com", true)]
        [InlineData("https://coredatastore-ui-w6zlqlyoma-uk.a.run.app", true)]
        [InlineData("http://localhost:5173", true)]
        [InlineData("http://localhost:4173", true)]
        [InlineData("http://localhost:2000", true)]
        [InlineData("https://evil.example", false)]
        [InlineData("http://coredatastore.com", false)]
        [InlineData("https://coredatastore.com.evil.example", false)]
        public async Task AppSettings_Origins(string origin, bool allowed)
        {
            // The Web API's appsettings.json is copied to the test output directory
            var configuration = new ConfigurationBuilder()
                .AddJsonFile(Path.Join(AppContext.BaseDirectory, "appsettings.json"))
                .Build();

            Assert.Equal(allowed, await IsAllowed(configuration, "GET", origin));
        }

        [Fact(DisplayName = "CORS - a JSON POST preflight from the site is allowed")]
        [Trait("Category", "Unit")]
        public async Task AppSettings_Preflight_Allowed()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile(Path.Join(AppContext.BaseDirectory, "appsettings.json"))
                .Build();

            Assert.True(await IsAllowed(configuration, "OPTIONS", "https://coredatastore.com", preflight: true));
        }

        [Fact(DisplayName = "CORS - with no origins configured, no cross-origin request is allowed")]
        [Trait("Category", "Unit")]
        public async Task No_Origins_Allows_None()
        {
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>()).Build();

            Assert.False(await IsAllowed(configuration, "GET", "https://coredatastore.com"));
        }

        private static async Task<bool> IsAllowed(IConfiguration configuration, string method, string origin, bool preflight = false)
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddCorsConfiguration(configuration);
            using var provider = services.BuildServiceProvider();

            var context = new DefaultHttpContext { RequestServices = provider };
            context.Request.Method = method;
            context.Request.Headers.Origin = origin;
            if (preflight)
            {
                context.Request.Headers.AccessControlRequestMethod = "POST";
                context.Request.Headers.AccessControlRequestHeaders = "content-type";
            }

            var policy = await provider.GetRequiredService<ICorsPolicyProvider>().GetPolicyAsync(context, null);
            var result = provider.GetRequiredService<ICorsService>().EvaluatePolicy(context, policy!);

            return preflight
                ? result.IsOriginAllowed && result.IsPreflightRequest && result.AllowedMethods.Contains("POST") && result.AllowedHeaders.Contains("content-type")
                : result.IsOriginAllowed;
        }
    }
}
