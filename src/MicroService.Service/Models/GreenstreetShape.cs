using MicroService.Service.Models.Base;
using MicroService.Service.Models.Enum;
using MicroService.Service.Models.Enum.Attributes;

namespace MicroService.Service.Models
{
    /// <summary>
    /// A Greenstreet (NYC Parks, NYC Open Data mk9u-qu7i): a planted traffic island or median.
    /// </summary>
    [ShapeProperties(ShapeProperties.Greenstreets)]
    public class GreenstreetShape : ShapeBase
    {
        [FeatureCollection]
        [FeatureName("gispropnum")]
        public string PropertyNumber { get; set; } = string.Empty;

        [FeatureCollection]
        [FeatureName("sitename")]
        public string SiteName { get; set; } = string.Empty;

        [FeatureCollection]
        [FeatureName("location")]
        public string Location { get; set; } = string.Empty;

        /// <summary>B, M, Q, R or X</summary>
        [FeatureCollection]
        [FeatureName("borough")]
        public string Borough { get; set; } = string.Empty;

        [FeatureCollection]
        [FeatureName("featuresta")]
        public string Status { get; set; } = string.Empty;

        [FeatureCollection]
        [FeatureName("acres")]
        public double Acres { get; set; }
    }
}
