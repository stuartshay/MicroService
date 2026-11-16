using MicroService.WebApi.Extensions.Constants;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace MicroService.WebApi.Extensions.OpenApi
{
    /// <summary>
    /// Sets the document's title, contact and license, and the Bearer security scheme,
    /// as the Swashbuckle configuration did.
    /// </summary>
    public sealed class DocumentInfoTransformer : IOpenApiDocumentTransformer
    {
        private const string BearerScheme = "Bearer";

        /// <inheritdoc />
        public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(document);
            ArgumentNullException.ThrowIfNull(context);

            document.Info = new OpenApiInfo
            {
                Title = ApiConstants.ApplicationName,
                Version = context.DocumentName.TrimStart('v') + ".0",
                Description = ApiConstants.ApplicationDescription,
                Contact = new OpenApiContact { Name = ApiConstants.OpenApiContactName, Email = ApiConstants.OpenApiContactEmail },
                License = new OpenApiLicense { Name = "MIT", Url = new Uri("https://github.com/stuartshay/MicroService") },
            };

            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
            document.Components.SecuritySchemes[BearerScheme] = new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "oauth2 Access token to authenticate and authorize the user",
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = BearerScheme,
            };
            document.Security ??= [];
            document.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference(BearerScheme, document)] = [],
            });

            return Task.CompletedTask;
        }
    }
}
