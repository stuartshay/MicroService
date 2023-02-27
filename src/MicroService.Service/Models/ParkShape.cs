using MicroService.Service.Models.Base;

namespace MicroService.Service.Models
{
    public class ParkShape : ShapeBase
    {
        public string ParkName { get; set; }

        public string ParkNumber { get; set; }

        public long SourceId { get; set; }

        public int FeatureCode { get; set; }

        public int SubCode { get; set; }

        public string LandUse { get; set; }
    }
}
