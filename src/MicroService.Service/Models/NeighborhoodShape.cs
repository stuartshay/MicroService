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
        public string BoroName { get; set; }

        [FeatureName("CountyFIPS")]
        public string CountyFIPS { get; set; }

        [FeatureName("NTACode")]
        public string NTACode { get; set; }

        [FeatureName("NTAName")]
        public string NTAName { get; set; }
    }
}
