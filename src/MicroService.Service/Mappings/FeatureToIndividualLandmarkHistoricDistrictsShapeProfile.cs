using MicroService.Service.Mappings.Base;
using MicroService.Service.Models;
using NetTopologySuite.Features;
using System;

namespace MicroService.Service.Mappings
{
    public class FeatureToIndividualLandmarkHistoricDistrictsShapeProfile : ShapeProfile<IndividualLandmarkHistoricDistrictsShape>
    {
        public FeatureToIndividualLandmarkHistoricDistrictsShapeProfile()
        {
            CreateMap<Feature, IndividualLandmarkHistoricDistrictsShape>()
                .ForMember(dest => dest.Bin, opt => opt.MapFrom(src => src.Attributes["BIN"] != null ? Double.Parse(src.Attributes["BIN"].ToString()) : 0))
                .ForMember(dest => dest.Bbl, opt => opt.MapFrom(src => src.Attributes["BBL"] != null ? Double.Parse(src.Attributes["BBL"].ToString()) : 0))
                .ForMember(dest => dest.BoroName, opt => opt.MapFrom(src => src.Attributes["Borough"].ToString()))

                .ForMember(dest => dest.BoroCode, opt => opt.Ignore())
                //
                //.ForMember(dest => dest.BoroCode, opt => opt.MapFrom(src => EnumHelper.IsEnumValid<Borough>(src.Attributes["borough"].ToString()) && src.Attributes["borough"] != null ?
                //    (int)Enum.Parse(typeof(Borough), src.Attributes["borough"].ToString()) : 0))
                //
                //    src.Attributes["borough"].ToString() : null))
                .ForMember(dest => dest.Block, opt => opt.MapFrom(src => src.Attributes["Block"].ToString()))
                .ForMember(dest => dest.Lot, opt => opt.MapFrom(src => src.Attributes["Lot"].ToString()))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Attributes["Des_Addres"].ToString()))
                .ForMember(dest => dest.Circa, opt => opt.MapFrom(src => src.Attributes["Circa"].ToString()))
                .ForMember(dest => dest.DateLow, opt => opt.MapFrom(src => src.Attributes["Date_Low"].ToString()))
                .ForMember(dest => dest.DateHigh, opt => opt.MapFrom(src => src.Attributes["Date_High"].ToString()))
                .ForMember(dest => dest.DateCombo, opt => opt.MapFrom(src => src.Attributes["Date_Combo"].ToString()))
                .ForMember(dest => dest.AltDate1, opt => opt.MapFrom(src => src.Attributes["Alt_Date_1"].ToString()))
                .ForMember(dest => dest.AltDate2, opt => opt.MapFrom(src => src.Attributes["Alt_Date_2"].ToString()))
                .ForMember(dest => dest.ArchBuild, opt => opt.MapFrom(src => src.Attributes["Arch_Build"].ToString()))
                .ForMember(dest => dest.OwnDevel, opt => opt.MapFrom(src => src.Attributes["Own_Devel"].ToString()))
                .ForMember(dest => dest.AltArch1, opt => opt.MapFrom(src => src.Attributes["Alt_Arch_1"].ToString()))
                .ForMember(dest => dest.AltArch2, opt => opt.MapFrom(src => src.Attributes["Alt_Arch_2"].ToString()))
                .ForMember(dest => dest.Altered, opt => opt.MapFrom(src => src.Attributes["Altered"].ToString()))
                .ForMember(dest => dest.StylePrim, opt => opt.MapFrom(src => src.Attributes["Style_Prim"].ToString()))
                .ForMember(dest => dest.StyleSec, opt => opt.MapFrom(src => src.Attributes["Style_Sec"].ToString()))
                .ForMember(dest => dest.StyleOth, opt => opt.MapFrom(src => src.Attributes["Style_Oth"].ToString()))
                .ForMember(dest => dest.MatPrim, opt => opt.MapFrom(src => src.Attributes["Mat_Prim"].ToString()))
                .ForMember(dest => dest.MatSec, opt => opt.MapFrom(src => src.Attributes["Mat_Sec"].ToString()))
                .ForMember(dest => dest.MatThird, opt => opt.MapFrom(src => src.Attributes["Mat_Third"].ToString()))
                .ForMember(dest => dest.MatFour, opt => opt.MapFrom(src => src.Attributes["Mat_Four"].ToString()))
                .ForMember(dest => dest.MatOther, opt => opt.MapFrom(src => src.Attributes["Mat_Other"].ToString()))
                .ForMember(dest => dest.UseOrig, opt => opt.MapFrom(src => src.Attributes["Use_Orig"].ToString()))
                .ForMember(dest => dest.UseOther, opt => opt.MapFrom(src => src.Attributes["Use_Other"].ToString()))
                .ForMember(dest => dest.BuildType, opt => opt.MapFrom(src => src.Attributes["Build_Type"].ToString()))
                .ForMember(dest => dest.BuildOth, opt => opt.MapFrom(src => src.Attributes["Build_Oth"].ToString()))
                .ForMember(dest => dest.BuildNme, opt => opt.MapFrom(src => src.Attributes["Build_Nme"].ToString()))
                .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Attributes["Notes"].ToString()))
                .ForMember(dest => dest.HistDist, opt => opt.MapFrom(src => src.Attributes["Hist_Dist"].ToString()))
                .ForMember(dest => dest.LmNew, opt => opt.MapFrom(src => src.Attributes["LM_New"].ToString()))
                .ForMember(dest => dest.AreaName, opt => opt.MapFrom(src => src.Attributes["LM_Orig"].ToString()))

                .ForMember(dest => dest.ShapeArea, opt => opt.MapFrom(src => double.Parse(src.Attributes["SHAPE_Area"].ToString())))
                .ForMember(dest => dest.ShapeLength, opt => opt.MapFrom(src => double.Parse(src.Attributes["SHAPE_Leng"].ToString())))
                .ForMember(dest => dest.LPNumber, opt => opt.Ignore())
                .ForMember(dest => dest.Geometry, opt => opt.MapFrom(src => src.Geometry))
                .ForMember(dest => dest.Feature, opt => opt.Ignore());
        }
    }
}
