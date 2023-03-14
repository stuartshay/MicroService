using MicroService.Service.Models.Base;
using MicroService.Service.Models.Enum;
using MicroService.Service.Models.Enum.Attributes;

namespace MicroService.Service.Models
{
    [ShapeProperties(ShapeProperties.HistoricDistricts)]
    public class HistoricDistrictShape : ShapeBase, ILandmark
    {
        [FeatureName("lp_number")]
        public string LPNumber { get; set; }

        [FeatureName("area_name")]
        public string AreaName { get; set; }

        [FeatureName("borough")]
        public string BoroName { get; set; }

        public int BoroCode { get; set; }

        [FeatureName("boundary_n")]
        public string BoundaryName { get; set; }

        [FeatureName("caldate")]
        public string CalendarDate { get; set; }

        [FeatureName("current")]
        public string Current { get; set; }

        [FeatureName("desdate")]
        public string DesignationDate { get; set; }

        [FeatureName("extension")]
        public string Extension { get; set; }

        [FeatureName("last_actio")]
        public string LastAction { get; set; }

        [FeatureName("other_hear")]
        public string OtherHearing { get; set; }

        [FeatureName("public_hea")]
        public string PublicHearing { get; set; }

        [FeatureName("status_of_")]
        public string Status { get; set; }
    }
}
