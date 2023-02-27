using MicroService.Service.Models.Base;
using MicroService.Service.Models.Enum;

namespace MicroService.Service.Models
{
    public class DsnyDistrictsShape : ShapeBase
    {
        [FeatureName("district")]
        public string District { get; set; }

        [FeatureName("districtco")]
        public int DistrictCode { get; set; }


        public string OperationZone { get; set; }


        public string OperationZoneName { get; set; }
    }
}