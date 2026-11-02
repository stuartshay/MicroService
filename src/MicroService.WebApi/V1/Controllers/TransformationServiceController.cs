using Asp.Versioning;
using MicroService.Service.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace MicroService.WebApi.V1.Controllers
{
    /// <summary>
    ///  Feature Service Controller
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class TransformationServiceController : ControllerBase
    {
        /// <summary>
        /// Wgs84 Conversion to Nad83 - New York Long Island
        /// </summary>
        /// <param name="latitude">Latitude (Wgs84)</param>
        /// <param name="longitude">Longitude (Wgs84)</param>
        /// <returns></returns>
        [HttpGet]
        [Route("ConvertWgs84ToNad83")]
        [Produces("application/json")]
        [ProducesResponseType(typeof((double?, double?)), 200)]
        [ProducesResponseType(404)]
        public ActionResult<(double?, double?)> ConvertWgs84ToNad83(double latitude, double longitude)
        {
            if (IsMissing(latitude) || IsMissing(longitude))
                return NoContent();

            var result = GeoTransformationHelper.ConvertWgs84ToNad83(latitude, longitude);
            var point = new { X = result.Item1, Y = result.Item2 };

            return Ok(point);
        }

        /// <summary>
        /// Nad83 New York Long Island  Convert to Wgs84
        /// </summary>
        /// <param name="x">X (Nad83)</param>
        /// <param name="y">Y {Nad83}</param>
        /// <returns></returns>
        [HttpGet]
        [Route("ConvertNad83ToWgs84")]
        [Produces("application/json")]
        [ProducesResponseType(typeof((double?, double?)), 200)]
        [ProducesResponseType(404)]
        public ActionResult<(double?, double?)> ConvertNad83ToWgs84(double x, double y)
        {
            if (IsMissing(x) || IsMissing(y))
                return NoContent();

            var result = GeoTransformationHelper.ConvertNad83ToWgs84(x, y);
            var point = new { X = result.Item1, Y = result.Item2 };

            return Ok(point);
        }

        // A coordinate of 0 (or -0.0) means "not supplied".
        private static bool IsMissing(double value) => value is 0d;
    }
}
