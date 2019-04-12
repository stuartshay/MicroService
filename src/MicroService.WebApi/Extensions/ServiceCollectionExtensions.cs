using System;
using System.IO;
using System.Reflection;
using MicroService.Service.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;

namespace MicroService.WebApi.Extensions
{
    /// <summary>
    ///     Service Collection Extensions
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        ///     Display Configuration
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="environment"></param>
        public static void DisplayConfiguration(this IServiceCollection services, IConfiguration configuration,
            IHostingEnvironment environment)
        {
            var config = configuration.Get<ApplicationOptions>();
            Console.WriteLine($"Environment: {environment.EnvironmentName}");
            Console.WriteLine($"PostgreSql: {config.ConnectionStrings.PostgreSql}");
        }

        /// <summary>
        ///     CORS Configuration
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void AddCorsConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(
                    "AllowAll",
                    builder => builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials());
            });
        }

        /// <summary>
        ///     Swagger Configuration
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void AddSwaggerConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            // Swagger
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Info
                {
                    Title = "MicroService.WebAPI",
                    Description = "MicroService.WebAPI",
                    Version = "v1",
                    TermsOfService = "None",
                });
                options.DescribeAllEnumsAsStrings();
                options.IncludeXmlComments(GetXmlCommentsPath());
            });
        }

        private static string GetXmlCommentsPath()
        {
            var basePath = AppContext.BaseDirectory;
            var assemblyName = Assembly.GetEntryAssembly().GetName().Name;
            var fileName = Path.GetFileName(assemblyName + ".xml");

            return Path.Combine(basePath, fileName);
        }
    }
}
