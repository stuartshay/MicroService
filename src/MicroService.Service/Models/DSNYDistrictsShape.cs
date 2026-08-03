using MicroService.Service.Models.Base;
using MicroService.Service.Models.Enum;
using MicroService.Service.Models.Enum.Attributes;

namespace MicroService.Service.Models
{
    [ShapeProperties(ShapeProperties.DSNYDistricts)]
    public class DsnyDistrictsShape : ShapeBase
    {
        [FeatureName("district")]
        public string District { get; set; } = string.Empty;

        [FeatureName("districtco")]
        public int DistrictCode { get; set; }

        [FeatureName("fid")]
        public string Fid { get; set; } = string.Empty;

        [FeatureName("globalid")]
        public string GlobalId { get; set; } = string.Empty;

        public string OperationZone { get; set; } = string.Empty;


        public string OperationZoneName { get; set; } = string.Empty;
    }
}