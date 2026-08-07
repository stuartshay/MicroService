using Microsoft.AspNetCore.Mvc.ModelBinding;
using MicroService.Service.Models.Enum;
using MicroService.Service.Models.Enum.Attributes;
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
        [BindRequired]
        public double X { get; set; }

        /// <summary>
        ///  Y Point.
        /// </summary>
        [BindRequired]
        public double Y { get; set; }

        /// <summary>
        /// GeoSpatial Reference System
        /// </summary>
        [BindRequired]
        [EnumDataType(typeof(Datum))]
        [DefaultValue(Datum.Wgs84)]
        public Datum Datum { get; set; }

        /// <summary>
        /// Lookup Service Key
        /// </summary>
        [BindRequired]
        [EnumDataType(typeof(ShapeProperties))]
        [DefaultValue(ShapeProperties.BoroughBoundaries)]
        public ShapeProperties Type { get; set; }
    }

}
