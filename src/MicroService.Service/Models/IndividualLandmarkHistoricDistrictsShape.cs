using MicroService.Service.Models.Base;
using MicroService.Service.Models.Enum;
using MicroService.Service.Models.Enum.Attributes;

namespace MicroService.Service.Models
{
    [ShapeProperties(ShapeProperties.IndividualLandmarkHistoricDistricts)]
    public class IndividualLandmarkHistoricDistrictsShape : ShapeBase, ILandmark
    {
        [FeatureName("bin")]
        public double Bin { get; set; }

        [FeatureName("bbl")]
        public string Bbl { get; set; } = string.Empty;

        [FeatureName("borough")]
        public string BoroName { get; set; } = string.Empty;

        public int BoroCode { get; set; }

        [FeatureName("block")]
        public double Block { get; set; }

        [FeatureName("lot")]
        public double Lot { get; set; }

        [FeatureName("des_addres")]
        public string Address { get; set; } = string.Empty;

        [FeatureName("circa")]
        public int Circa { get; set; }

        [FeatureName("date_low")]
        public int DateLow { get; set; }

        [FeatureName("date_high")]
        public int DateHigh { get; set; }

        [FeatureName("date_combo")]
        public string DateCombo { get; set; } = string.Empty;

        [FeatureName("alt_date_1")]
        public string AltDate1 { get; set; } = string.Empty;

        [FeatureName("alt_date_2")]
        public string AltDate2 { get; set; } = string.Empty;

        [FeatureName("arch_build")]
        public string ArchBuild { get; set; } = string.Empty;

        [FeatureName("own_devel")]
        public string OwnDevel { get; set; } = string.Empty;

        [FeatureName("alt_arch_1")]
        public string AltArch1 { get; set; } = string.Empty;

        [FeatureName("alt_arch_2")]
        public string AltArch2 { get; set; } = string.Empty;

        [FeatureName("altered")]
        public int Altered { get; set; }

        [FeatureName("style_prim")]
        public string StylePrim { get; set; } = string.Empty;

        [FeatureName("style_sec")]
        public string StyleSec { get; set; } = string.Empty;

        [FeatureName("style_oth")]
        public string StyleOth { get; set; } = string.Empty;

        [FeatureName("mat_prim")]
        public string MatPrim { get; set; } = string.Empty;

        [FeatureName("mat_sec")]
        public string MatSec { get; set; } = string.Empty;

        [FeatureName("mat_third")]
        public string MatThird { get; set; } = string.Empty;

        [FeatureName("mat_four")]
        public string MatFour { get; set; } = string.Empty;

        [FeatureName("mat_other")]
        public string MatOther { get; set; } = string.Empty;

        [FeatureName("use_orig")]
        public string UseOrig { get; set; } = string.Empty;

        [FeatureName("use_other")]
        public string UseOther { get; set; } = string.Empty;

        [FeatureName("build_type")]
        public string BuildType { get; set; } = string.Empty;

        [FeatureName("build_oth")]
        public string BuildOth { get; set; } = string.Empty;

        [FeatureName("build_nme")]
        public string BuildNme { get; set; } = string.Empty;

        [FeatureName("notes")]
        public string Notes { get; set; } = string.Empty;

        [FeatureName("hist_dist")]
        public string HistDist { get; set; } = string.Empty;

        [FeatureName("lm_new")]
        public string LmNew { get; set; } = string.Empty;

        [FeatureName("lm_orig")]
        public string AreaName { get; set; } = string.Empty;

        [MappingKeyAttribute("LPNumber")]
        public string LPNumber { get; set; } = string.Empty;
    }
}
