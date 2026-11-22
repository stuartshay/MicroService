using MicroService.Service.Models.Base;
using MicroService.Service.Models.Enum;
using MicroService.Service.Models.Enum.Attributes;

namespace MicroService.Service.Models
{
    /// <summary>
    /// A public library branch (NYC Open Data feuq-due4, last updated 2016): NYPL, BPL or QPL.
    /// </summary>
    [ShapeProperties(ShapeProperties.Libraries)]
    public class LibraryShape : ShapeBase
    {
        [FeatureCollection]
        [FeatureName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>NYPL, BPL or QPL</summary>
        [FeatureCollection]
        [FeatureName("system")]
        public string System { get; set; } = string.Empty;

        [FeatureCollection]
        public string Address { get; set; } = string.Empty;

        [FeatureCollection]
        [FeatureName("city")]
        public string City { get; set; } = string.Empty;

        [FeatureCollection]
        [FeatureName("zip")]
        public string ZipCode { get; set; } = string.Empty;

        [FeatureCollection]
        [FeatureName("url")]
        public string Url { get; set; } = string.Empty;

        [FeatureCollection]
        [FeatureName("bbl")]
        public string Bbl { get; set; } = string.Empty;
    }
}
