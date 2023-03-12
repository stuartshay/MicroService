using MicroService.Service.Models.Base;
using MicroService.Service.Models.Enum;
using MicroService.Service.Models.Enum.Attibutes;
using MicroService.Service.Models.Enum.Attributes;

namespace MicroService.Service.Models
{
    [ShapeProperties(ShapeProperties.NypdPolicePrecincts)]
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
