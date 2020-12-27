using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using App.Metrics;
using MicroService.Service.Helpers;
using MicroService.Service.Models.Base;
using MicroService.Service.Models.Enum;
using MicroService.WebApi.Extensions;
using MicroService.WebApi.Extensions.Constants;
using MicroService.WebApi.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using NetTopologySuite.IO;
using static MicroService.WebApi.Startup;

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
        private readonly ShapeServiceResolver _shapeServiceResolver;

        private readonly IMetrics _metrics;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeatureServiceController"/> class.
        /// </summary>
        /// <param name="shapeServiceResolver"></param>
        /// <param name="metrics"></param>
        public FeatureServiceController(ShapeServiceResolver shapeServiceResolver, IMetrics metrics)
        {
            _shapeServiceResolver = shapeServiceResolver;
            _metrics = metrics;
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
            var result = EnumHelper.EnumToList<ShapeProperties>().ToList().Select(j => new
                {
                    key = j.ToString(),
                    description = j.GetEnumDescription(),
                    fileName = j.GetAttribute<ShapeAttributes>().FileName,
                    directory = j.GetAttribute<ShapeAttributes>().Directory,
                });

            _metrics.Measure.Counter.Increment(MetricsRegistry.GetShapesCounter);
            return Ok(result);
        }

        /// <summary>
        ///  Get Shape Properties
        /// </summary>
        /// <param name="id">Shape Id</param>
        /// <returns></returns>
        [HttpGet("{id}", Name = "GetShapeProperties")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public ActionResult<object> GetShapeProperties(string id)
        {
            if (id == null)
                return BadRequest();

            var service = _shapeServiceResolver(id);

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

            _metrics.Measure.Counter.Increment(MetricsRegistry.GetShapePropertiesCounter);
            return Ok(result);
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
            _metrics.Measure.Counter.Increment(MetricsRegistry.GetFeatureLookupCounter);

            if (string.IsNullOrEmpty(request?.Key))
                return NoContent();

            var validate = _shapeServiceResolver(ShapeProperties.BoroughBoundaries.ToString()).GetFeatureLookup(request.X, request.Y);
            if (validate == null)
                return new HttpResponseMessage(HttpStatusCode.BadRequest);

            var results = _shapeServiceResolver(request.Key).GetFeatureLookup(request.X, request.Y);
            if (results == null)
                return NotFound();

            _metrics.Measure.Counter.Increment(MetricsRegistry.GetFeatureTypeLookupCounter(request.Key));

            return Ok(results);
        }
    }
}
