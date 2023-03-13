using MicroService.Service.Models.Base;
using MicroService.Service.Models.Enum;
using MicroService.Service.Models.Enum.Attributes;

namespace MicroService.Service.Models
{
    [ShapeProperties(ShapeProperties.NychaDevelopments)]
    public class NychaDevelopmentShape : ShapeBase
    {
        [FeatureName("DEVELOPMEN")]
        public string Development { get; set; }

        [FeatureName("TDS_NUM")]
        public string TdsNumber { get; set; }

        [FeatureName("BOROUGH")]
        public string Borough { get; set; }
    }
}
