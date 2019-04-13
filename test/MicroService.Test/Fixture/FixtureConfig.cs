using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace MicroService.Test.Fixture
{
    public abstract class FixtureConfig : IDisposable
    {
        protected FixtureConfig()
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            Console.WriteLine("ENV:" + env);

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{env}.json", true)
                .AddEnvironmentVariables()
                .Build();

            DbConnection = builder.GetConnectionString("PostgreSql");
            Console.WriteLine("DbConnection:" + DbConnection);
        }

        public string DbConnection { get; }

        public virtual void Dispose()
        {
        }
    }
}
