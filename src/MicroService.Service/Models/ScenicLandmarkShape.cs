using MicroService.Service.Models.Base;
using MicroService.Service.Models.Enum.Attibutes;

namespace MicroService.Service.Models
{
    public class ScenicLandmarkShape : ShapeBase, ILandmark
    {
        [FeatureCollection]
        [FeatureName("lp_number")]
        public string LPNumber { get; set; }

        [FeatureCollection]
        [FeatureName("scen_lm_na")]
        public string AreaName { get; set; }

        [FeatureCollection]
        [FeatureName("borough")]
        public string BoroName { get; set; }

        [FeatureCollection]
        public int BoroCode { get; set; }
    }
}
