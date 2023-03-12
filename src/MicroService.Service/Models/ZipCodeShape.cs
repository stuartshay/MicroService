using MicroService.Service.Models.Base;
using MicroService.Service.Models.Enum;
using MicroService.Service.Models.Enum.Attibutes;
using MicroService.Service.Models.Enum.Attributes;

namespace MicroService.Service.Models
{
    [ShapeProperties(ShapeProperties.ZipCodes)]
    public class ZipCodeShape : ShapeBase
    {
        [FeatureName("ZIPCODE")]
        public string ZipCode { get; set; }

        [FeatureName("BLDGZIP")]
        public string BldgZip { get; set; }

        [FeatureName("PO_NAME")]
        public string PostOfficeName { get; set; }

        [FeatureName("POPULATION")]
        public int Population { get; set; }

        [FeatureName("AREA")]
        public double Area { get; set; }

        [FeatureName("STATE")]
        public string State { get; set; }

        [FeatureName("COUNTY")]
        public string County { get; set; }

        [FeatureName("ST_FIPS")]
        public string StateFibs { get; set; }

        [FeatureName("CTY_FIPS")]
        public string CityFibs { get; set; }

        [FeatureName("URL")]
        public string Url { get; set; }

    }
}
