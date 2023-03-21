using MicroService.Service.Models.Enum;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MicroService.WebApi.Models
{
    /// <summary>
    /// Feature Attribute Request Model
    /// </summary>
    public class FeatureAttributeRequestModel
    {
        /// <summary>
        /// Lookup Service Key
        /// </summary>
        [SwaggerParameter("Shape Property Key")]
        [EnumDataType(typeof(ShapeProperties))]
        [DefaultValue(ShapeProperties.BoroughBoundaries)]
        public ShapeProperties Key { get; set; }
    }
}