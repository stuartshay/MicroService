using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MicroService.Data.Models;
using MicroService.Data.Repository;
using MicroService.Service.Constants;
using MicroService.Service.Interfaces;
using MicroService.Service.Services;
using MicroService.WebApi.Extensions.Constants;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace MicroService.WebApi.V1.Controllers
{
    /// <summary>
    ///  Test Data Controller
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    [ApiVersion("3.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [EnableCors(ApiConstants.CorsPolicy)]
    public class TestDataController : ControllerBase
    {
        private readonly ITestDataRepository _testDataRepository;

        private readonly ICalculationService _calculationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestDataController"/> class.
        ///  TestDataController
        /// </summary>
        /// <param name="testDataRepository"></param>
        /// <param name="calculationService"></param>
        public TestDataController(ITestDataRepository testDataRepository, ICalculationService calculationService)
        {
            _testDataRepository = testDataRepository ?? throw new ArgumentNullException(nameof(testDataRepository));
            _calculationService = calculationService ?? throw new ArgumentNullException(nameof(calculationService));
        }

        /// <summary>
        ///     Get Test Data Set Dump.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Produces("application/json", Type = typeof(IEnumerable<TestData>))]
        [ProducesResponseType(typeof(IEnumerable<TestData>), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IEnumerable<TestData>>> Get()
        {
            var results = await _testDataRepository.FindAll().ConfigureAwait(false);
            if (results == null)
                return NotFound();

            return Ok(results);
        }

        /// <summary>
        /// Get Test Data Percentile.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("percentile")]
        [Produces("application/json", Type = typeof(IEnumerable<TestData>))]
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
