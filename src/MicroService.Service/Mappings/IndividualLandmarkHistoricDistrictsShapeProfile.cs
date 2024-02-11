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
                .ForMember(dest => dest.Bin, opt => opt.MapFrom(src => src.Attributes["bin"] != null ? Double.Parse(src.Attributes["bin"].ToString()) : 0))
                .ForMember(dest => dest.Bbl, opt => opt.MapFrom(src => src.Attributes["bbl"] != null ? Double.Parse(src.Attributes["bbl"].ToString()) : 0))
                .ForMember(dest => dest.BoroName, opt => opt.MapFrom(src => src.Attributes["borough"].ToString()))

                .ForMember(dest => dest.BoroCode, opt => opt.Ignore())
                //
                //.ForMember(dest => dest.BoroCode, opt => opt.MapFrom(src => EnumHelper.IsEnumValid<Borough>(src.Attributes["borough"].ToString()) && src.Attributes["borough"] != null ?
                //    (int)Enum.Parse(typeof(Borough), src.Attributes["borough"].ToString()) : 0))
                //
                //    src.Attributes["borough"].ToString() : null))
                .ForMember(dest => dest.Block, opt => opt.MapFrom(src => src.Attributes["block"].ToString()))
                .ForMember(dest => dest.Lot, opt => opt.MapFrom(src => src.Attributes["lot"].ToString()))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Attributes["des_addres"].ToString()))
                .ForMember(dest => dest.Circa, opt => opt.MapFrom(src => src.Attributes["circa"].ToString()))
                .ForMember(dest => dest.DateLow, opt => opt.MapFrom(src => src.Attributes["date_low"].ToString()))
                .ForMember(dest => dest.DateHigh, opt => opt.MapFrom(src => src.Attributes["date_high"].ToString()))
                .ForMember(dest => dest.DateCombo, opt => opt.MapFrom(src => src.Attributes["date_combo"].ToString()))
                .ForMember(dest => dest.AltDate1, opt => opt.MapFrom(src => src.Attributes["alt_date_1"].ToString()))
                .ForMember(dest => dest.AltDate2, opt => opt.MapFrom(src => src.Attributes["alt_date_2"].ToString()))
                .ForMember(dest => dest.ArchBuild, opt => opt.MapFrom(src => src.Attributes["arch_build"].ToString()))
                .ForMember(dest => dest.OwnDevel, opt => opt.MapFrom(src => src.Attributes["own_devel"].ToString()))
                .ForMember(dest => dest.AltArch1, opt => opt.MapFrom(src => src.Attributes["alt_arch_1"].ToString()))
                .ForMember(dest => dest.AltArch2, opt => opt.MapFrom(src => src.Attributes["alt_arch_2"].ToString()))
                .ForMember(dest => dest.Altered, opt => opt.MapFrom(src => src.Attributes["altered"].ToString()))
                .ForMember(dest => dest.StylePrim, opt => opt.MapFrom(src => src.Attributes["style_prim"].ToString()))
                .ForMember(dest => dest.StyleSec, opt => opt.MapFrom(src => src.Attributes["style_sec"].ToString()))
                .ForMember(dest => dest.StyleOth, opt => opt.MapFrom(src => src.Attributes["style_oth"].ToString()))
                .ForMember(dest => dest.MatPrim, opt => opt.MapFrom(src => src.Attributes["mat_prim"].ToString()))
                .ForMember(dest => dest.MatSec, opt => opt.MapFrom(src => src.Attributes["mat_sec"].ToString()))
                .ForMember(dest => dest.MatThird, opt => opt.MapFrom(src => src.Attributes["mat_third"].ToString()))
                .ForMember(dest => dest.MatFour, opt => opt.MapFrom(src => src.Attributes["mat_four"].ToString()))
                .ForMember(dest => dest.MatOther, opt => opt.MapFrom(src => src.Attributes["mat_other"].ToString()))
                .ForMember(dest => dest.UseOrig, opt => opt.MapFrom(src => src.Attributes["use_orig"].ToString()))
                .ForMember(dest => dest.UseOther, opt => opt.MapFrom(src => src.Attributes["use_other"].ToString()))
                .ForMember(dest => dest.BuildType, opt => opt.MapFrom(src => src.Attributes["build_type"].ToString()))
                .ForMember(dest => dest.BuildOth, opt => opt.MapFrom(src => src.Attributes["build_oth"].ToString()))
                .ForMember(dest => dest.BuildNme, opt => opt.MapFrom(src => src.Attributes["build_nme"].ToString()))
                .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Attributes["notes"].ToString()))
                .ForMember(dest => dest.HistDist, opt => opt.MapFrom(src => src.Attributes["hist_dist"].ToString()))
                .ForMember(dest => dest.LmNew, opt => opt.MapFrom(src => src.Attributes["lm_new"].ToString()))
                .ForMember(dest => dest.AreaName, opt => opt.MapFrom(src => src.Attributes["lm_orig"].ToString()))

                .ForMember(dest => dest.ShapeArea, opt => opt.MapFrom(src => double.Parse(src.Attributes["shape_area"].ToString())))
                .ForMember(dest => dest.ShapeLength, opt => opt.MapFrom(src => double.Parse(src.Attributes["shape_leng"].ToString())))
                .ForMember(dest => dest.LPNumber, opt => opt.Ignore())
                .ForMember(dest => dest.Geometry, opt => opt.MapFrom(src => src.Geometry))
                .ForMember(dest => dest.Feature, opt => opt.Ignore());
        }
    }
}
