using MicroService.Service.Models.Base;
using MicroService.Service.Models.Enum;
using MicroService.Service.Models.Enum.Attibutes;
using MicroService.Service.Models.Enum.Attributes;

namespace MicroService.Service.Models
{
    [ShapeProperties(ShapeProperties.NeighborhoodTabulationAreas)]
    public class NeighborhoodTabulationAreaShape : ShapeBase
    {
        [FeatureName("BoroCode")]
        public int BoroCode { get; set; }

        [FeatureName("BoroName")]
        public string BoroName { get; set; }

        [FeatureName("CountyFIPS")]
        public string CountyFIPS { get; set; }

        [FeatureName("NTA2020")]
        public string NTA2020 { get; set; }

        [FeatureName("NTAName")]
        public string NTAName { get; set; }

        [FeatureName("NTAAbbrev")]
        public string NTAAbbrev { get; set; }

        [FeatureName("NTAType")]
        public int NTAType { get; set; }

        [FeatureName("CDTA2020")]
        public string CDTA2020 { get; set; }

        [FeatureName("CDTAName")]
        public string CDTAName { get; set; }
    }
}
