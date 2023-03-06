using AutoMapper;
using MicroService.Service.Models;
using NetTopologySuite.Features;

namespace MicroService.Service.Mappings
{
    public class FeatureToParkShapeProfile : Profile
    {
        public FeatureToParkShapeProfile()
        {
            CreateMap<Feature, ParkShape>()
                .ForMember(dest => dest.ParkName, opt => opt.MapFrom(src => src.Attributes["PARK_NAME"].ToString()))
                .ForMember(dest => dest.ParkNumber, opt => opt.MapFrom(src => src.Attributes["PARKNUM"].ToString()))
                .ForMember(dest => dest.SourceId, opt => opt.MapFrom(src => long.Parse(src.Attributes["SOURCE_ID"].ToString())))
                .ForMember(dest => dest.FeatureCode, opt => opt.MapFrom(src => int.Parse(src.Attributes["FEAT_CODE"].ToString())))
                .ForMember(dest => dest.SubCode, opt => opt.MapFrom(src => int.Parse(src.Attributes["SUB_CODE"].ToString())))
                .ForMember(dest => dest.LandUse, opt => opt.MapFrom(src => src.Attributes["LANDUSE"].ToString()))
                .ForMember(dest => dest.System, opt => opt.MapFrom(src => src.Attributes["SYSTEM"].ToString()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Attributes["STATUS"].ToString()))
                .ForMember(dest => dest.ShapeArea, opt => opt.MapFrom(src => double.Parse(src.Attributes["SHAPE_Area"].ToString())))
                .ForMember(dest => dest.ShapeLength, opt => opt.MapFrom(src => double.Parse(src.Attributes["SHAPE_Leng"].ToString())))
                .ForMember(dest => dest.Feature, opt => opt.Ignore());
        }
    }
}
