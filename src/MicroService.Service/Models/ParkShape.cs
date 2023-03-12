using MicroService.Service.Models.Base;
using MicroService.Service.Models.Enum;
using MicroService.Service.Models.Enum.Attibutes;
using MicroService.Service.Models.Enum.Attributes;

namespace MicroService.Service.Models
{
    [ShapeProperties(ShapeProperties.Parks)]
    public class ParkShape : ShapeBase
    {
        [FeatureName("PARK_NAME")]
        public string ParkName { get; set; }

        [FeatureName("PARKNUM")]
        public string ParkNumber { get; set; }

        [FeatureName("SOURCE_ID")]
        public long SourceId { get; set; }

        [FeatureName("FEAT_CODE")]
        public int FeatureCode { get; set; }

        [FeatureName("SUB_CODE")]
        public int SubCode { get; set; }

        [FeatureName("LANDUSE")]
        public string LandUse { get; set; }

        [FeatureName("SYSTEM")]
        public string System { get; set; }

        [FeatureName("STATUS")]
        public string Status { get; set; }
    }
}
