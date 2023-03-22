using MicroService.Service.Configuration;
using MicroService.Service.Models.Enum;
using MicroService.WebApi.Extensions.Constants;
using MicroService.WebApi.Services.Cron;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Swashbuckle.AspNetCore.Filters;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

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
            var shapeCronExpressionDescription = CronExpressionDescriptor.ExpressionDescriptor.GetDescription(config!.ShapeConfiguration.CronExpression);

            Console.WriteLine($"PostgreSql: {config.ConnectionStrings.PostgreSql}");
            Console.WriteLine($"ShapeRootDirectory Config: {config.ShapeConfiguration.ShapeRootDirectory}");
            Console.WriteLine($"ShapeRootDirectory: {Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), config.ShapeConfiguration.ShapeRootDirectory))}");
            Console.WriteLine($"Shape CronExpression: {config.ShapeConfiguration.CronExpression}");
            Console.WriteLine($"Shape Cron Description: {shapeCronExpressionDescription}");
        }

        /// <summary>
        ///     CORS Configuration
        /// </summary>
        /// <param name="services"></param>
        public static void AddCorsConfiguration(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                   builder => builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());

                options.AddPolicy(
                    ApiConstants.CorsPolicy,
                    builder => builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
            });
        }

        /// <summary>
        /// Api Versioning
        /// </summary>
        /// <param name="services"></param>
        public static void AddCustomApiVersioning(this IServiceCollection services)
        {
            _ = services.AddApiVersioning(options => { options.ReportApiVersions = true; });

            _ = services.AddVersionedApiExplorer(
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
        public static void AddSwaggerConfiguration(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "oauth2 Access token to authenticate and authorize the user",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
                c.MapType<ShapeProperties>(() => new OpenApiSchema
                {
                    Type = "string",
                    Enum = Enum.GetNames(typeof(ShapeProperties))
                        .Select(name => new OpenApiString(name))
                        .Cast<IOpenApiAny>()
                        .ToList(),
                });

                var xmlFilePath = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
                c.UseAllOfToExtendReferenceSchemas();
                c.EnableAnnotations();
                c.ExampleFilters();
                c.IncludeXmlComments(xmlFilePath);
            });

            services.AddSwaggerExamplesFromAssemblies(Assembly.GetEntryAssembly());

        }

        /// <summary>
        /// Open Telemetry Tracing Custom
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="env"></param>
        public static void AddOpenTelemetryTracingCustom(this IServiceCollection services, IConfiguration configuration, IHostEnvironment env)
        {
            var serviceVersion = Assembly.GetEntryAssembly()?.GetName().Version?.ToString();

            var commonConfig = configuration.Get<MicroService.Common.Configuration.ApplicationOptions>();
            if (commonConfig!.JaegerConfiguration.Enabled)
            {
                services.AddOpenTelemetry().WithTracing(tracerProviderBuilder =>
                {
                    tracerProviderBuilder
                        .AddConsoleExporter()
                        .AddSource(env.ApplicationName).SetResourceBuilder(ResourceBuilder.CreateDefault()
                          .AddService(serviceName: env.ApplicationName, serviceVersion: serviceVersion))
                          .AddHttpClientInstrumentation()
                          .AddAspNetCoreInstrumentation();
                });
            }
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
        /// Configure Swagger
        /// </summary>
        /// <param name="app"></param>
        /// <param name="provider"></param>
        public static void UseSwaggerCustom(this IApplicationBuilder app, IApiVersionDescriptionProvider provider)
        {
            app.UseSwagger();

            app.UseSwaggerUI(options =>
            {
                provider.ApiVersionDescriptions
                    .Select(description => new
                    {
                        Url = $"/swagger/{description.GroupName}/swagger.json",
                        Title = $"MicroService.WebApi - {description.GroupName.ToUpperInvariant()}"
                    })
                    .ToList()
                    .ForEach(x => options.SwaggerEndpoint(x.Url, x.Title));
            });
        }

        /// <summary>
        /// 
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
