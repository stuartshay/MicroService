using System;
using System.IO;
using System.Reflection;
using MicroService.Service.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
        public static void DisplayConfiguration(this IServiceCollection services, IConfiguration configuration, IHostingEnvironment environment)
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
        /// Api Versioning
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void AddApiVersioning(this IServiceCollection services, IConfiguration configuration)
        {
            _ = services.AddApiVersioning(options => { options.ReportApiVersions = true; });

            services.AddVersionedApiExplorer(
                options =>
                {
                    // add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
                    // note: the specified format code will format the version as "'v'major[.minor][-status]"
                    options.GroupNameFormat = "'v'VVV";

                    // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
                    // can also be used to control the format of the API version in route templates
                    options.SubstituteApiVersionInUrl = true;
                });
        }

        /// <summary>
        ///     Swagger Configuration
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        //public static void AddSwaggerConfiguration(this IServiceCollection services, IConfiguration configuration)
        //{
        //    // Swagger
        //    services.AddSwaggerGen(
        //       options =>
        //       {
        //           options.OperationFilter<SwaggerDefaultValues>();

        //           // options.DocumentFilter<Swagger.SwaggerDocumentFilter>();
        //           options.DescribeAllEnumsAsStrings();
        //           options.IncludeXmlComments(GetXmlCommentsPath());
        //       });
        //}

        /// <summary>
        ///   Custom Health Check.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection AddCustomHealthCheck(this IServiceCollection services, IConfiguration configuration)
        {
            var config = configuration.Get<ApplicationOptions>();
            services.AddHealthChecks()
                .AddNpgSql(config.ConnectionStrings.PostgreSql);

            services.AddHealthChecksUI();

            return services;
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
