using MicroService.Service.Models.Base;
using MicroService.Service.Models.Enum;

namespace MicroService.Service.Models
{
    public class IndividualLandmarkSiteShape : ShapeBase, ILandmark
    {
        public int BoroCode { get; set; }

        [FeatureName("lpc_lpnumb")]
        public string LPNumber { get; set; }

        [FeatureName("lpc_name")]
        public string AreaName { get; set; }

        [FeatureName("bbl")]
        public double BBL { get; set; }

        [FeatureName("borough")]
        public string BoroName { get; set; }
    }
}
