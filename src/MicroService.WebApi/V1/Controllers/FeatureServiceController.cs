using System.Collections.Generic;
using System.Threading.Tasks;
using MicroService.Service.Services;
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
    [ApiVersion("2.0")]
    [ApiVersion("3.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [EnableCors(ApiConstants.CorsPolicy)]
    public class FeatureServiceController : ControllerBase
    {
        private readonly IBoroughBoundariesService _boroughBoundariesService;

        public FeatureServiceController(IBoroughBoundariesService boroughBoundariesService)
        {
            _boroughBoundariesService = boroughBoundariesService;
        }

        /// <summary>
        ///  Get Available Shapes
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Produces("application/json", Type = typeof(IEnumerable<string>))]
        [ProducesResponseType(typeof(IEnumerable<string>), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<object>> Get()
        {
            var results = _boroughBoundariesService.GetShapeDatabaseProperties();

            if (results == null)
                return NotFound();

            return Ok(results.NumRecords);
        }

    }
}
