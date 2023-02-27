using MicroService.Service.Models.Base;
using MicroService.Service.Models.Enum;

namespace MicroService.Service.Models
{
    public class ScenicLandmarkShape : ShapeBase, ILandmark
    {
        [FeatureName("lp_number")]
        public string LPNumber { get; set; }

        [FeatureName("scen_lm_na")]
        public string AreaName { get; set; }

        [FeatureName("borough")]
        public string BoroName { get; set; }

        public int BoroCode { get; set; }
    }
}
