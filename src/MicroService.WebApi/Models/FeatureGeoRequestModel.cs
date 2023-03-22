using MicroService.Service.Models.Enum;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MicroService.WebApi.Models
{
    /// <summary>
    /// Feature Request Model
    /// </summary>
    public class FeatureGeoRequestModel
    {
        /// <summary>
        ///  X Point.
        /// </summary>
        public double X { get; set; }

        /// <summary>
        ///  Y Point.
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// Lookup Service Key
        /// </summary>
        [EnumDataType(typeof(ShapeProperties))]
        [DefaultValue(ShapeProperties.BoroughBoundaries)]
        public ShapeProperties Type { get; set; }
    }

}
