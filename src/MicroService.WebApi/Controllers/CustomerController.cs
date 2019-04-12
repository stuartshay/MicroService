using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using MicroService.Data.Models;
using MicroService.Data.Repository;

namespace MicroService.WebApi.Controllers
{
    /// <summary>
    /// Customer Controller.
    /// </summary>
    [Route("api/[controller]")]
    [EnableCors("AllowAll")]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerRepository _customerRepository;

        /// <summary>
        ///  Customer Controller.
        /// </summary>
        /// <param name="customerRepository"></param>
        public CustomerController(ICustomerRepository customerRepository)
        {
            this._customerRepository = customerRepository;
        }

        /// <summary>
        ///     Get Customers
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Produces("application/json", Type = typeof(IEnumerable<Customer>))]
        [ProducesResponseType(typeof(IEnumerable<Customer>), 200)]
        public IActionResult Get()
        {
            var results = _customerRepository.FindAll();
            return Ok(results.OrderBy(x => x.Name));
        }
    }
}
