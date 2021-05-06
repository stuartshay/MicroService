using System;
using System.Collections.Generic;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MicroService.WebApi.Extensions.Swagger
{
    /// <summary>
    ///
    /// </summary>
    public class SwaggerDocumentFilter : IDocumentFilter
    {
        /// <inheritdoc/>
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            if (swaggerDoc == null)
            {
                throw new ArgumentNullException(nameof(swaggerDoc));
            }

            swaggerDoc.Tags = new List<OpenApiTag> { new OpenApiTag { Name = "RoutingApi", Description = "This is a description for the api routes" }, };
        }
    }
}
