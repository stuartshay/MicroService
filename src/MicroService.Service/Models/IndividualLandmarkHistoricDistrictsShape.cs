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
        public string Bbl { get; set; }

        [FeatureName("borough")]
        public string BoroName { get; set; }

        public int BoroCode { get; set; }

        [FeatureName("block")]
        public double Block { get; set; }

        [FeatureName("lot")]
        public double Lot { get; set; }

        [FeatureName("des_addres")]
        public string Address { get; set; }

        [FeatureName("circa")]
        public int Circa { get; set; }

        [FeatureName("date_low")]
        public int DateLow { get; set; }

        [FeatureName("date_high")]
        public int DateHigh { get; set; }

        [FeatureName("date_combo")]
        public string DateCombo { get; set; }

        [FeatureName("alt_date_1")]
        public string AltDate1 { get; set; }

        [FeatureName("alt_date_2")]
        public string AltDate2 { get; set; }

        [FeatureName("arch_build")]
        public string ArchBuild { get; set; }

        [FeatureName("own_devel")]
        public string OwnDevel { get; set; }

        [FeatureName("alt_arch_1")]
        public string AltArch1 { get; set; }

        [FeatureName("alt_arch_2")]
        public string AltArch2 { get; set; }

        [FeatureName("altered")]
        public int Altered { get; set; }

        [FeatureName("style_prim")]
        public string StylePrim { get; set; }

        [FeatureName("style_sec")]
        public string StyleSec { get; set; }

        [FeatureName("style_oth")]
        public string StyleOth { get; set; }

        [FeatureName("mat_prim")]
        public string MatPrim { get; set; }

        [FeatureName("mat_sec")]
        public string MatSec { get; set; }

        [FeatureName("mat_third")]
        public string MatThird { get; set; }

        [FeatureName("mat_four")]
        public string MatFour { get; set; }

        [FeatureName("mat_other")]
        public string MatOther { get; set; }

        [FeatureName("use_orig")]
        public string UseOrig { get; set; }

        [FeatureName("use_other")]
        public string UseOther { get; set; }

        [FeatureName("build_type")]
        public string BuildType { get; set; }

        [FeatureName("build_oth")]
        public string BuildOth { get; set; }

        [FeatureName("build_nme")]
        public string BuildNme { get; set; }

        [FeatureName("notes")]
        public string Notes { get; set; }

        [FeatureName("hist_dist")]
        public string HistDist { get; set; }

        [FeatureName("lm_new")]
        public string LmNew { get; set; }

        [FeatureName("lm_orig")]
        public string AreaName { get; set; }

        //[FeatureName("SC_Flag")]
        //public int ScFlag { get; set; }

        //[FeatureName("BBL_Int")]
        //public int BblInt { get; set; }

        [MappingKeyAttribute("LPNumber")]
        public string LPNumber { get; set; }


    }
}
