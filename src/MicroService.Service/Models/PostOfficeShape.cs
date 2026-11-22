using MicroService.Service.Models.Base;
using MicroService.Service.Models.Enum;
using MicroService.Service.Models.Enum.Attributes;

namespace MicroService.Service.Models
{
    /// <summary>
    /// A post office (NYC Open Data bdha-6eqy, last updated 2014).
    /// </summary>
    [ShapeProperties(ShapeProperties.PostOffices)]
    public class PostOfficeShape : ShapeBase
    {
        [FeatureCollection]
        [FeatureName("name")]
        public string Name { get; set; } = string.Empty;

        [FeatureCollection]
        public string Address { get; set; } = string.Empty;

        [FeatureCollection]
        [FeatureName("city")]
        public string City { get; set; } = string.Empty;

        [FeatureCollection]
        [FeatureName("zip")]
        public string ZipCode { get; set; } = string.Empty;

        [FeatureCollection]
        [FeatureName("bbl")]
        public string Bbl { get; set; } = string.Empty;
    }
}
