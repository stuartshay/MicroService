using AutoMapper;
using MicroService.Service.Models;
using NetTopologySuite.Features;

namespace MicroService.Service.Mappings
{
    public class FeatureToNychaDevelopmentShapeProfile : Profile
    {
        public FeatureToNychaDevelopmentShapeProfile()
        {
            CreateMap<Feature, NychaDevelopmentShape>()
                .ForMember(dest => dest.Development, opt => opt.MapFrom(src => src.Attributes["DEVELOPMEN"].ToString()))
                .ForMember(dest => dest.TdsNumber, opt => opt.MapFrom(src => src.Attributes["TDS_NUM"] != null ? src.Attributes["TDS_NUM"].ToString() : null))
                .ForMember(dest => dest.Borough, opt => opt.MapFrom(src => src.Attributes["BOROUGH"] != null ? src.Attributes["BOROUGH"].ToString() : null))
                .ForMember(dest => dest.ShapeArea, opt => opt.Ignore())
                .ForMember(dest => dest.ShapeLength, opt => opt.Ignore())
                .ForMember(dest => dest.Geometry, opt => opt.MapFrom(src => src.Geometry))
                .ForMember(dest => dest.Feature, opt => opt.Ignore());
        }
    }

}
