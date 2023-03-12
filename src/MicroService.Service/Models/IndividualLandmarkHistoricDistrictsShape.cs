using MicroService.Service.Models.Base;
using MicroService.Service.Models.Enum.Attibutes;
using System.Text.Json.Serialization;

namespace MicroService.Service.Models
{
    public class IndividualLandmarkHistoricDistrictsShape : ShapeBase, ILandmark
    {
        [FeatureName("BIN")]
        public double Bin { get; set; }

        [FeatureName("BBL")]
        public string Bbl { get; set; }

        [FeatureName("Borough")]
        public string BoroName { get; set; }

        public int BoroCode { get; set; }

        [FeatureName("Block")]
        public double Block { get; set; }

        [FeatureName("Lot")]
        public double Lot { get; set; }

        [FeatureName("Des_Addres")]
        public string Address { get; set; }

        [FeatureName("Circa")]
        public int Circa { get; set; }

        [FeatureName("Date_Low")]
        public int DateLow { get; set; }

        [FeatureName("Date_High")]
        public int DateHigh { get; set; }

        [FeatureName("Date_Combo")]
        public string DateCombo { get; set; }

        [FeatureName("Alt_Date_1")]
        public string AltDate1 { get; set; }

        [FeatureName("Alt_Date_2")]
        public string AltDate2 { get; set; }

        [FeatureName("Arch_Build")]
        public string ArchBuild { get; set; }

        [FeatureName("Own_Devel")]
        public string OwnDevel { get; set; }

        [FeatureName("Alt_Arch_1")]
        public string AltArch1 { get; set; }

        [FeatureName("Alt_Arch_2")]
        public string AltArch2 { get; set; }

        [FeatureName("Altered")]
        public int Altered { get; set; }

        [FeatureName("Style_Prim")]
        public string StylePrim { get; set; }

        [FeatureName("Style_Sec")]
        public string StyleSec { get; set; }

        [FeatureName("Style_Oth")]
        public string StyleOth { get; set; }

        [FeatureName("Mat_Prim")]
        public string MatPrim { get; set; }

        [FeatureName("Mat_Sec")]
        public string MatSec { get; set; }

        [FeatureName("Mat_Third")]
        public string MatThird { get; set; }

        [FeatureName("Mat_Four")]
        public string MatFour { get; set; }

        [FeatureName("Mat_Other")]
        public string MatOther { get; set; }

        [FeatureName("Use_Orig")]
        public string UseOrig { get; set; }

        [FeatureName("Use_Other")]
        public string UseOther { get; set; }

        [FeatureName("Build_Type")]
        public string BuildType { get; set; }

        [FeatureName("Build_Oth")]
        public string BuildOth { get; set; }

        [FeatureName("Build_Nme")]
        public string BuildNme { get; set; }

        [FeatureName("Notes")]
        public string Notes { get; set; }

        [FeatureName("Hist_Dist")]
        public string HistDist { get; set; }

        [FeatureName("LM_New")]
        public string LmNew { get; set; }

        [FeatureName("LM_Orig")]
        public string AreaName { get; set; }

        //[FeatureName("SC_Flag")]
        //public int ScFlag { get; set; }

        //[FeatureName("BBL_Int")]
        //public int BblInt { get; set; }

        [JsonIgnore]
        public string LPNumber { get; set; }

    }
}
