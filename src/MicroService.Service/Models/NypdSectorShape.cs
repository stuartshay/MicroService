using MicroService.Service.Models.Base;
using MicroService.Service.Models.Enum;

namespace MicroService.Service.Models
{
    public class NypdSectorShape : ShapeBase
    {
        [FeatureName("pct")]
        public string Pct { get; set; }

        [FeatureName("sector")]
        public string Sector { get; set; }

        [FeatureName("patrol_bor")]
        public string PatrolBoro { get; set; }

        [FeatureName("phase")]
        public string Phase { get; set; }
    }
}
