using MicroService.Data.Enum;
using MicroService.Service.Helpers;
using MicroService.Service.Mappings.Base;
using MicroService.Service.Models;
using NetTopologySuite.Features;
using System;

namespace MicroService.Service.Mappings
{
    public class FeatureToNationalRegisterHistoricPlacesShapeProfile : ShapeProfile<NationalRegisterHistoricPlacesShape>
    {
        public FeatureToNationalRegisterHistoricPlacesShapeProfile()
        {
            CreateMap<Feature, NationalRegisterHistoricPlacesShape>()
                .ForMember(dest => dest.BBL, opt => opt.MapFrom(src => double.Parse(src.Attributes["bbl"].ToString())))
                .ForMember(dest => dest.BoroName, opt => opt.MapFrom(src => src.Attributes["borough"].ToString()))
                .ForMember(dest => dest.BoroCode, opt => opt.MapFrom(src =>
                    EnumHelper.IsEnumValid<Borough>(src.Attributes["borough"].ToString()) &&
                    src.Attributes["borough"] != null
                        ? (int)Enum.Parse(typeof(Borough), src.Attributes["borough"].ToString())
                        : 0))
                .ForMember(dest => dest.LPNumber, opt => opt.MapFrom(src => src.Attributes["lpc_lpnumb"].ToString()))
                .ForMember(dest => dest.AreaName, opt => opt.MapFrom(src => src.Attributes["lpc_name"].ToString()))
                .ForMember(dest => dest.ShapeArea,
                    opt => opt.MapFrom(src => double.Parse(src.Attributes["shape_area"].ToString())))
                .ForMember(dest => dest.ShapeLength,
                    opt => opt.MapFrom(src => double.Parse(src.Attributes["shape_leng"].ToString())))
                .ForMember(dest => dest.Geometry, opt => opt.MapFrom(src => src.Geometry))
                .ForMember(dest => dest.Feature, opt => opt.Ignore());
        }
    }

}
