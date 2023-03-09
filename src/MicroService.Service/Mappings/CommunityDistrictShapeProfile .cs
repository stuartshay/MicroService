using MicroService.Data.Enum;
using MicroService.Service.Helpers;
using MicroService.Service.Mappings.Base;
using MicroService.Service.Models;
using NetTopologySuite.Features;

namespace MicroService.Service.Mappings
{
    public class CommunityDistrictShapeProfile : ShapeProfile<CommunityDistrictShape>
    {
        public CommunityDistrictShapeProfile()
        {
            CreateMap<Feature, CommunityDistrictShape>()
                .ForMember(dest => dest.Cd, opt => opt.MapFrom(src => int.Parse(src.Attributes["BoroCD"].ToString().Substring(1, 2))))
                .ForMember(dest => dest.BoroCd, opt => opt.MapFrom(src => int.Parse(src.Attributes["BoroCD"].ToString())))
                .ForMember(dest => dest.BoroCode, opt => opt.MapFrom(src => int.Parse(src.Attributes["BoroCD"].ToString().Substring(0, 1))))
                .ForMember(dest => dest.Borough, opt => opt.MapFrom(src => src.Attributes["BoroCD"].ToString().Substring(0, 1).ParseEnum<Borough>().ToString()))
                .ForMember(dest => dest.BoroName, opt => opt.MapFrom(src => src.Attributes["BoroCD"].ToString().Substring(0, 1).ParseEnum<Borough>().GetEnumDescription()))
                .ForMember(dest => dest.ShapeArea, opt => opt.MapFrom(src => double.Parse(src.Attributes["Shape_Area"].ToString())))
                .ForMember(dest => dest.ShapeLength, opt => opt.MapFrom(src => double.Parse(src.Attributes["Shape_Leng"].ToString())))
                .ForMember(dest => dest.Geometry, opt => opt.MapFrom(src => src.Geometry))
                .ForMember(dest => dest.Feature, opt => opt.Ignore());
        }
    }

}
