using Asp.Versioning;
using MicroService.Data.Enum;
using MicroService.Service.Models.Subway;
using MicroService.Service.Services.Subway;
using Microsoft.AspNetCore.Mvc;
using NetTopologySuite.Features;

namespace MicroService.WebApi.V1.Controllers
{
    /// <summary>
    ///  Subway Controller: the MTA's subway services, their lines for a map, and the stations near a set of points
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class SubwayController : ControllerBase
    {
        // The data changes only with a deploy
        private const string CacheControl = "public, max-age=86400";

        private readonly SubwayRouteService _routes;

        private readonly SubwayStationLookupService _stations;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubwayController"/> class.
        /// </summary>
        /// <param name="routes"></param>
        /// <param name="stations"></param>
        public SubwayController(SubwayRouteService routes, SubwayStationLookupService stations)
        {
            _routes = routes;
            _stations = stations;
        }

        /// <summary>
        ///  Get Subway Services
        /// </summary>
        /// <remarks>
        /// Every service in the MTA's Subway Service Lines, 1-7 then letters, with its MTA color and the
        /// color of text on it. Shuttles are SF (Franklin Avenue), SR (Rockaway Park) and ST (42 Street).
        /// </remarks>
        /// <returns>The services</returns>
        [HttpGet("services", Name = "GetSubwayServices")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(IReadOnlyList<SubwayRoute>), StatusCodes.Status200OK, Description = "The services and their colors")]
        public ActionResult<IReadOnlyList<SubwayRoute>> GetServices()
        {
            var services = _routes.GetServices();
            Response.Headers.CacheControl = CacheControl;
            return Ok(services);
        }

        /// <summary>
        ///  Get Subway Lines
        /// </summary>
        /// <remarks>
        /// One MultiLineString feature per service in WGS84, with the properties service, name, color and
        /// textColor. Example: <c>GET /api/v1/Subway/lines?services=1,2,3&amp;borough=MN&amp;simplify=5</c>
        ///
        /// With stations, each line keeps only the stretch between the map's stations on it: a station is on
        /// a service when it lies within radius metres of the service's line (after the borough clip), and each
        /// connected part of the line keeps the line from its first to its last station, carrying on across a
        /// junction to another part with stations. A part with a single station keeps stub metres each side of
        /// it. A service with no station on it is left out, and each feature also has stations, how many of the
        /// stations are on it. Example:
        /// <c>GET /api/v1/Subway/lines?services=1&amp;borough=MN&amp;stations=40.715556,-74.009722;40.860593,-73.926345</c>
        /// </remarks>
        /// <param name="services">Comma-separated service codes, e.g. 1,2,3,A,SIR. Codes match regardless of case,
        /// and S means every shuttle (SF, SR, ST). Leave out for every service.</param>
        /// <param name="borough">Clip each line to a borough: MN, BX, BK, QN or SI. A service with nothing left is left out.</param>
        /// <param name="simplify">Simplification tolerance in metres, 0 (none) to 50. Default 5.</param>
        /// <param name="stations">The map's stations as lat,lon in decimal degrees, separated by ';'. At most 300, in any
        /// order. Trims each line to the stretch between them; leave out for whole lines.</param>
        /// <param name="radius">With stations: how near a line, in metres, a station must be to count for it, 10 to 500.
        /// Default 150.</param>
        /// <param name="stub">With stations: metres of line kept each side of a lone station, 0 (none: the part is
        /// dropped) to 2000. Default 400.</param>
        /// <returns>A GeoJSON FeatureCollection</returns>
        [HttpGet("lines", Name = "GetSubwayLines")]
        [Produces("application/geo+json")]
        [ProducesResponseType(typeof(FeatureCollection), StatusCodes.Status200OK, Description = "The service lines")]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public IActionResult GetLines(
            [FromQuery] string? services = null,
            [FromQuery] string? borough = null,
            [FromQuery] double? simplify = null,
            [FromQuery] string? stations = null,
            [FromQuery] double? radius = null,
            [FromQuery] double? stub = null)
        {
            var (resolved, unknown) = _routes.ResolveServices(services?.Split(','));
            if (unknown.Count > 0)
            {
                ModelState.AddModelError(nameof(services), $"Unknown service codes: {string.Join(", ", unknown)}. GET api/v1/Subway/services lists them.");
            }

            Borough? boroughCode = null;
            if (!string.IsNullOrWhiteSpace(borough))
            {
                if (TryParseBorough(borough.Trim(), out var parsed))
                {
                    boroughCode = parsed;
                }
                else
                {
                    ModelState.AddModelError(nameof(borough), $"Unknown borough '{borough}'. Use MN, BX, BK, QN or SI.");
                }
            }

            var tolerance = simplify ?? SubwayRouteService.DefaultSimplifyMetres;
            if (!double.IsFinite(tolerance) || tolerance < 0 || tolerance > SubwayRouteService.MaxSimplifyMetres)
            {
                ModelState.AddModelError(nameof(simplify), $"simplify is in metres, 0 to {SubwayRouteService.MaxSimplifyMetres}.");
            }

            List<(double Latitude, double Longitude)> points = [];
            var hasStations = !string.IsNullOrWhiteSpace(stations);
            if (hasStations && !SubwayStationLookupService.TryParsePoints(stations, nameof(stations), out points, out var error))
            {
                ModelState.AddModelError(nameof(stations), error);
            }

            var radiusMetres = radius ?? SubwayStretch.DefaultRadiusMetres;
            if (!double.IsFinite(radiusMetres) || radiusMetres < SubwayStretch.MinRadiusMetres || radiusMetres > SubwayStretch.MaxRadiusMetres)
            {
                ModelState.AddModelError(nameof(radius), $"radius is in metres, {SubwayStretch.MinRadiusMetres} to {SubwayStretch.MaxRadiusMetres}.");
            }

            var stubMetres = stub ?? SubwayStretch.DefaultStubMetres;
            if (!double.IsFinite(stubMetres) || stubMetres < 0 || stubMetres > SubwayStretch.MaxStubMetres)
            {
                ModelState.AddModelError(nameof(stub), $"stub is in metres, 0 to {SubwayStretch.MaxStubMetres}.");
            }

            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var stretch = hasStations ? new SubwayStretch(points, radiusMetres, stubMetres) : null;
            var geoJson = _routes.GetLinesGeoJson(resolved, boroughCode, tolerance, stretch);
            Response.Headers.CacheControl = CacheControl;
            return Content(geoJson, "application/geo+json");
        }

