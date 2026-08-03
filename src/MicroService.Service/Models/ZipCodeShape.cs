using MicroService.Service.Models.Base;
using MicroService.Service.Models.Enum;
using MicroService.Service.Models.Enum.Attributes;

namespace MicroService.Service.Models
{
    [ShapeProperties(ShapeProperties.ZipCodes)]
    public class ZipCodeShape : ShapeBase
    {
        [FeatureName("ZIPCODE")]
        public string ZipCode { get; set; } = string.Empty;

        [FeatureName("BLDGZIP")]
        public string BldgZip { get; set; } = string.Empty;

        [FeatureName("PO_NAME")]
        public string PostOfficeName { get; set; } = string.Empty;

        [FeatureName("POPULATION")]
        public int Population { get; set; }

        [FeatureName("AREA")]
        public double Area { get; set; }

        [FeatureName("STATE")]
        public string State { get; set; } = string.Empty;

        [FeatureName("COUNTY")]
        public string County { get; set; } = string.Empty;

        [FeatureName("ST_FIPS")]
        public string StateFibs { get; set; } = string.Empty;

        [FeatureName("CTY_FIPS")]
        public string CityFibs { get; set; } = string.Empty;

        [FeatureName("URL")]
        public string Url { get; set; } = string.Empty;

    }
}
