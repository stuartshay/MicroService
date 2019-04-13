using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MicroService.Data.Models;
using MicroService.Data.Repository;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="TestDataController"/> class.
        ///  TestDataController
        /// </summary>
        /// <param name="testDataRepository"></param>
        public TestDataController(ITestDataRepository testDataRepository)
        {
            this._testDataRepository = testDataRepository ?? throw new ArgumentNullException(nameof(testDataRepository));
        }

        /// <summary>
        ///     Get Test Data
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Produces("application/json", Type = typeof(IEnumerable<TestData>))]
        [ProducesResponseType(typeof(IEnumerable<TestData>), 200)]
        public async Task<IActionResult> Get()
        {
            var results = await _testDataRepository.FindAll();
            return Ok(results.Take(100).OrderBy(x => x.Id));
        }
    }
}
