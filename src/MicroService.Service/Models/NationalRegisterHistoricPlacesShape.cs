using MicroService.Service.Models.Base;
using MicroService.Service.Models.Enum;
using MicroService.Service.Models.Enum.Attributes;
using System;

namespace MicroService.Service.Models
{
    [ShapeProperties(ShapeProperties.NationalRegisterHistoricPlaces)]
    public class NationalRegisterHistoricPlacesShape : ShapeBase, ILandmark
    {
        [FeatureName("objectid")]
        public double ObjectId { get; set; }

        [FeatureName("address")]
        public string Address { get; set; } = string.Empty;

        [FeatureName("borough")]
        public string BoroName { get; set; } = string.Empty;

        public int BoroCode { get; set; }

        [FeatureName("lpc_lpnumb")]
        public string LPNumber { get; set; } = string.Empty;

        [FeatureName("lpc_name")]
        public string AreaName { get; set; } = string.Empty;

        [FeatureName("lpc_altern")]
        public string AlternativeName { get; set; } = string.Empty;

        [FeatureName("lpc_site_d")]
        public string SiteDesignation { get; set; } = string.Empty;

        [FeatureName("landmark_t")]
        public string LandmarkType { get; set; } = string.Empty;

        [FeatureName("lpc_site_s")]
        public string DesignationStatus { get; set; } = string.Empty;

        [FeatureName("bbl")]
        public double Bbl { get; set; }

        [FeatureName("block")]
        public double Block { get; set; }

        [FeatureName("lot")]
        public double Lot { get; set; }

        [FeatureName("url_report")]
        public string UrlReport { get; set; } = string.Empty;

        [FeatureName("date_des_d")]
        public DateTime DateDesignated { get; set; }

        [FeatureName("time_des_d")]
        public string TimeDesignated { get; set; } = string.Empty;
    }
}
