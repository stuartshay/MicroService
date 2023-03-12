using MicroService.Service.Models.Base;
using MicroService.Service.Models.Enum.Attibutes;

namespace MicroService.Service.Models
{
    public class DsnyDistrictsShape : ShapeBase
    {
        [FeatureName("district")]
        public string District { get; set; }

        [FeatureName("districtco")]
        public int DistrictCode { get; set; }

        [FeatureName("fid")]
        public string Fid { get; set; }

        [FeatureName("globalid")]
        public string GlobalId { get; set; }

        public string OperationZone { get; set; }


        public string OperationZoneName { get; set; }
    }
}