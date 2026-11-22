using Asp.Versioning;
using MicroService.Data.Enum;
using MicroService.Service.Services.Streets;
using MicroService.Service.Services.Subway;
using Microsoft.AspNetCore.Mvc;
using NetTopologySuite.Features;

namespace MicroService.WebApi.V1.Controllers
{
    /// <summary>
    ///  Streets Controller: street lines for a map from the city's Street Centerline (CSCL), and street names
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class StreetsController : ControllerBase
    {
        /// <summary>
        /// How near a street, in metres, a place must be to count for it. Default 100.
        /// </summary>
        public const double DefaultRadiusMetres = 100;

        /// <summary>
        /// The street kept beyond the first and last place, in metres. Default 100.
        /// </summary>
        public const double DefaultMarginMetres = 100;

        // The centerline changes a few times a year
        private const string CacheControl = "public, max-age=86400";

        private readonly StreetLineService _lines;
        private readonly StreetCenterlineClient _centerline;
        private readonly ILogger<StreetsController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="StreetsController"/> class.
        /// </summary>
        public StreetsController(StreetLineService lines, StreetCenterlineClient centerline, ILogger<StreetsController> logger)
        {
            _lines = lines;
            _centerline = centerline;
            _logger = logger;
        }

        /// <summary>
        ///  Get Street Lines
        /// </summary>
        /// <remarks>
        /// One MultiLineString feature per street in WGS84, with the properties name and borough. Names are
        /// matched as the Street Centerline spells them; common spellings are converted ("Fifth Avenue" and
        /// "5th Ave" are "5 AVE"; "West 42nd Street" finds "W  42 ST"). A street with no segments in the borough is left out. Example:
        /// <c>GET /api/v1/Streets/lines?names=5 AVE&amp;borough=MN</c>
        ///
        /// With places, each street keeps only the stretch along them: a place is on a street when it lies
        /// within radius metres of it, and the street keeps the line from its first to its last place plus
        /// margin metres each way (a lone place keeps margin metres each side). Each feature then also has
        /// places, how many of the places are along it. Example:
        /// <c>GET /api/v1/Streets/lines?names=5 AVE&amp;borough=MN&amp;places=40.7812,-73.9590;40.7854,-73.9572</c>
        /// </remarks>
        /// <param name="names">Comma-separated street names, 1 to 10, e.g. 5 AVE,MADISON AVE.</param>
        /// <param name="borough">The borough the streets are in: MN, BX, BK, QN or SI. Required: many names are in several boroughs.</param>
        /// <param name="places">The map's places as lat,lon in decimal degrees, separated by ';'. At most 300. Trims each
        /// street to the stretch along them; leave out for whole streets.</param>
        /// <param name="radius">With places: how near a street, in metres, a place must be to count for it, 10 to 500. Default 100.</param>
        /// <param name="margin">With places: metres of street kept beyond the first and last place, 0 to 2000. Default 100.</param>
        /// <param name="simplify">Simplification tolerance in metres, 0 (none) to 50. Default 2.</param>
        /// <param name="cancellationToken">Cancels the request</param>
        /// <returns>A GeoJSON FeatureCollection</returns>
        [HttpGet("lines", Name = "GetStreetLines")]
        [Produces("application/geo+json")]
        [ProducesResponseType(typeof(FeatureCollection), StatusCodes.Status200OK, Description = "The street lines")]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> GetLines(
            [FromQuery] string? names = null,
            [FromQuery] string? borough = null,
            [FromQuery] string? places = null,
            [FromQuery] double? radius = null,
            [FromQuery] double? margin = null,
            [FromQuery] double? simplify = null,
            CancellationToken cancellationToken = default)
        {
            var streets = ValidateNames(names);
            var boroughCode = ValidateBorough(borough, required: true);

            var tolerance = simplify ?? StreetLineService.DefaultSimplifyMetres;
            if (!double.IsFinite(tolerance) || tolerance < 0 || tolerance > StreetLineService.MaxSimplifyMetres)
            {
                ModelState.AddModelError(nameof(simplify), $"simplify is in metres, 0 to {StreetLineService.MaxSimplifyMetres}.");
            }

            var stretch = ValidateStretch(places, radius, margin);
            if (!ModelState.IsValid || boroughCode is null)
            {
                return ValidationProblem(ModelState);
            }

            try
            {
                var geoJson = await _lines.GetLinesGeoJsonAsync(streets, boroughCode.Value, tolerance, stretch, cancellationToken);
                Response.Headers.CacheControl = CacheControl;
                return Content(geoJson, "application/geo+json");
            }
            catch (Exception exception) when (IsCenterlineFailure(exception, cancellationToken))
            {
                return CenterlineUnavailable(exception);
            }
        }

        /// <summary>
        ///  Search Street Names
        /// </summary>
        /// <remarks>
        /// The Street Centerline names that contain the text, most segments first, with their borough. The
        /// text is converted to the centerline's spelling first ("fifth avenue" searches for "5 AVE").
        /// Example: <c>GET /api/v1/Streets/search?q=fifth avenue&amp;borough=MN</c>
        /// </remarks>
        /// <param name="q">The text, 2 to 64 characters.</param>
        /// <param name="borough">Only this borough: MN, BX, BK, QN or SI. Leave out for all.</param>
        /// <param name="cancellationToken">Cancels the request</param>
        /// <returns>At most 20 names</returns>
        [HttpGet("search", Name = "SearchStreetNames")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(IReadOnlyList<StreetName>), StatusCodes.Status200OK, Description = "The matching street names")]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> Search(
            [FromQuery] string? q = null,
            [FromQuery] string? borough = null,
            CancellationToken cancellationToken = default)
        {
            var text = q?.Trim() ?? string.Empty;
            if (text.Length < 2 || !StreetNames.IsValid(text))
            {
                ModelState.AddModelError(nameof(q), $"q is 2 to {StreetNames.MaxLength} letters, figures, spaces, apostrophes or hyphens.");
            }

            var boroughCode = ValidateBorough(borough, required: false);
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            try
            {
                var found = await _centerline.SearchAsync(StreetNames.Normalize(text), boroughCode, cancellationToken);
                Response.Headers.CacheControl = CacheControl;
                return Ok(found);
            }
            catch (Exception exception) when (IsCenterlineFailure(exception, cancellationToken))
            {
                return CenterlineUnavailable(exception);
            }
        }

