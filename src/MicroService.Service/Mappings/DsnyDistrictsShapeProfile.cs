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
                .ForMember(dest => dest.District, opt => opt.MapFrom(src => src.Attributes["district"].ToString()))
                .ForMember(dest => dest.DistrictCode, opt => opt.MapFrom(src => int.Parse(src.Attributes["districtco"].ToString())))
                .ForMember(dest => dest.Fid, opt => opt.MapFrom(src => src.Attributes["fid"].ToString()))
                .ForMember(dest => dest.GlobalId, opt => opt.MapFrom(src => src.Attributes["globalid"].ToString()))
                .ForMember(dest => dest.OperationZone, opt => opt.MapFrom(src => src.Attributes["district"].ToString().RemoveIntegers()))
                .ForMember(dest => dest.OperationZoneName, opt => opt.MapFrom(src => src.Attributes["district"].ToString().RemoveIntegers().ParseEnum<DsnyOperationZone>().GetEnumDescription()))
                .ForMember(dest => dest.ShapeArea, opt => opt.MapFrom(src => double.Parse(src.Attributes["shape_area"].ToString())))
                .ForMember(dest => dest.ShapeLength, opt => opt.MapFrom(src => double.Parse(src.Attributes["shape_leng"].ToString())))
                .ForMember(dest => dest.Geometry, opt => opt.MapFrom(src => src.Geometry))
                .ForMember(dest => dest.Feature, opt => opt.Ignore());
        }
    }
}
