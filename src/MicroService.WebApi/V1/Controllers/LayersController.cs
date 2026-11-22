using Asp.Versioning;
using MicroService.Service.Services.Layers;
using Microsoft.AspNetCore.Mvc;
using NetTopologySuite.Features;

namespace MicroService.WebApi.V1.Controllers
{
    /// <summary>
    ///  Layers Controller: shapes a map can draw as layers, and their features in a box
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class LayersController : ControllerBase
    {
        // The shapes change only with a deploy
        private const string CacheControl = "public, max-age=86400";

        private readonly LayerService _layers;

        /// <summary>
        /// Initializes a new instance of the <see cref="LayersController"/> class.
        /// </summary>
        public LayersController(LayerService layers)
        {
            _layers = layers;
        }

        /// <summary>
        ///  List Layers
        /// </summary>
        /// <remarks>
        /// The shapes a navigatorglass.com map can draw as layers: key, name, geometry (polygon or
        /// point), the color every app draws it in, its source, when the source was last updated and
        /// how many features it has.
        /// </remarks>
        /// <returns>The layers</returns>
        [HttpGet(Name = "GetLayers")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(IReadOnlyList<LayerSummary>), StatusCodes.Status200OK, Description = "The layers")]
        public IActionResult Get()
        {
            Response.Headers.CacheControl = CacheControl;
            return Ok(LayerCatalog.All.Select(l => new LayerSummary(
                l.Key, l.Name, l.Geometry, l.Color, l.Source, l.DataUpdated, _layers.Count(l))).ToList());
        }

        /// <summary>
        ///  Get Layer Features
        /// </summary>
        /// <remarks>
        /// The layer's features whose outline touches the box, as a GeoJSON FeatureCollection in
        /// WGS84: polygons simplified to 2 m, coordinates to 6 decimals, at most 5,000 features. Each
        /// feature has the properties title (its name or address) and, when it has any, details
        /// (label to value, e.g. "LP number": "LP-00099"). Example:
        /// <c>GET /api/v1/Layers/HistoricDistricts?bbox=-74.00,40.69,-73.99,40.70</c>
        /// </remarks>
        /// <param name="key">The layer key from GET /api/v1/Layers, any case.</param>
        /// <param name="bbox">west,south,east,north in decimal degrees, in the New York City area, at most 0.5 degrees each way.</param>
        /// <returns>A GeoJSON FeatureCollection</returns>
        [HttpGet("{key}", Name = "GetLayerFeatures")]
        [Produces("application/geo+json")]
        [ProducesResponseType(typeof(FeatureCollection), StatusCodes.Status200OK, Description = "The layer's features in the box")]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public IActionResult GetFeatures(string key, [FromQuery] string? bbox = null)
        {
            var layer = LayerCatalog.Find(key);
            if (layer is null)
            {
                return Problem(
                    title: "Unknown layer",
                    detail: $"There is no layer '{key}'. GET /api/v1/Layers lists them.",
                    statusCode: StatusCodes.Status404NotFound);
            }

            var box = LayerBox.Parse(bbox, out var error);
            if (box is null)
            {
                ModelState.AddModelError(nameof(bbox), error!);
                return ValidationProblem(ModelState);
            }

            Response.Headers.CacheControl = CacheControl;
            return Content(_layers.GetGeoJson(layer, box), "application/geo+json");
        }
    }

    /// <summary>
    /// A layer a map can draw.
    /// </summary>
    /// <param name="Key">The key GET /api/v1/Layers/{key} takes, e.g. HistoricDistricts</param>
    /// <param name="Name">Its name for people</param>
    /// <param name="Geometry">polygon or point</param>
    /// <param name="Color">The color every app draws it in, #RRGGBB</param>
    /// <param name="Source">Where the data comes from</param>
    /// <param name="DataUpdated">When the source was last updated, YYYY-MM-DD</param>
    /// <param name="Features">How many features it has</param>
    public sealed record LayerSummary(string Key, string Name, string Geometry, string Color, string Source, string DataUpdated, int Features);
}
