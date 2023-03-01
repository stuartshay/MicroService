﻿using MicroService.Service.Helpers;
using MicroService.Service.Models.Base;
using MicroService.Service.Models.Enum;
using MicroService.WebApi.Extensions.Constants;
using MicroService.WebApi.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace MicroService.WebApi.V1.Controllers
{
    /// <summary>
    ///  Feature Service Controller
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    [ApiVersion("3.0")]
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
        ///  Get Available Shapes
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Produces("application/json", Type = typeof(IEnumerable<string>))]
        [ProducesResponseType(typeof(IEnumerable<string>), 200)]
        public ActionResult<object> Get()
        {
            var result = EnumHelper.EnumToList<ShapeProperties>().Select(j => new
            {
                key = j.ToString(),
                description = j.GetEnumDescription(),
                fileName = j.GetAttribute<ShapeAttribute>().FileName,
                directory = j.GetAttribute<ShapeAttribute>().Directory,
            });

            _logger.LogInformation("{@ShapeProperties}", result);

            return Ok(result);
        }

        /// <summary>
        ///  Get Shape Attributes
        /// </summary>
        /// <remarks>
        ///   Attribute List of Shape
        /// </remarks>
        /// <param name="id">Shape Id</param>
        /// <returns>Attribute List of Shape </returns>
        [HttpGet("{id}", Name = "GetShapeProperties")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public ActionResult<object> GetShapeProperties(string id)
        {
            if (id == null || !Enum.IsDefined(typeof(ShapeProperties), id))
                return BadRequest();

            var service = _shapeServiceResolver!(id);

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
            if (string.IsNullOrEmpty(request?.Key) || !Enum.IsDefined(typeof(ShapeProperties), request.Key))
                return BadRequest();

            IEnumerable<ShapeBase> results = _shapeServiceResolver!(request.Key).GetFeatureList();
            return await Task.FromResult(Ok(results));
        }

        /// <summary>
        ///  Get Feature Lookup
        /// </summary>
        /// <param name="request">Feature Request</param>
        /// <returns></returns>
        [HttpGet("featurelookup", Name = "GetFeatureLookup")]
        [Produces("application/json", Type = typeof(ShapeBase))]
        [ProducesResponseType(typeof(ShapeBase), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<object>> GetFeatureLookup([FromQuery] FeatureRequestModel request)
        {
            if (string.IsNullOrEmpty(request?.Key) || !Enum.IsDefined(typeof(ShapeProperties), request.Key))
                return BadRequest();

            var validate = _shapeServiceResolver!(ShapeProperties.BoroughBoundaries.ToString()).GetFeatureLookup(request.X, request.Y);
            if (validate == null)
                return NoContent();

            var results = _shapeServiceResolver!(request.Key).GetFeatureLookup(request.X, request.Y);
            if (results == null)
                return NotFound();

            return await Task.FromResult(Ok(results));
        }


        /// <summary>
        ///  Get Feature Attribute Lookup
        /// </summary>
        /// <returns></returns>
        [HttpPost("attributelookup", Name = "GetAttributeLookup")]
        [Produces("application/json", Type = typeof(ShapeBase))]
        [ProducesResponseType(typeof(ShapeBase), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<List<object>>> GetAttributeLookup([FromBody] FeatureAttributeLookupRequestModel request)
        {
            if (string.IsNullOrEmpty(request?.Key) || !Enum.IsDefined(typeof(ShapeProperties), request.Key))
                return BadRequest();

            var keys = request.Attributes!.Select(kv => kv.Key).ToList();
            var service = _shapeServiceResolver!(request?.Key!);

            var shapeType = service.GetType().GetInterface("IShapeService`1")!.GetGenericArguments()[0];
            var fields = shapeType.GetPropertiesWithCustomAttribute<FeatureNameAttribute>().Select(x => x.Name).ToList();

            var invalidItems = keys.Where(x => !fields.Contains(x)).ToList();
            if (invalidItems.Any())
            {
                var invalidFields = string.Join(", ", invalidItems);
                return BadRequest($"The following attributes are not valid for the selected shape type: {invalidFields}");
            }

            var results = _shapeServiceResolver(request!.Key).GetFeatureLookup(request.Attributes);
            if (!results.Any())
                return NotFound();

            return Ok(results);
        }
    }
}
