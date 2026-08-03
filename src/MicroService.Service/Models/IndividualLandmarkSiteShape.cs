using MicroService.Service.Models.Base;
using MicroService.Service.Models.Enum;
using MicroService.Service.Models.Enum.Attributes;

namespace MicroService.Service.Models
{
    [ShapeProperties(ShapeProperties.IndividualLandmarkSite)]
    public class IndividualLandmarkSiteShape : ShapeBase, ILandmark
    {
        public int BoroCode { get; set; }

        [FeatureName("objectid")]
        public double ObjectId { get; set; }

        [FeatureName("lpc_lpnumb")]
        public string LPNumber { get; set; } = string.Empty;

        [FeatureName("lpc_name")]
        public string AreaName { get; set; } = string.Empty;

        [FeatureName("lpc_altern")]
        public string AlternativeName { get; set; } = string.Empty;

        [FeatureName("bbl")]
        public double BBL { get; set; }

        [FeatureName("borough")]
        public string BoroName { get; set; } = string.Empty;

        [FeatureName("address")]
        public string Address { get; set; } = string.Empty;

        [FeatureName("block")]
        public double Block { get; set; }

        [FeatureName("lot")]
        public double Lot { get; set; }

        [FeatureName("date_des_d")]
        public string DesignationDate { get; set; } = string.Empty;

        [FeatureName("url_report")]
        public string UrlReport { get; set; } = string.Empty;

        [FeatureName("lpc_site_d")]
        public string SiteDesignation { get; set; } = string.Empty;

        [FeatureName("lpc_site_s")]
        public string DesignationStatus { get; set; } = string.Empty;

        [FeatureName("landmark_t")]
        public string LandmarkType { get; set; } = string.Empty;




    }
}
