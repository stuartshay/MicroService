using MicroService.Service.Mappings.Base;
using MicroService.Service.Models;
using NetTopologySuite.Features;

namespace MicroService.Service.Mappings
{
    public class IndividualLandmarkHistoricDistrictsShapeProfile : ShapeProfile<IndividualLandmarkHistoricDistrictsShape>
    {
        public IndividualLandmarkHistoricDistrictsShapeProfile()
        {
            CreateMap<Feature, IndividualLandmarkHistoricDistrictsShape>()
                .ForMember(dest => dest.Bin, opt => opt.MapFrom(src => GetDouble(src, "bin")))
                .ForMember(dest => dest.Bbl, opt => opt.MapFrom(src => GetDouble(src, "bbl")))
                .ForMember(dest => dest.BoroName, opt => opt.MapFrom(src => GetString(src, "borough")))

                .ForMember(dest => dest.BoroCode, opt => opt.Ignore())
                //
                //.ForMember(dest => dest.BoroCode, opt => opt.MapFrom(src => EnumHelper.IsEnumValid<Borough>(src.Attributes["borough"].ToString() ?? string.Empty) && src.Attributes["borough"] != null ?
                //    (int)Enum.Parse(typeof(Borough), src.Attributes["borough"].ToString() ?? string.Empty) : 0))
                //
                //    src.Attributes["borough"].ToString() ?? string.Empty : null))
                .ForMember(dest => dest.Block, opt => opt.MapFrom(src => GetString(src, "block")))
                .ForMember(dest => dest.Lot, opt => opt.MapFrom(src => GetString(src, "lot")))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => GetString(src, "des_addres")))
                .ForMember(dest => dest.Circa, opt => opt.MapFrom(src => GetString(src, "circa")))
                .ForMember(dest => dest.DateLow, opt => opt.MapFrom(src => GetString(src, "date_low")))
                .ForMember(dest => dest.DateHigh, opt => opt.MapFrom(src => GetString(src, "date_high")))
                .ForMember(dest => dest.DateCombo, opt => opt.MapFrom(src => GetString(src, "date_combo")))
                .ForMember(dest => dest.AltDate1, opt => opt.MapFrom(src => GetString(src, "alt_date_1")))
                .ForMember(dest => dest.AltDate2, opt => opt.MapFrom(src => GetString(src, "alt_date_2")))
                .ForMember(dest => dest.ArchBuild, opt => opt.MapFrom(src => GetString(src, "arch_build")))
                .ForMember(dest => dest.OwnDevel, opt => opt.MapFrom(src => GetString(src, "own_devel")))
                .ForMember(dest => dest.AltArch1, opt => opt.MapFrom(src => GetString(src, "alt_arch_1")))
                .ForMember(dest => dest.AltArch2, opt => opt.MapFrom(src => GetString(src, "alt_arch_2")))
                .ForMember(dest => dest.Altered, opt => opt.MapFrom(src => GetString(src, "altered")))
                .ForMember(dest => dest.StylePrim, opt => opt.MapFrom(src => GetString(src, "style_prim")))
                .ForMember(dest => dest.StyleSec, opt => opt.MapFrom(src => GetString(src, "style_sec")))
                .ForMember(dest => dest.StyleOth, opt => opt.MapFrom(src => GetString(src, "style_oth")))
                .ForMember(dest => dest.MatPrim, opt => opt.MapFrom(src => GetString(src, "mat_prim")))
                .ForMember(dest => dest.MatSec, opt => opt.MapFrom(src => GetString(src, "mat_sec")))
                .ForMember(dest => dest.MatThird, opt => opt.MapFrom(src => GetString(src, "mat_third")))
                .ForMember(dest => dest.MatFour, opt => opt.MapFrom(src => GetString(src, "mat_four")))
                .ForMember(dest => dest.MatOther, opt => opt.MapFrom(src => GetString(src, "mat_other")))
                .ForMember(dest => dest.UseOrig, opt => opt.MapFrom(src => GetString(src, "use_orig")))
                .ForMember(dest => dest.UseOther, opt => opt.MapFrom(src => GetString(src, "use_other")))
                .ForMember(dest => dest.BuildType, opt => opt.MapFrom(src => GetString(src, "build_type")))
                .ForMember(dest => dest.BuildOth, opt => opt.MapFrom(src => GetString(src, "build_oth")))
                .ForMember(dest => dest.BuildNme, opt => opt.MapFrom(src => GetString(src, "build_nme")))
                .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => GetString(src, "notes")))
                .ForMember(dest => dest.HistDist, opt => opt.MapFrom(src => GetString(src, "hist_dist")))
                .ForMember(dest => dest.LmNew, opt => opt.MapFrom(src => GetString(src, "lm_new")))
                .ForMember(dest => dest.AreaName, opt => opt.MapFrom(src => GetString(src, "lm_orig")))

                .ForMember(dest => dest.ShapeArea, opt => opt.MapFrom(src => ParseDouble(src, "shape_area")))
                .ForMember(dest => dest.ShapeLength, opt => opt.MapFrom(src => ParseDouble(src, "shape_leng")))
                .ForMember(dest => dest.LPNumber, opt => opt.Ignore())
                .ForMember(dest => dest.Geometry, opt => opt.MapFrom(src => src.Geometry))
                .ForMember(dest => dest.Feature, opt => opt.Ignore());
        }
    }
}
