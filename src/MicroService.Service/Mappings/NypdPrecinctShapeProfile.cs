using MicroService.Service.Mappings.Base;
using MicroService.Service.Models;
using NetTopologySuite.Features;

namespace MicroService.Service.Mappings
{
    public class NypdPrecinctShapeProfile : ShapeProfile<NypdPrecinctShape>
    {
        public NypdPrecinctShapeProfile()
        {
            CreateMap<Feature, NypdPrecinctShape>()
                .ForMember(dest => dest.Precinct, opt => opt.MapFrom(src => int.Parse(src.Attributes["Precinct"].ToString())))
                .ForMember(dest => dest.ShapeArea, opt => opt.MapFrom(src => double.Parse(src.Attributes["Shape_Area"].ToString())))
                .ForMember(dest => dest.ShapeLength, opt => opt.MapFrom(src => double.Parse(src.Attributes["Shape_Leng"].ToString())))
                .ForMember(dest => dest.Geometry, opt => opt.MapFrom(src => src.Geometry))
                .ForMember(dest => dest.Feature, opt => opt.Ignore());
        }
    }
}
