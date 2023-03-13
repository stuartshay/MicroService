using MicroService.Service.Models.Base;
using MicroService.Service.Models.Enum;
using MicroService.Service.Models.Enum.Attributes;

namespace MicroService.Service.Models
{
    [ShapeProperties(ShapeProperties.HistoricDistricts)]
    public class HistoricDistrictShape : ShapeBase, ILandmark
    {
        [FeatureName("lp_number")]
        public string LPNumber { get; set; }

        [FeatureName("area_name")]
        public string AreaName { get; set; }

        [FeatureName("borough")]
        public string BoroName { get; set; }

        public int BoroCode { get; set; }
    }
}
