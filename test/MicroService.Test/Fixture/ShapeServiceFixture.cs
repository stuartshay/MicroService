using System;
using System.IO;
using MicroService.Service.Configuration;
using MicroService.Service.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MicroService.Test.Fixture
{
    public class ShapeServiceFixture : FixtureConfig
    {
        public ShapeServiceFixture()
        {
            var serviceProvider = new ServiceCollection()
                .AddScoped<IBoroughBoundariesService, BoroughBoundariesService>()
                .AddOptions()
                .Configure<ApplicationOptions>(Configuration)
                .AddSingleton(Configuration)
                .BuildServiceProvider();

            BoroughBoundariesService = serviceProvider.GetRequiredService<IBoroughBoundariesService> ();
        }

        public IBoroughBoundariesService BoroughBoundariesService { get; set; }

        public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json", optional: true)
            .Build();


    }
}
