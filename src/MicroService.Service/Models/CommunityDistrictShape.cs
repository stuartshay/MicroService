using MicroService.Service.Models.Base;
using MicroService.Service.Models.Enum;
using MicroService.Service.Models.Enum.Attibutes;
using MicroService.Service.Models.Enum.Attributes;

namespace MicroService.Service.Models
{
    [ShapeProperties(ShapeProperties.CommunityDistricts)]
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
