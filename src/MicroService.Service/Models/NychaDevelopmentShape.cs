using MicroService.Service.Models.Base;
using MicroService.Service.Models.Enum;
using MicroService.Service.Models.Enum.Attributes;

namespace MicroService.Service.Models
{
    [ShapeProperties(ShapeProperties.NychaDevelopments)]
    public class NychaDevelopmentShape : ShapeBase
    {
        [FeatureName("DEVELOPMEN")]
        public string Development { get; set; } = string.Empty;

        [FeatureName("TDS_NUM")]
        public string TdsNumber { get; set; } = string.Empty;

        [FeatureName("BOROUGH")]
        public string Borough { get; set; } = string.Empty;
    }
}
