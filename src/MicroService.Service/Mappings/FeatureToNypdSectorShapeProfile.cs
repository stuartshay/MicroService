using AutoMapper;
using MicroService.Service.Models;
using NetTopologySuite.Features;

namespace MicroService.Service.Mappings
{
    public class FeatureToNypdSectorShapeProfile : Profile
    {
        public FeatureToNypdSectorShapeProfile()
        {
            CreateMap<Feature, NypdSectorShape>()
                .ForMember(dest => dest.Pct, opt => opt.MapFrom(src => src.Attributes["pct"].ToString()))
                .ForMember(dest => dest.Sector, opt => opt.MapFrom(src => src.Attributes["sector"].ToString()))
                .ForMember(dest => dest.PatrolBoro, opt => opt.MapFrom(src => src.Attributes["patrol_bor"].ToString()))
                .ForMember(dest => dest.Phase, opt => opt.MapFrom(src => src.Attributes["phase"].ToString()))
                .ForMember(dest => dest.ShapeArea, opt => opt.Ignore())
                .ForMember(dest => dest.ShapeLength, opt => opt.Ignore())
                .ForMember(dest => dest.Geometry, opt => opt.MapFrom(src => src.Geometry))
                .ForMember(dest => dest.Feature, opt => opt.Ignore());
        }
    }

}
