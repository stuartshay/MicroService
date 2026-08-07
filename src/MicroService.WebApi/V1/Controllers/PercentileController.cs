using Asp.Versioning;
using MicroService.Service.Constants;
using MicroService.Service.Interfaces;
using MicroService.WebApi.Extensions.Constants;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace MicroService.WebApi.V1.Controllers
{
    /// <summary>
    ///  Percentile Controller
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    [ApiVersion("3.0")]
    [Route("api/v{version:apiVersion}/TestData")]
    [ApiExplorerSettings(IgnoreApi = true)]
    [EnableCors(ApiConstants.CorsPolicy)]
    public class PercentileController : ControllerBase
    {
        private readonly ICalculationService _calculationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="PercentileController"/> class.
        /// </summary>
        /// <param name="calculationService"></param>
        public PercentileController(ICalculationService calculationService)
        {
            _calculationService = calculationService ?? throw new ArgumentNullException(nameof(calculationService));
        }

        /// <summary>
        /// Get Test Data Percentile.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("percentile")]
        [Produces("application/json", Type = typeof(double))]
        [ProducesResponseType(typeof(double), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<double>> GetPercentile()
        {
            var results = await _calculationService.CalculatePercentile(DataConstants.ExcelPercentile).ConfigureAwait(false);

            if (Math.Abs(results) < 15)
                return NotFound();

            return Ok(results);
        }
    }
}
