using System;
using System.Collections.Generic;
using System.Linq;
using MicroService.Service.Helpers;
using MicroService.Service.Models.Enum;
using MicroService.Service.Models.FlatFileModels;
using MicroService.WebApi.Extensions.Constants;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace MicroService.WebApi.V1.Controllers
{
    /// <summary>
    ///  FlatFile Service Controller
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [EnableCors(ApiConstants.CorsPolicy)]
    public class FlatFileServiceController : ControllerBase
    {
        private readonly Startup.FlatFileResolver _flatFileResolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="FlatFileServiceController"/> class.
        /// </summary>
        /// <param name="flatFileResolver"></param>
        public FlatFileServiceController(Startup.FlatFileResolver flatFileResolver)
        {
            _flatFileResolver = flatFileResolver;
        }

        /// <summary>
        /// Get Available Flat Files
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Produces("application/json", Type = typeof(IEnumerable<string>))]
        [ProducesResponseType(typeof(IEnumerable<string>), 200)]
        public ActionResult<object> Get()
        {
            var result = EnumHelper.EnumToList<FlatFileProperties>().ToList().Select(j => new
            {
                key = j.ToString(),
                description = j.GetEnumDescription(),
                fileName = j.GetAttribute<FlatFileAttributes>().FileName,
                directory = j.GetAttribute<FlatFileAttributes>().Directory,
                fileType = j.GetAttribute<FlatFileAttributes>().FileType.ToString(),
            });

            return Ok(result);
        }

        /// <summary>
        ///  Get Flat File
        /// </summary>
        /// <param name="id">Flat File Id</param>
        /// <returns></returns>
        [HttpGet("{id}", Name = "GetFlatFile")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public ActionResult<dynamic> GetFlatFile(string id)
        {
            if (id == null || !Enum.IsDefined(typeof(FlatFileProperties), id))
                return BadRequest();

            var service = _flatFileResolver(id);
            var modelName = Enum.Parse<FlatFileProperties>(id).GetAttribute<FlatFileAttributes>().ModelName;

            if (modelName == "StationFlatFile")
            {
                var results = service.GetAll().Cast<StationFlatFile>().ToList();
                return Ok(results);
            }
            else if (modelName == "StationComplexFlatFile")
            {
                var results = service.GetAll().Cast<StationComplexFlatFile>().ToList();
                return Ok(results);
            }

            return NotFound();
        }
    }
}
