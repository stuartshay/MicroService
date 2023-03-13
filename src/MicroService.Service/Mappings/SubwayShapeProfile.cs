using MicroService.Service.Mappings.Base;
using MicroService.Service.Models;
using NetTopologySuite.Features;

namespace MicroService.Service.Mappings
{
    public class SubwayShapeProfile : ShapeProfile<SubwayShape>
    {
        public SubwayShapeProfile()
        {
            CreateMap<Feature, SubwayShape>()
                .ForMember(dest => dest.Line, opt => opt.MapFrom(src => src.Attributes["line"].ToString()))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Attributes["name"].ToString()))
                .ForMember(dest => dest.ObjectId, opt => opt.MapFrom(src => int.Parse(src.Attributes["objectid"].ToString())))
                .ForMember(dest => dest.Url, opt => opt.Ignore())
                .ForMember(dest => dest.Distance, opt => opt.Ignore())
                .ForMember(dest => dest.ShapeArea, opt => opt.Ignore())
                .ForMember(dest => dest.ShapeLength, opt => opt.Ignore())
                .ForMember(dest => dest.Geometry, opt => opt.MapFrom(src => src.Geometry))
                .ForMember(dest => dest.Feature, opt => opt.Ignore());
        }
    }

}
