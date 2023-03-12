using MicroService.Service.Models.Base;
using MicroService.Service.Models.Enum.Attibutes;

namespace MicroService.Service.Models
{
    public class CommunityDistrictShape : ShapeBase
    {
        public int Cd { get; set; }

        [FeatureName("BoroCD")]
        public int BoroCd { get; set; }

        public int BoroCode { get; set; }

        public string Borough { get; set; }

        public string BoroName { get; set; }

    }
}
