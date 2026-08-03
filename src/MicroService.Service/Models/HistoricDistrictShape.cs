using MicroService.Service.Models.Base;
using MicroService.Service.Models.Enum;
using MicroService.Service.Models.Enum.Attributes;

namespace MicroService.Service.Models
{
    [ShapeProperties(ShapeProperties.HistoricDistricts)]
    public class HistoricDistrictShape : ShapeBase, ILandmark
    {
        [FeatureName("lp_number")]
        public string LPNumber { get; set; } = string.Empty;

        [FeatureName("area_name")]
        public string AreaName { get; set; } = string.Empty;

        [FeatureName("borough")]
        public string BoroName { get; set; } = string.Empty;

        public int BoroCode { get; set; }

        [FeatureName("boundary_n")]
        public string BoundaryName { get; set; } = string.Empty;

        [FeatureName("caldate")]
        public string CalendarDate { get; set; } = string.Empty;

        [FeatureName("current_")]
        public string Current { get; set; } = string.Empty;

        [FeatureName("desdate")]
        public string DesignationDate { get; set; } = string.Empty;

        [FeatureName("extension")]
        public string Extension { get; set; } = string.Empty;

        [FeatureName("last_actio")]
        public string LastAction { get; set; } = string.Empty;

        [FeatureName("other_hear")]
        public string OtherHearing { get; set; } = string.Empty;

        [FeatureName("public_hea")]
        public string PublicHearing { get; set; } = string.Empty;

        [FeatureName("status_of_")]
        public string Status { get; set; } = string.Empty;
    }
}
