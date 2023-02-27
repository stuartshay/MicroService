using MicroService.Service.Models.Base;
using MicroService.Service.Models.Enum;

namespace MicroService.Service.Models
{
    public class HistoricDistrictShape : ShapeBase, ILandmark
    {
        [FeatureName("LP_NUMBER")]
        public string LPNumber { get; set; }

        [FeatureName("AREA_NAME")]
        public string AreaName { get; set; }

        [FeatureName("BOROUGH")]
        public string BoroName { get; set; }

        public int BoroCode { get; set; }
    }
}
