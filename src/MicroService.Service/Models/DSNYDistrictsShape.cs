using MicroService.Service.Models.Base;
using MicroService.Service.Models.Enum;
using MicroService.Service.Models.Enum.Attributes;

namespace MicroService.Service.Models
{
    [ShapeProperties(ShapeProperties.DSNYDistricts)]
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