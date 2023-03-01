using MicroService.Service.Models.Base;
using MicroService.Service.Models.Enum;

namespace MicroService.Service.Models
{
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
