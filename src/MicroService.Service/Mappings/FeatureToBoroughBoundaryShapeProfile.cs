using MicroService.Service.Mappings.Base;
using MicroService.Service.Models;
using NetTopologySuite.Features;

namespace MicroService.Service.Mappings
{
    public class FeatureToBoroughBoundaryShapeProfile : ShapeProfile<BoroughBoundaryShape>
    {
        public FeatureToBoroughBoundaryShapeProfile()
        {
            CreateMap<Feature, BoroughBoundaryShape>()
                .ForMember(dest => dest.BoroCode, opt => opt.MapFrom(src => int.Parse(src.Attributes["BoroCode"].ToString())))
                .ForMember(dest => dest.BoroName, opt => opt.MapFrom(src => src.Attributes["BoroName"].ToString()))
                .ForMember(dest => dest.ShapeArea, opt => opt.MapFrom(src => double.Parse(src.Attributes["Shape_Area"].ToString())))
                .ForMember(dest => dest.ShapeLength, opt => opt.MapFrom(src => double.Parse(src.Attributes["Shape_Leng"].ToString())))
                .ForMember(dest => dest.Geometry, opt => opt.MapFrom(src => src.Geometry))
                .ForMember(dest => dest.Feature, opt => opt.Ignore());

        }
    }
}
