using MicroService.Service.Mappings.Base;
using MicroService.Service.Models;
using NetTopologySuite.Features;
using System;
using System.Globalization;

namespace MicroService.Service.Mappings
{
    public class IndividualLandmarkHistoricDistrictsShapeProfile : ShapeProfile<IndividualLandmarkHistoricDistrictsShape>
    {
        protected IndividualLandmarkHistoricDistrictsShapeProfile()
        {
        }

        public static IndividualLandmarkHistoricDistrictsShape Map(Feature src)
        {
            ArgumentNullException.ThrowIfNull(src);

            return new IndividualLandmarkHistoricDistrictsShape
            {
                Bin = GetDouble(src, "bin"),
                Bbl = GetDouble(src, "bbl").ToString(CultureInfo.InvariantCulture),
                BoroName = GetString(src, "borough"),
                Block = ParseDouble(src, "block"),
                Lot = ParseDouble(src, "lot"),
                Address = GetString(src, "des_addres"),
                Circa = ParseInt(src, "circa"),
                DateLow = ParseInt(src, "date_low"),
                DateHigh = ParseInt(src, "date_high"),
                DateCombo = GetString(src, "date_combo"),
                AltDate1 = GetString(src, "alt_date_1"),
                AltDate2 = GetString(src, "alt_date_2"),
                ArchBuild = GetString(src, "arch_build"),
                OwnDevel = GetString(src, "own_devel"),
                AltArch1 = GetString(src, "alt_arch_1"),
                AltArch2 = GetString(src, "alt_arch_2"),
                Altered = ParseInt(src, "altered"),
                StylePrim = GetString(src, "style_prim"),
                StyleSec = GetString(src, "style_sec"),
                StyleOth = GetString(src, "style_oth"),
                MatPrim = GetString(src, "mat_prim"),
                MatSec = GetString(src, "mat_sec"),
                MatThird = GetString(src, "mat_third"),
                MatFour = GetString(src, "mat_four"),
                MatOther = GetString(src, "mat_other"),
                UseOrig = GetString(src, "use_orig"),
                UseOther = GetString(src, "use_other"),
                BuildType = GetString(src, "build_type"),
                BuildOth = GetString(src, "build_oth"),
                BuildNme = GetString(src, "build_nme"),
                Notes = GetString(src, "notes"),
                HistDist = GetString(src, "hist_dist"),
                LmNew = GetString(src, "lm_new"),
                AreaName = GetString(src, "lm_orig"),
                ShapeArea = ParseDouble(src, "shape_area"),
                ShapeLength = ParseDouble(src, "shape_leng"),
                BoundingBox = MapBoundingBox(src.BoundingBox),
                Geometry = src.Geometry,
            };
        }
    }
}
