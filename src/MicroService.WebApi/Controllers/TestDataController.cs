using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MicroService.Data.Models;
using MicroService.Data.Repository;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using MicroService.Service.Constants;
using MicroService.Service.Services;

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
            return Ok(results);
        }

        /// <summary>
        /// Get Test Data Percentile.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("/percentile")]
        [Produces("application/json", Type = typeof(IEnumerable<TestData>))]
        [ProducesResponseType(typeof(double), 200)]
        public async Task<IActionResult> GetPercentile()
        {
            var results = await _calculationService.CalculatePercentile(DataConstants.ExcelPercentile);
            return Ok(results);
        }

    }
}
