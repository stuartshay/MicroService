using MicroService.Data.Enum;
using MicroService.Service.Helpers;
using MicroService.Service.Mappings.Base;
using MicroService.Service.Models;
using NetTopologySuite.Features;
using System;

namespace MicroService.Service.Mappings
{
    public class NationalRegisterHistoricPlacesShapeProfile : ShapeProfile<NationalRegisterHistoricPlacesShape>
    {
        public NationalRegisterHistoricPlacesShapeProfile()
        {
            CreateMap<Feature, NationalRegisterHistoricPlacesShape>()
                .ForMember(dest => dest.ObjectId, opt => opt.MapFrom(src => ParseDouble(src, "objectid")))
                .ForMember(dest => dest.Bbl, opt => opt.MapFrom(src => ParseDouble(src, "bbl")))
                .ForMember(dest => dest.BoroName, opt => opt.MapFrom(src => GetString(src, "borough")))
                .ForMember(dest => dest.BoroCode, opt => opt.MapFrom(src => GetBoroCode(src)))
                .ForMember(dest => dest.LPNumber, opt => opt.MapFrom(src => GetString(src, "lpc_lpnumb")))
                .ForMember(dest => dest.AreaName, opt => opt.MapFrom(src => GetString(src, "lpc_name")))
                .ForMember(dest => dest.SiteDesignation, opt => opt.MapFrom(src => GetString(src, "lpc_site_d")))
                .ForMember(dest => dest.LandmarkType, opt => opt.MapFrom(src => GetString(src, "landmark_t")))
                .ForMember(dest => dest.DesignationStatus, opt => opt.MapFrom(src => GetString(src, "lpc_site_s")))
                .ForMember(dest => dest.AlternativeName, opt => opt.MapFrom(src => GetSanitizedString(src, "lpc_altern")))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => GetString(src, "address")))
                .ForMember(dest => dest.DateDesignated, opt => opt.MapFrom(src => GetString(src, "date_des_d")))
                .ForMember(dest => dest.TimeDesignated, opt => opt.MapFrom(src => GetString(src, "time_des_d")))
                .ForMember(dest => dest.Block, opt => opt.MapFrom(src => GetString(src, "block")))
                .ForMember(dest => dest.Lot, opt => opt.MapFrom(src => GetString(src, "lot")))
                .ForMember(dest => dest.UrlReport, opt => opt.MapFrom(src => GetString(src, "url_report")))
                .ForMember(dest => dest.ShapeArea, opt => opt.MapFrom(src => ParseDouble(src, "shape_area")))
                .ForMember(dest => dest.ShapeLength, opt => opt.MapFrom(src => ParseDouble(src, "shape_leng")))
                .ForMember(dest => dest.Geometry, opt => opt.MapFrom(src => src.Geometry))
                .ForMember(dest => dest.Feature, opt => opt.Ignore());
        }

        private static int GetBoroCode(Feature src)
        {
            var borough = GetString(src, "borough");
            return src.Attributes["borough"] != null && EnumHelper.IsEnumValid<Borough>(borough)
                ? (int)Enum.Parse<Borough>(borough)
                : 0;
        }
    }

}
