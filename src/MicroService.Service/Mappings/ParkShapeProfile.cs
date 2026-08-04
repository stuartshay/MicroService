using MicroService.Service.Mappings.Base;
using MicroService.Service.Models;
using NetTopologySuite.Features;

namespace MicroService.Service.Mappings
{
    public class ParkShapeProfile : ShapeProfile<ParkShape>
    {
        public ParkShapeProfile()
        {
            CreateMap<Feature, ParkShape>()
                .ForMember(dest => dest.ParkName, opt => opt.MapFrom(src => src.Attributes["PARK_NAME"].ToString() ?? string.Empty))
                .ForMember(dest => dest.ParkNumber, opt => opt.MapFrom(src => src.Attributes["PARKNUM"].ToString() ?? string.Empty))
                .ForMember(dest => dest.SourceId, opt => opt.MapFrom(src => long.Parse(src.Attributes["SOURCE_ID"].ToString() ?? string.Empty)))
                .ForMember(dest => dest.FeatureCode, opt => opt.MapFrom(src => int.Parse(src.Attributes["FEAT_CODE"].ToString() ?? string.Empty)))
                .ForMember(dest => dest.SubCode, opt => opt.MapFrom(src => int.Parse(src.Attributes["SUB_CODE"].ToString() ?? string.Empty)))
                .ForMember(dest => dest.LandUse, opt => opt.MapFrom(src => src.Attributes["LANDUSE"].ToString() ?? string.Empty))
                .ForMember(dest => dest.System, opt => opt.MapFrom(src => src.Attributes["SYSTEM"].ToString() ?? string.Empty))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Attributes["STATUS"].ToString() ?? string.Empty))
                .ForMember(dest => dest.ShapeArea, opt => opt.MapFrom(src => double.Parse(src.Attributes["SHAPE_Area"].ToString() ?? string.Empty)))
                .ForMember(dest => dest.ShapeLength, opt => opt.MapFrom(src => double.Parse(src.Attributes["SHAPE_Leng"].ToString() ?? string.Empty)))
                .ForMember(dest => dest.Geometry, opt => opt.MapFrom(src => src.Geometry))
                .ForMember(dest => dest.Feature, opt => opt.Ignore());
        }
    }
}
