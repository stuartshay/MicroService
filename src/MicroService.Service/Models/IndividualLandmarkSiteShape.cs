using MicroService.Service.Models.Base;

namespace MicroService.Service.Models
{
    public class IndividualLandmarkSiteShape : ShapeBase
    {
        public int BoroCode { get; set; }

        public string BoroName { get; set; }
    }
}
