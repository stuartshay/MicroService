using System;
using System.Collections.Generic;
using System.Linq;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MicroService.WebApi.Extensions.Swagger
{
    /// <summary>
    ///
    /// </summary>
    public class SwaggerDocumentFilter : IDocumentFilter
    {
        /// <inheritdoc/>
        public void Apply(SwaggerDocument swaggerDoc, DocumentFilterContext context)
        {
            if (swaggerDoc == null)
            {
                throw new ArgumentNullException(nameof(swaggerDoc));
            }

            swaggerDoc.Tags = new List<Tag> { new Tag { Name = "RoutingApi", Description = "This is a description for the api routes" }, };

            swaggerDoc.Paths = swaggerDoc.Paths.OrderBy(pair => pair.Key).ToDictionary(pair => pair.Key, pair => pair.Value);
        }
    }
}
