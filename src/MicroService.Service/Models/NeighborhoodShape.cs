using MicroService.Service.Models.Base;
using MicroService.Service.Models.Enum;
using MicroService.Service.Models.Enum.Attributes;

namespace MicroService.Service.Models
{
    [ShapeProperties(ShapeProperties.Neighborhoods)]
    public class NeighborhoodShape : ShapeBase
    {
        [FeatureName("BoroCode")]
        public int BoroCode { get; set; }

        [FeatureName("BoroName")]
        public string BoroName { get; set; } = string.Empty;

        [FeatureName("CountyFIPS")]
        public string CountyFIPS { get; set; } = string.Empty;

        [FeatureName("NTACode")]
        public string NTACode { get; set; } = string.Empty;

        [FeatureName("NTAName")]
        public string NTAName { get; set; } = string.Empty;
    }
}
