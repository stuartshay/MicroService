using MicroService.Service.Helpers;
using MicroService.Service.Interfaces;
using MicroService.Service.Models.Base;
using MicroService.Service.Models.Enum;
using MicroService.Service.Models.Enum.Attributes;
using MicroService.WebApi.Extensions.Constants;
using MicroService.WebApi.Extensions.Swagger.Examples;
using MicroService.WebApi.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using NetTopologySuite.Features;
using NetTopologySuite.IO;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;
using System.ComponentModel.DataAnnotations;

namespace MicroService.WebApi.V1.Controllers
{
    /// <summary>
    ///  Feature Service Controller
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [EnableCors(ApiConstants.CorsPolicy)]
    public class FeatureServiceController : ControllerBase
    {
        private readonly ShapeServiceResolver? _shapeServiceResolver;

        private readonly ILogger<FeatureServiceController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeatureServiceController"/> class.
        /// </summary>
        /// <param name="shapeServiceResolver"></param>
        /// <param name="logger"></param>
        public FeatureServiceController(ShapeServiceResolver? shapeServiceResolver, ILogger<FeatureServiceController> logger)
        {
            _shapeServiceResolver = shapeServiceResolver;
            _logger = logger;
        }

        /// <summary>
        ///  Get List of Available Shapes
        /// </summary>
        /// <returns>An enumerable list of available shape datasets</returns>
        [HttpGet]
        [Produces("application/json", Type = typeof(IEnumerable<string>))]
        [ProducesResponseType(typeof(IEnumerable<object>), 200)]
        [SwaggerResponseExample(200, typeof(GetShapeTypesExample))]
        public ActionResult<object> Get()
        {
            var result = EnumHelper.EnumToList<ShapeProperties>().Select(j => new
            {
                key = j.ToString(),
                description = j.GetEnumDescription(),
                fileName = j.GetAttribute<ShapeAttribute>().FileName,
                directory = j.GetAttribute<ShapeAttribute>().Directory,
                datum = j.GetAttribute<ShapeAttribute>().Datum.ToString(),
            });

            _logger.LogInformation("{@ShapeProperties}", result);

            return Ok(result);
        }

        /// <summary>
        ///  Get Shape Attributes
        /// </summary>
        /// <remarks>
        ///   Attribute List of Shape Properties on the ERSI Shape File
        /// </remarks>
        /// <param name="id">Shape Id</param>
        /// <returns>Attribute List of Shape </returns>
        [HttpGet("{id}", Name = "GetShapeProperties")]
        [Produces("application/json", Type = typeof(object))]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [SwaggerResponseExample(200, typeof(GetShapePropertiesExample))]
        public ActionResult<object> GetShapeProperties(
            [FromRoute][EnumDataType(typeof(ShapeProperties))] ShapeProperties id)
        {
            if (!Enum.IsDefined(typeof(ShapeProperties), id))
                return BadRequest();

            var service = _shapeServiceResolver!(id.ToString());

            var databaseProperties = service.GetShapeDatabaseProperties();
            var shapeProperties = service.GetShapeProperties();

            if (databaseProperties == null)
                return NotFound();

            var result = new
            {
                NumFields = databaseProperties.NumFields,
                NumRecords = databaseProperties.NumRecords,
                Bounds = new
                {
                    MaxX = shapeProperties.Bounds.MaxX,
                    MaxY = shapeProperties.Bounds.MaxY,
                    MinX = shapeProperties.Bounds.MinX,
                    MinY = shapeProperties.Bounds.MinY,
                },
                LastUpdatedDate = databaseProperties.LastUpdateDate,
                FieldsList = databaseProperties.Fields.Select(f => new { f.Name, f.Type.FullName }),
            };

            return Ok(result);
        }

