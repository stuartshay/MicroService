using MicroService.Service.Mappings.Base;
using MicroService.Service.Models;
using NetTopologySuite.Features;

namespace MicroService.Service.Mappings
{
    public class NeighborhoodTabulationAreaShapeProfile : ShapeProfile<NeighborhoodTabulationAreaShape>
    {
        public NeighborhoodTabulationAreaShapeProfile()
        {
            CreateMap<Feature, NeighborhoodTabulationAreaShape>()
                .ForMember(dest => dest.BoroCode, opt => opt.MapFrom(src => int.Parse(src.Attributes["BoroCode"].ToString() ?? string.Empty)))
                .ForMember(dest => dest.BoroName, opt => opt.MapFrom(src => src.Attributes["BoroName"].ToString() ?? string.Empty))
                .ForMember(dest => dest.CountyFIPS, opt => opt.MapFrom(src => src.Attributes["CountyFIPS"].ToString() ?? string.Empty))
                .ForMember(dest => dest.NTA2020, opt => opt.MapFrom(src => src.Attributes["NTA2020"].ToString() ?? string.Empty))
                .ForMember(dest => dest.NTAName, opt => opt.MapFrom(src => src.Attributes["BoroCode"].ToString() ?? string.Empty))
                .ForMember(dest => dest.NTAAbbrev, opt => opt.MapFrom(src => src.Attributes["NTAName"].ToString() ?? string.Empty))
                .ForMember(dest => dest.NTAType, opt => opt.MapFrom(src => int.Parse(src.Attributes["NTAType"].ToString() ?? string.Empty)))
                .ForMember(dest => dest.CDTA2020, opt => opt.MapFrom(src => src.Attributes["CDTA2020"].ToString() ?? string.Empty))
                .ForMember(dest => dest.CDTAName, opt => opt.MapFrom(src => src.Attributes["CDTAName"].ToString() ?? string.Empty))
                .ForMember(dest => dest.ShapeArea, opt => opt.MapFrom(src => double.Parse(src.Attributes["Shape_Area"].ToString() ?? string.Empty)))
                .ForMember(dest => dest.ShapeLength, opt => opt.MapFrom(src => double.Parse(src.Attributes["Shape_Leng"].ToString() ?? string.Empty)))
                .ForMember(dest => dest.Geometry, opt => opt.MapFrom(src => src.Geometry))
                .ForMember(dest => dest.Feature, opt => opt.Ignore());
        }
    }

}