        /// <summary>
        /// NYC Open Data failed or did not answer in time (the HttpClient timeout cancels the call); a
        /// caller that went away cancels too, and that is not a failure to report.
        /// </summary>
        private static bool IsCenterlineFailure(Exception exception, CancellationToken cancellationToken) =>
            exception is HttpRequestException
            || (exception is OperationCanceledException && !cancellationToken.IsCancellationRequested);

        private ObjectResult CenterlineUnavailable(Exception exception)
        {
            _logger.LogWarning(exception, "The street centerline could not be read");
            return Problem(
                title: "The street centerline could not be read",
                detail: "NYC Open Data did not answer; try again later.",
                statusCode: StatusCodes.Status502BadGateway);
        }

        private List<string> ValidateNames(string? names)
        {
            var list = (names ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();
            if (list.Count == 0 || list.Count > StreetLineService.MaxStreets)
            {
                ModelState.AddModelError(nameof(names), $"names is 1 to {StreetLineService.MaxStreets} comma-separated street names.");
                return [];
            }

            var invalid = list.Where(n => !StreetNames.IsValid(n)).ToList();
            if (invalid.Count > 0)
            {
                ModelState.AddModelError(nameof(names), $"Not a street name: {string.Join(", ", invalid)}.");
            }

            // Repeats of one spelling go here; names that resolve to the same centerline street are
            // drawn once by StreetLineService, which knows the street each one found
            return list.Select(StreetNames.Normalize).Distinct(StringComparer.Ordinal).ToList();
        }

        private Borough? ValidateBorough(string? borough, bool required)
        {
            if (string.IsNullOrWhiteSpace(borough))
            {
                if (required)
                {
                    ModelState.AddModelError(nameof(borough), "borough is required: MN, BX, BK, QN or SI.");
                }

                return null;
            }

            var name = Enum.GetNames<Borough>().FirstOrDefault(n => string.Equals(n, borough.Trim(), StringComparison.OrdinalIgnoreCase));
            if (name is null)
            {
                ModelState.AddModelError(nameof(borough), $"Unknown borough '{borough}'. Use MN, BX, BK, QN or SI.");
                return null;
            }

            return Enum.Parse<Borough>(name);
        }

        private SubwayStretch? ValidateStretch(string? places, double? radius, double? margin)
        {
            List<(double Latitude, double Longitude)> points = [];
            var hasPlaces = !string.IsNullOrWhiteSpace(places);
            if (hasPlaces && !SubwayStationLookupService.TryParsePoints(places, nameof(places), out points, out var error))
            {
                ModelState.AddModelError(nameof(places), error);
            }

            var radiusMetres = radius ?? DefaultRadiusMetres;
            if (!double.IsFinite(radiusMetres) || radiusMetres < SubwayStretch.MinRadiusMetres || radiusMetres > SubwayStretch.MaxRadiusMetres)
            {
                ModelState.AddModelError(nameof(radius), $"radius is in metres, {SubwayStretch.MinRadiusMetres} to {SubwayStretch.MaxRadiusMetres}.");
            }

            var marginMetres = margin ?? DefaultMarginMetres;
            if (!double.IsFinite(marginMetres) || marginMetres < 0 || marginMetres > SubwayStretch.MaxMarginMetres)
            {
                ModelState.AddModelError(nameof(margin), $"margin is in metres, 0 to {SubwayStretch.MaxMarginMetres}.");
            }

            // A lone place keeps the margin each side of it, as a stub
            return hasPlaces && ModelState.IsValid
                ? new SubwayStretch(points, radiusMetres, Math.Min(marginMetres, SubwayStretch.MaxStubMetres), marginMetres)
                : null;
        }
    }
}
