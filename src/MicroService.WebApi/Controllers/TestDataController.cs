using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MicroService.Data.Models;
using MicroService.Data.Repository;
using MicroService.Service.Constants;
using MicroService.Service.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace MicroService.WebApi.Controllers
{
    /// <summary>
    ///  Test Data Controller
    /// </summary>
    [Route("api/[controller]")]
    [EnableCors("AllowAll")]
    public class TestDataController : ControllerBase
    {
        private readonly ITestDataRepository _testDataRepository;

        private readonly ICalculationService _calculationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestDataController"/> class.
        ///  TestDataController
        /// </summary>
        /// <param name="testDataRepository"></param>
        public TestDataController(ITestDataRepository testDataRepository, ICalculationService calculationService)
        {
            this._testDataRepository = testDataRepository ?? throw new ArgumentNullException(nameof(testDataRepository));
            this._calculationService = calculationService ?? throw new ArgumentNullException(nameof(calculationService));
        }

        /// <summary>
        ///     Get Test Data Set.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Produces("application/json", Type = typeof(IEnumerable<TestData>))]
        [ProducesResponseType(typeof(IEnumerable<TestData>), 200)]
        public async Task<IActionResult> Get()
        {
            var results = await _testDataRepository.FindAll();
            if (results == null)
                return NotFound();

            return Ok(results);
        }

        /// <summary>
        /// Get Test Data Percentile.
        /// Assume No Results will 0
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("/percentile")]
        [Produces("application/json", Type = typeof(IEnumerable<TestData>))]
        [ProducesResponseType(typeof(double), 200)]
        public async Task<IActionResult> GetPercentile()
        {
            var results = await _calculationService.CalculatePercentile(DataConstants.ExcelPercentile);

            if (Math.Abs(results) < 15)
                return NotFound();

            return Ok(results);
        }

    }
}
