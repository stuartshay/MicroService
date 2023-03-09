using MicroService.Service.Mappings.Base;
using MicroService.Service.Models;
using NetTopologySuite.Features;

namespace MicroService.Service.Mappings
{
    public class NeighborhoodShapeMappingsProfile : ShapeProfile<NeighborhoodShape>
    {
        public NeighborhoodShapeMappingsProfile()
        {
            CreateMap<Feature, NeighborhoodShape>()
                .ForMember(dest => dest.BoroCode, opt => opt.MapFrom(src => int.Parse(src.Attributes["BoroCode"].ToString())))
                .ForMember(dest => dest.BoroName, opt => opt.MapFrom(src => src.Attributes["BoroName"].ToString()))
                .ForMember(dest => dest.CountyFIPS, opt => opt.MapFrom(src => src.Attributes["CountyFIPS"].ToString()))
                .ForMember(dest => dest.NTACode, opt => opt.MapFrom(src => src.Attributes["NTACode"].ToString()))
                .ForMember(dest => dest.NTAName, opt => opt.MapFrom(src => src.Attributes["NTAName"].ToString()))
                .ForMember(dest => dest.ShapeArea, opt => opt.MapFrom(src => double.Parse(src.Attributes["Shape_Area"].ToString())))
                .ForMember(dest => dest.ShapeLength, opt => opt.MapFrom(src => double.Parse(src.Attributes["Shape_Leng"].ToString())))
                .ForMember(dest => dest.Geometry, opt => opt.MapFrom(src => src.Geometry))
                .ForMember(dest => dest.Feature, opt => opt.Ignore());
        }
    }
}
