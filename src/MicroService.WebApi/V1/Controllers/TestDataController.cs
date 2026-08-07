using Asp.Versioning;
using MicroService.Data.Models;
using MicroService.Data.Repository;
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
    [ApiExplorerSettings(IgnoreApi = true)]
    [EnableCors(ApiConstants.CorsPolicy)]
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
            _testDataRepository = testDataRepository ?? throw new ArgumentNullException(nameof(testDataRepository));
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
    }
}
