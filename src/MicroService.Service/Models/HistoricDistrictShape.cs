using MicroService.Service.Models.Base;
using MicroService.Service.Models.Enum.Attibutes;

namespace MicroService.Service.Models
{
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
