using MicroService.Service.Models.Base;

namespace MicroService.Service.Models
{
    public class IndividualLandmarkSiteShape : ShapeBase, ILandmark
    {
        public int BoroCode { get; set; }

        public string LPNumber { get; set; }

        public string AreaName { get; set; }

        public string BoroName { get; set; }
    }
}
