using MicroService.Service.Helpers;
using MicroService.WebApi.Extensions.Constants;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace MicroService.WebApi.V1.Controllers
{
    /// <summary>
    ///  Feature Service Controller
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [EnableCors(ApiConstants.CorsPolicy)]
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
            if (latitude == 0 || longitude == 0)
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
            if (x == 0 || y == 0)
                return NoContent();

            var result = GeoTransformationHelper.ConvertNad83ToWgs84(x, y);
            var point = new { X = result.Item1, Y = result.Item2 };

            return Ok(point);
        }
    }
}
