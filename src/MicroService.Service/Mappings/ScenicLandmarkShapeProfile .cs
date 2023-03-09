using MicroService.Data.Enum;
using MicroService.Service.Mappings.Base;
using MicroService.Service.Models;
using NetTopologySuite.Features;
using System;

namespace MicroService.Service.Mappings
{
    public class FeatureToScenicLandmarkShapeProfile : ShapeProfile<ScenicLandmarkShape>
    {
        public FeatureToScenicLandmarkShapeProfile()
        {
            CreateMap<Feature, ScenicLandmarkShape>()
                .ForMember(dest => dest.LPNumber, opt => opt.MapFrom(src => src.Attributes["lp_number"].ToString()))
                .ForMember(dest => dest.AreaName, opt => opt.MapFrom(src => src.Attributes["scen_lm_na"].ToString()))
                .ForMember(dest => dest.BoroName, opt => opt.MapFrom(src => src.Attributes["borough"].ToString()))
                .ForMember(dest => dest.BoroCode, opt => opt.MapFrom(src => (int)Enum.Parse(typeof(Borough), src.Attributes["borough"].ToString())))
                .ForMember(dest => dest.ShapeArea, opt => opt.MapFrom(src => double.Parse(src.Attributes["shape_area"].ToString())))
                .ForMember(dest => dest.ShapeLength, opt => opt.MapFrom(src => double.Parse(src.Attributes["shape_leng"].ToString())))
                .ForMember(dest => dest.Geometry, opt => opt.MapFrom(src => src.Geometry))
                .ForMember(dest => dest.Feature, opt => opt.Ignore());
        }
    }

}
