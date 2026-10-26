using MicroService.Service.Models.Base;
using MicroService.Service.Models.Enum;
using MicroService.Service.Models.Enum.Attributes;
using System;

namespace MicroService.Service.Models
{
    [ShapeProperties(ShapeProperties.NationalRegisterHistoricPlaces)]
    public class NationalRegisterHistoricPlacesShape : ShapeBase
    {
        [FeatureName("resource_n")]
        public string ResourceName { get; set; } = string.Empty;

        [FeatureName("county")]
        public string County { get; set; } = string.Empty;

        [FeatureName("date_natio")]
        public DateTime DateDesignated { get; set; }

        [FeatureName("time_natio")]
        public string TimeDesignated { get; set; } = string.Empty;

        [FeatureName("sphinx_num")]
        public string SphinxNumber { get; set; } = string.Empty;

        [FeatureName("x")]
        public double X { get; set; }

        [FeatureName("y")]
        public double Y { get; set; }
    }
}