        /// <summary>
        ///  Get Feature Collection
        /// </summary>
        /// <remarks>
        ///   List of features with attributes 
        /// </remarks>
        /// <param name="request">Feature Attribute Request</param>
        /// <returns>List of features with attributes</returns>
        [HttpGet("featureList", Name = "GetFeatureList")]
        [Produces("application/json", Type = typeof(ShapeBase))]
        [ProducesResponseType(typeof(IEnumerable<ShapeBase>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<object>> GetFeatureList([FromQuery] FeatureAttributeRequestModel request)
        {
            if (string.IsNullOrEmpty(request?.Key.ToString()) || !Enum.IsDefined(typeof(ShapeProperties), request.Key))
                return BadRequest();

            IEnumerable<ShapeBase> results = _shapeServiceResolver!(request.Key.ToString()).GetFeatureList();
            return await Task.FromResult(Ok(results));
        }

        /// <summary>
        ///  Get Geospatial Feature Lookup
        /// </summary>
        /// <param name="request">Feature Request</param>
        /// <returns></returns>
        [HttpGet("geospatiallookup", Name = "GetGeospatialLookup")]
        [Produces("application/json", Type = typeof(ShapeBase))]
        [ProducesResponseType(typeof(ShapeBase), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<object>> GetGeospatialLookup([FromQuery] FeatureGeoRequestModel request)
        {
            if (string.IsNullOrEmpty(request?.Type.ToString()) || !Enum.IsDefined(typeof(ShapeProperties), request.Type))
                return BadRequest();

            var validate = _shapeServiceResolver!(ShapeProperties.BoroughBoundaries.ToString()).GetFeatureLookup(request.X, request.Y, request.Datum);
            if (validate == null)
                return NoContent();

            var results = _shapeServiceResolver!(request.Type.ToString()).GetFeatureLookup(request.X, request.Y, request.Datum);
            if (results == null)
                return NotFound();

            return await Task.FromResult(Ok(results));
        }

        /// <summary>
        /// Get Feature Attribute Lookup
        /// </summary>
        [HttpPost("attributelookup", Name = "GetAttributeLookup")]
        [Produces("application/json", Type = typeof(List<object>))]
        [ProducesResponseType(typeof(List<object>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<object>> GetAttributeLookup([FromBody] FeatureAttributeLookupRequestModel request)
        {
            return await FeatureAttributeValidation(request, (service, attributes) =>
            {
                var results = service.GetFeatureLookup(attributes);
                if (!results.Any())
                {
                    return new List<object>();
                }

                return results.Cast<object>().ToList();
            });
        }

        /// <summary>
        ///  Get Feature Attribute Lookup GeoJson
        /// </summary>
        /// <returns></returns>
        [HttpPost("attributelookupgeojson", Name = "GetLookupFeatureGeoJson")]
        [Produces("application/geo+json")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns a list of features", typeof(FeatureCollection))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<FeatureCollection>> GetLookupFeatureGeoJson([FromBody] FeatureAttributeLookupRequestModel request)
        {
            if (string.IsNullOrEmpty(request?.Key) || !Enum.IsDefined(typeof(ShapeProperties), request.Key))
                return BadRequest();

            var keys = request.Attributes!.Select(kv => kv.Key).ToList();
            var service = _shapeServiceResolver!(request?.Key!);

            var shapeType = service.GetType().GetInterface("IShapeService`1")!.GetGenericArguments()[0];

            // Validate Input
            var fields = shapeType.GetPropertiesWithCustomAttribute<FeatureNameAttribute>().Select(x => x.Name).ToList();
            var mappingFields = shapeType.GetPropertiesWithCustomAttribute<MappingKeyAttribute>().Select(x => x.Name).ToList();
            var allFields = fields.Union(mappingFields).ToList();

            var invalidItems = keys.Where(x => !allFields.Contains(x)).ToList();
            if (invalidItems.Any())
            {
                var invalidFields = string.Join(", ", invalidItems);
                return BadRequest($"The following attributes are not valid for the selected shape type: {invalidFields}");
            }

            var featureCollection = _shapeServiceResolver(request!.Key).GetFeatureCollection(request.Attributes);
            var writer = new GeoJsonWriter();
            var geoJsonString = writer.Write(featureCollection);

            var serializer = GeoJsonSerializer.Create();
            using var stringReader = new StringReader(geoJsonString);
            using var jsonReader = new JsonTextReader(stringReader);

            var geometry = serializer.Deserialize<FeatureCollection>(jsonReader);

            return await Task.FromResult(Ok(geometry));
        }


        /// <summary>
        /// Feature Attribute Validation
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request"></param>
        /// <param name="lookupFunction"></param>
        /// <returns></returns>
        public async Task<ActionResult<T>> FeatureAttributeValidation<T>(FeatureAttributeLookupRequestModel request, Func<IShapeService<ShapeBase>, List<KeyValuePair<string, object>>, IEnumerable<T>> lookupFunction)
        {
            if (string.IsNullOrEmpty(request?.Key) || !Enum.IsDefined(typeof(ShapeProperties), request.Key))
            {
                return BadRequest();
            }

            var keys = request.Attributes!.Select(kv => kv.Key).ToList();
            var service = _shapeServiceResolver!(request?.Key!);

            var shapeType = service.GetType().GetInterface("IShapeService`1")!.GetGenericArguments()[0];

            // Validate Input
            var fields = shapeType.GetPropertiesWithCustomAttribute<FeatureNameAttribute>()
                .Select(x => x.Name)
                .ToList();
            var mappingFields = shapeType.GetPropertiesWithCustomAttribute<MappingKeyAttribute>()
                .Select(x => x.Name)
                .ToList();
            var allFields = fields.Union(mappingFields).ToList();

            var invalidItems = keys.Where(x => !allFields.Contains(x)).ToList();
            if (invalidItems.Any())
            {
                var invalidFields = string.Join(", ", invalidItems);
                return BadRequest($"The following attributes are not valid for the selected shape type: {invalidFields}");
            }

            var results = lookupFunction(service, request!.Attributes!).ToList();
            if (!results.Any())
            {
                return NotFound();
            }

            return await Task.FromResult(Ok(results));
        }

    }
}
