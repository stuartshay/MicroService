using MicroService.Data.Enum;
using MicroService.Service.Mappings.Base;
using MicroService.Service.Models;
using NetTopologySuite.Features;
using System;

namespace MicroService.Service.Mappings
{
    public class HistoricDistrictShapeProfile : ShapeProfile<HistoricDistrictShape>
    {
        public HistoricDistrictShapeProfile()
        {
            CreateMap<Feature, HistoricDistrictShape>()
                .ForMember(dest => dest.LPNumber, opt => opt.MapFrom(src => src.Attributes["lp_number"].ToString()))
                .ForMember(dest => dest.AreaName, opt => opt.MapFrom(src => src.Attributes["area_name"].ToString()))
                .ForMember(dest => dest.BoroName, opt => opt.MapFrom(src => src.Attributes["borough"].ToString()))
                .ForMember(dest => dest.BoroCode, opt => opt.MapFrom(src => (int)Enum.Parse(typeof(Borough), src.Attributes["borough"].ToString())))
                .ForMember(dest => dest.ShapeArea, opt => opt.MapFrom(src => double.Parse(src.Attributes["shape_area"].ToString())))
                .ForMember(dest => dest.ShapeLength, opt => opt.MapFrom(src => double.Parse(src.Attributes["shape_leng"].ToString())))
                .ForMember(dest => dest.Geometry, opt => opt.MapFrom(src => src.Geometry))
                .ForMember(dest => dest.Feature, opt => opt.Ignore());
        }
    }

}
