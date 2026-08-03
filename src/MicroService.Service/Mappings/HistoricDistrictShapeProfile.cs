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
                .ForMember(dest => dest.LPNumber, opt => opt.MapFrom(src => GetString(src, "lp_number")))
                .ForMember(dest => dest.AreaName, opt => opt.MapFrom(src => GetString(src, "area_name")))
                .ForMember(dest => dest.BoroName, opt => opt.MapFrom(src => GetString(src, "borough")))
                .ForMember(dest => dest.BoundaryName, opt => opt.MapFrom(src => GetSanitizedString(src, "boundary_n")))
                .ForMember(dest => dest.CalendarDate, opt => opt.MapFrom(src => GetSanitizedString(src, "caldate")))
                .ForMember(dest => dest.Current, opt => opt.MapFrom(src => GetString(src, "current_")))
                .ForMember(dest => dest.DesignationDate, opt => opt.MapFrom(src => GetSanitizedString(src, "desdate")))
                .ForMember(dest => dest.Extension, opt => opt.MapFrom(src => GetSanitizedString(src, "extension")))
                .ForMember(dest => dest.LastAction, opt => opt.MapFrom(src => GetString(src, "last_actio")))
                .ForMember(dest => dest.OtherHearing, opt => opt.MapFrom(src => GetSanitizedString(src, "other_hear")))
                .ForMember(dest => dest.PublicHearing, opt => opt.MapFrom(src => GetString(src, "public_hea")))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => GetString(src, "status_of_")))
                .ForMember(dest => dest.BoroCode, opt => opt.MapFrom(src => (int)Enum.Parse<Borough>(GetString(src, "borough"))))
                .ForMember(dest => dest.ShapeArea, opt => opt.MapFrom(src => ParseDouble(src, "shape_area")))
                .ForMember(dest => dest.ShapeLength, opt => opt.MapFrom(src => ParseDouble(src, "shape_leng")))
                .ForMember(dest => dest.Geometry, opt => opt.MapFrom(src => src.Geometry))
                .ForMember(dest => dest.Feature, opt => opt.Ignore());
        }
    }

}
