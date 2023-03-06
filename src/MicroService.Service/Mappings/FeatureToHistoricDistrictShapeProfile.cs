using AutoMapper;
using MicroService.Data.Enum;
using MicroService.Service.Models;
using NetTopologySuite.Features;
using System;

namespace MicroService.Service.Mappings
{
    public class FeatureToHistoricDistrictShapeProfile : Profile
    {
        public FeatureToHistoricDistrictShapeProfile()
        {
            CreateMap<Feature, HistoricDistrictShape>()
                .ForMember(dest => dest.LPNumber, opt => opt.MapFrom(src => src.Attributes["lp_number"].ToString()))
                .ForMember(dest => dest.AreaName, opt => opt.MapFrom(src => src.Attributes["area_name"].ToString()))
                .ForMember(dest => dest.BoroName, opt => opt.MapFrom(src => src.Attributes["borough"].ToString()))
                .ForMember(dest => dest.BoroCode, opt => opt.MapFrom(src => (int)Enum.Parse(typeof(Borough), src.Attributes["borough"].ToString())))
                .ForMember(dest => dest.ShapeArea, opt => opt.MapFrom(src => double.Parse(src.Attributes["shape_area"].ToString())))
                .ForMember(dest => dest.ShapeLength, opt => opt.MapFrom(src => double.Parse(src.Attributes["shape_leng"].ToString())))
                .ForMember(dest => dest.Feature, opt => opt.Ignore());
        }
    }

}