        /// <summary>
        ///  Get Subway Stations Near Points
        /// </summary>
        /// <remarks>
        /// For each point, the MTA stations within the radius, nearest first, with their daytime routes;
        /// and services, the union of those routes, 1-7 then letters (S is a shuttle). Example:
        /// <c>GET /api/v1/Subway/stations?near=40.7527,-73.9772;40.7066,-74.0110&amp;radius=100</c>
        /// </remarks>
        /// <param name="near">Points as lat,lon in decimal degrees, separated by ';'. At most 300.</param>
        /// <param name="radius">Search radius in metres, 10 to 500. Default 100.</param>
        /// <returns>The stations near each point and the services they share</returns>
        [HttpGet("stations", Name = "GetSubwayStationsNear")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(SubwayStationsNear), StatusCodes.Status200OK, Description = "The stations near each point")]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public ActionResult<SubwayStationsNear> GetStations(
            [FromQuery] string? near = null,
            [FromQuery] double? radius = null)
        {
            if (!SubwayStationLookupService.TryParsePoints(near, out var points, out var error))
            {
                ModelState.AddModelError(nameof(near), error);
            }

            var radiusMetres = radius ?? SubwayStationLookupService.DefaultRadiusMetres;
            if (!double.IsFinite(radiusMetres)
                || radiusMetres < SubwayStationLookupService.MinRadiusMetres
                || radiusMetres > SubwayStationLookupService.MaxRadiusMetres)
            {
                ModelState.AddModelError(nameof(radius), $"radius is in metres, {SubwayStationLookupService.MinRadiusMetres} to {SubwayStationLookupService.MaxRadiusMetres}.");
            }

            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var result = _stations.FindNear(points, radiusMetres);
            Response.Headers.CacheControl = CacheControl;
            return Ok(result);
        }

        private static bool TryParseBorough(string value, out Borough borough)
        {
            // Names only: Enum.TryParse would also take a number such as "1"
            var name = Enum.GetNames<Borough>().FirstOrDefault(n => string.Equals(n, value, StringComparison.OrdinalIgnoreCase));
            borough = name is null ? default : Enum.Parse<Borough>(name);
            return name is not null;
        }
    }
}
