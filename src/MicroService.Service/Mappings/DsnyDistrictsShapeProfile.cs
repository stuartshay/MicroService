using MicroService.Data.Enum;
using MicroService.Service.Helpers;
using MicroService.Service.Mappings.Base;
using MicroService.Service.Models;
using NetTopologySuite.Features;

namespace MicroService.Service.Mappings
{
    public class DsnyDistrictsShapeProfile : ShapeProfile<DsnyDistrictsShape>
    {
        public DsnyDistrictsShapeProfile()
        {
            CreateMap<Feature, DsnyDistrictsShape>()
                .ForMember(dest => dest.District, opt => opt.MapFrom(src => src.Attributes["district"].ToString() ?? string.Empty))
                .ForMember(dest => dest.DistrictCode, opt => opt.MapFrom(src => int.Parse(src.Attributes["districtco"].ToString() ?? string.Empty)))
                .ForMember(dest => dest.Fid, opt => opt.MapFrom(src => src.Attributes["fid"].ToString() ?? string.Empty))
                .ForMember(dest => dest.GlobalId, opt => opt.MapFrom(src => src.Attributes["globalid"].ToString() ?? string.Empty))
                .ForMember(dest => dest.OperationZone, opt => opt.MapFrom(src => (src.Attributes["district"].ToString() ?? string.Empty).RemoveIntegers()))
                .ForMember(dest => dest.OperationZoneName, opt => opt.MapFrom(src => (src.Attributes["district"].ToString() ?? string.Empty).RemoveIntegers().ParseEnum<DsnyOperationZone>().GetEnumDescription()))
                .ForMember(dest => dest.ShapeArea, opt => opt.MapFrom(src => double.Parse(src.Attributes["shape_area"].ToString() ?? string.Empty)))
                .ForMember(dest => dest.ShapeLength, opt => opt.MapFrom(src => double.Parse(src.Attributes["shape_leng"].ToString() ?? string.Empty)))
                .ForMember(dest => dest.Geometry, opt => opt.MapFrom(src => src.Geometry))
                .ForMember(dest => dest.Feature, opt => opt.Ignore());
        }
    }
}
