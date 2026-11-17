using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using MicroService.Service.Configuration;
using MicroService.Service.Models.Enum;
using MicroService.WebApi.Services.Cron;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json.Nodes;

namespace MicroService.WebApi.Extensions
{
    /// <summary>
    ///     Service Collection Extensions
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        ///     Display Configuration
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void DisplayConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            var config = configuration.Get<ApplicationOptions>();
            var shapeRootDirectory = config?.ShapeConfiguration?.ShapeRootDirectory;
            var shapeCronExpression = config?.ShapeConfiguration?.CronExpression;
            var shapeCronExpressionDescription = string.IsNullOrWhiteSpace(shapeCronExpression)
                ? "(not configured)"
                : CronExpressionDescriptor.ExpressionDescriptor.GetDescription(shapeCronExpression);

            Console.WriteLine($"ShapeRootDirectory Config: {shapeRootDirectory}");
            Console.WriteLine($"ShapeRootDirectory: {(shapeRootDirectory == null ? "(not configured)" : Path.GetFullPath(shapeRootDirectory, Directory.GetCurrentDirectory()))}");
            Console.WriteLine($"Shape CronExpression: {shapeCronExpression}");
            Console.WriteLine($"Shape Cron Description: {shapeCronExpressionDescription}");
        }

        /// <summary>
        ///     CORS: browsers may call the API only from the origins in <c>Cors:AllowedOrigins</c>
        ///     (the CoreDataStore site, its UI's Cloud Run URL for the post-deploy smoke test, and local UI development).
        ///     With none configured, no cross-origin request is allowed.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void AddCorsConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

            services.AddCors(options =>
                options.AddDefaultPolicy(builder => builder
                    .WithOrigins(origins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()));
        }

        /// <summary>
        /// Api Versioning
        /// </summary>
        /// <param name="services"></param>
        public static void AddCustomApiVersioning(this IServiceCollection services)
        {
            _ = services.AddApiVersioning(options =>
                {
                    options.ReportApiVersions = true;
                    options.ApiVersionReader = new UrlSegmentApiVersionReader();
                })
                .AddMvc()
                .AddApiExplorer(
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
        ///  Custom Controllers
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection AddCustomControllers(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllers(setupAction => { }).ConfigureApiBehaviorOptions(setupAction =>
             {
                 setupAction.InvalidModelStateResponseFactory = context =>
                 {
                     var problemDetails = new ValidationProblemDetails(context.ModelState)
                     {
                         Type = "https://courselibrary.com/modelvalidationproblem",
                         Title = "One or more model validation errors occurred.",
                         Status = StatusCodes.Status422UnprocessableEntity,
                         Detail = "See the errors property for details.",
                         Instance = context.HttpContext.Request.Path,
                     };

                     problemDetails.Extensions.Add("traceId", context.HttpContext.TraceIdentifier);
                     return new UnprocessableEntityObjectResult(problemDetails)
                     {
                         ContentTypes = { "application/problem+json" },
                     };
                 };
             });

            return services;
        }

        /// <summary>
        ///     HTTPS redirection, except on Cloud Run: it terminates TLS in front of the container,
        ///     so there is no HTTPS port to redirect to.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="environment"></param>
        public static IApplicationBuilder UseHttpsRedirectionOutsideCloudRun(this IApplicationBuilder app, IHostEnvironment environment)
        {
            if (!environment.IsEnvironment("CloudRun"))
            {
                app.UseHttpsRedirection();
            }

            return app;
        }

        /// <summary>
        /// Registers a <see cref="CronJobService"/> as a hosted service with the given schedule configuration.
        /// </summary>
        /// <typeparam name="T">CronJobService Type</typeparam>
        /// <param name="services"></param>
        /// <param name="options"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        public static IServiceCollection AddCronJob<T>(this IServiceCollection services, Action<IScheduleConfig> options)
            where T : CronJobService
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options), @"Please provide Schedule Configurations.");
            }

            var scheduleConfig = new ScheduleConfig();
            options.Invoke(scheduleConfig);
            if (string.IsNullOrWhiteSpace(scheduleConfig.CronExpression))
            {
                throw new ArgumentNullException(nameof(ScheduleConfig.CronExpression), @"Empty Cron Expression is not allowed.");
            }

            services.AddSingleton<IScheduleConfig>(scheduleConfig);
            services.AddHostedService<T>();

            return services;
        }
    }
}
