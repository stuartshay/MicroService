using MicroService.Data.Enum;
using MicroService.Service.Helpers;
using MicroService.Service.Mappings.Base;
using MicroService.Service.Models;
using NetTopologySuite.Features;

namespace MicroService.Service.Mappings
{
    public class CommunityDistrictShapeProfile : ShapeProfile<CommunityDistrictShape>
    {
        private const string BoroCdField = "BoroCD";

        public CommunityDistrictShapeProfile()
        {
            CreateMap<Feature, CommunityDistrictShape>()
                .ForMember(dest => dest.Cd, opt => opt.MapFrom(src => int.Parse((src.Attributes[BoroCdField].ToString() ?? string.Empty).Substring(1, 2))))
                .ForMember(dest => dest.BoroCd, opt => opt.MapFrom(src => int.Parse(src.Attributes[BoroCdField].ToString() ?? string.Empty)))
                .ForMember(dest => dest.BoroCode, opt => opt.MapFrom(src => int.Parse((src.Attributes[BoroCdField].ToString() ?? string.Empty).Substring(0, 1))))
                .ForMember(dest => dest.Borough, opt => opt.MapFrom(src => (src.Attributes[BoroCdField].ToString() ?? string.Empty).Substring(0, 1).ParseEnum<Borough>().ToString()))
                .ForMember(dest => dest.BoroName, opt => opt.MapFrom(src => (src.Attributes[BoroCdField].ToString() ?? string.Empty).Substring(0, 1).ParseEnum<Borough>().GetEnumDescription()))
                .ForMember(dest => dest.ShapeArea, opt => opt.MapFrom(src => double.Parse(src.Attributes["Shape_Area"].ToString() ?? string.Empty)))
                .ForMember(dest => dest.ShapeLength, opt => opt.MapFrom(src => double.Parse(src.Attributes["Shape_Leng"].ToString() ?? string.Empty)))
                .ForMember(dest => dest.Geometry, opt => opt.MapFrom(src => src.Geometry))
                .ForMember(dest => dest.Feature, opt => opt.Ignore());
        }
    }

}
