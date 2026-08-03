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
                .ForMember(dest => dest.Bin, opt => opt.MapFrom(src => src.Attributes["bin"] != null ? Double.Parse(src.Attributes["bin"].ToString() ?? string.Empty) : 0))
                .ForMember(dest => dest.Bbl, opt => opt.MapFrom(src => src.Attributes["bbl"] != null ? Double.Parse(src.Attributes["bbl"].ToString() ?? string.Empty) : 0))
                .ForMember(dest => dest.BoroName, opt => opt.MapFrom(src => src.Attributes["borough"].ToString() ?? string.Empty))

                .ForMember(dest => dest.BoroCode, opt => opt.Ignore())
                //
                //.ForMember(dest => dest.BoroCode, opt => opt.MapFrom(src => EnumHelper.IsEnumValid<Borough>(src.Attributes["borough"].ToString() ?? string.Empty) && src.Attributes["borough"] != null ?
                //    (int)Enum.Parse(typeof(Borough), src.Attributes["borough"].ToString() ?? string.Empty) : 0))
                //
                //    src.Attributes["borough"].ToString() ?? string.Empty : null))
                .ForMember(dest => dest.Block, opt => opt.MapFrom(src => src.Attributes["block"].ToString() ?? string.Empty))
                .ForMember(dest => dest.Lot, opt => opt.MapFrom(src => src.Attributes["lot"].ToString() ?? string.Empty))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Attributes["des_addres"].ToString() ?? string.Empty))
                .ForMember(dest => dest.Circa, opt => opt.MapFrom(src => src.Attributes["circa"].ToString() ?? string.Empty))
                .ForMember(dest => dest.DateLow, opt => opt.MapFrom(src => src.Attributes["date_low"].ToString() ?? string.Empty))
                .ForMember(dest => dest.DateHigh, opt => opt.MapFrom(src => src.Attributes["date_high"].ToString() ?? string.Empty))
                .ForMember(dest => dest.DateCombo, opt => opt.MapFrom(src => src.Attributes["date_combo"].ToString() ?? string.Empty))
                .ForMember(dest => dest.AltDate1, opt => opt.MapFrom(src => src.Attributes["alt_date_1"].ToString() ?? string.Empty))
                .ForMember(dest => dest.AltDate2, opt => opt.MapFrom(src => src.Attributes["alt_date_2"].ToString() ?? string.Empty))
                .ForMember(dest => dest.ArchBuild, opt => opt.MapFrom(src => src.Attributes["arch_build"].ToString() ?? string.Empty))
                .ForMember(dest => dest.OwnDevel, opt => opt.MapFrom(src => src.Attributes["own_devel"].ToString() ?? string.Empty))
                .ForMember(dest => dest.AltArch1, opt => opt.MapFrom(src => src.Attributes["alt_arch_1"].ToString() ?? string.Empty))
                .ForMember(dest => dest.AltArch2, opt => opt.MapFrom(src => src.Attributes["alt_arch_2"].ToString() ?? string.Empty))
                .ForMember(dest => dest.Altered, opt => opt.MapFrom(src => src.Attributes["altered"].ToString() ?? string.Empty))
                .ForMember(dest => dest.StylePrim, opt => opt.MapFrom(src => src.Attributes["style_prim"].ToString() ?? string.Empty))
                .ForMember(dest => dest.StyleSec, opt => opt.MapFrom(src => src.Attributes["style_sec"].ToString() ?? string.Empty))
                .ForMember(dest => dest.StyleOth, opt => opt.MapFrom(src => src.Attributes["style_oth"].ToString() ?? string.Empty))
                .ForMember(dest => dest.MatPrim, opt => opt.MapFrom(src => src.Attributes["mat_prim"].ToString() ?? string.Empty))
                .ForMember(dest => dest.MatSec, opt => opt.MapFrom(src => src.Attributes["mat_sec"].ToString() ?? string.Empty))
                .ForMember(dest => dest.MatThird, opt => opt.MapFrom(src => src.Attributes["mat_third"].ToString() ?? string.Empty))
                .ForMember(dest => dest.MatFour, opt => opt.MapFrom(src => src.Attributes["mat_four"].ToString() ?? string.Empty))
                .ForMember(dest => dest.MatOther, opt => opt.MapFrom(src => src.Attributes["mat_other"].ToString() ?? string.Empty))
                .ForMember(dest => dest.UseOrig, opt => opt.MapFrom(src => src.Attributes["use_orig"].ToString() ?? string.Empty))
                .ForMember(dest => dest.UseOther, opt => opt.MapFrom(src => src.Attributes["use_other"].ToString() ?? string.Empty))
                .ForMember(dest => dest.BuildType, opt => opt.MapFrom(src => src.Attributes["build_type"].ToString() ?? string.Empty))
                .ForMember(dest => dest.BuildOth, opt => opt.MapFrom(src => src.Attributes["build_oth"].ToString() ?? string.Empty))
                .ForMember(dest => dest.BuildNme, opt => opt.MapFrom(src => src.Attributes["build_nme"].ToString() ?? string.Empty))
                .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Attributes["notes"].ToString() ?? string.Empty))
                .ForMember(dest => dest.HistDist, opt => opt.MapFrom(src => src.Attributes["hist_dist"].ToString() ?? string.Empty))
                .ForMember(dest => dest.LmNew, opt => opt.MapFrom(src => src.Attributes["lm_new"].ToString() ?? string.Empty))
                .ForMember(dest => dest.AreaName, opt => opt.MapFrom(src => src.Attributes["lm_orig"].ToString() ?? string.Empty))

                .ForMember(dest => dest.ShapeArea, opt => opt.MapFrom(src => double.Parse(src.Attributes["shape_area"].ToString() ?? string.Empty)))
                .ForMember(dest => dest.ShapeLength, opt => opt.MapFrom(src => double.Parse(src.Attributes["shape_leng"].ToString() ?? string.Empty)))
                .ForMember(dest => dest.LPNumber, opt => opt.Ignore())
                .ForMember(dest => dest.Geometry, opt => opt.MapFrom(src => src.Geometry))
                .ForMember(dest => dest.Feature, opt => opt.Ignore());
        }
    }
}
