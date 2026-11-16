using MicroService.WebApi.Extensions.Constants;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.OpenApi;

namespace MicroService.WebApi.Extensions.OpenApi
{
    /// <summary>
    /// The OpenAPI documents (Microsoft.AspNetCore.OpenApi) and the Swagger UI that shows them.
    /// </summary>
    public static class OpenApiExtensions
    {
        /// <summary>
        /// The document route. The Swashbuckle-era route stays as an alias.
        /// </summary>
        public const string DocumentRoute = "/openapi/{documentName}.json";

        /// <summary>
        /// The route Swashbuckle served the document on, kept so existing links keep working.
        /// </summary>
        public const string LegacyDocumentRoute = "/swagger/{documentName}/swagger.json";

        /// <summary>
        /// Registers one OpenAPI document per API version.
        /// </summary>
        /// <param name="services">The service collection.</param>
        public static IServiceCollection AddOpenApiDocuments(this IServiceCollection services)
        {
            // Each document is registered with a literal name: the XML-comment source generator only
            // hooks into AddOpenApi calls whose document name is a string literal. A new API version
            // needs its own call here and an entry in ApiConstants.OpenApiDocuments (for Swagger UI).
            services.AddOpenApi("v1", ConfigureDocument);

            return services;
        }

        private static void ConfigureDocument(OpenApiOptions options)
        {
            // 3.1: what Swagger UI and most client generators read today (3.2 is the .NET 11 default)
            options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_1;
            options.AddDocumentTransformer<DocumentInfoTransformer>();
            options.AddOperationTransformer<ExampleOperationTransformer>();
            options.AddOperationTransformer((operation, _, _) =>
            {
                // A response without an XML <response> comment gets its reason phrase ("OK"), as before
                foreach (var (code, response) in operation.Responses ?? [])
                {
                    if (string.IsNullOrEmpty(response.Description) && response is OpenApiResponse concrete
                        && int.TryParse(code, out var status))
                    {
                        concrete.Description = ReasonPhrases.GetReasonPhrase(status);
                    }
                }

                return Task.CompletedTask;
            });
            options.AddSchemaTransformer((schema, context, _) =>
            {
                // The controllers write enums by name (JsonStringEnumConverter in the MVC JSON
                // options), but the generator reads the minimal-API JSON options, so list the names
                var type = Nullable.GetUnderlyingType(context.JsonTypeInfo.Type) ?? context.JsonTypeInfo.Type;
                if (type.IsEnum)
                {
                    schema.Type = JsonSchemaType.String;
                    schema.Format = null;
                    schema.Enum = [.. Enum.GetNames(type).Select(name => (JsonNode)JsonValue.Create(name))];
                }

                return Task.CompletedTask;
            });
        }

        /// <summary>
        /// Maps the OpenAPI documents and the Swagger UI at <c>/swagger</c>.
        /// </summary>
        /// <param name="app">The web application.</param>
        public static WebApplication MapOpenApiDocuments(this WebApplication app)
        {
            ArgumentNullException.ThrowIfNull(app);

            app.MapOpenApi(DocumentRoute);
            app.MapOpenApi(LegacyDocumentRoute);

            app.UseSwaggerUI(options =>
            {
                foreach (var documentName in ApiConstants.OpenApiDocuments)
                {
                    options.SwaggerEndpoint(DocumentRoute.Replace("{documentName}", documentName, StringComparison.Ordinal),
                        $"{ApiConstants.ApplicationName} - {documentName.ToUpperInvariant()}");
                }
            });

            return app;
        }
    }
}
