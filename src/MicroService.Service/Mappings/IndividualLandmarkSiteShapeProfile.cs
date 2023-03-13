using MicroService.Data.Enum;
using MicroService.Service.Helpers;
using MicroService.Service.Mappings.Base;
using MicroService.Service.Models;
using NetTopologySuite.Features;
using System;
using System.Text.RegularExpressions;

namespace MicroService.Service.Mappings
{
    public class IndividualLandmarkSiteShapeProfile : ShapeProfile<IndividualLandmarkSiteShape>
    {
        public IndividualLandmarkSiteShapeProfile()
        {
            CreateMap<Feature, IndividualLandmarkSiteShape>()
                .ForMember(dest => dest.LPNumber, opt => opt.MapFrom(src => src.Attributes["lpc_lpnumb"].ToString()))
                .ForMember(dest => dest.AreaName, opt => opt.MapFrom(src => src.Attributes["lpc_name"].ToString()))
                .ForMember(dest => dest.BoroCode, opt => opt.MapFrom(src => EnumHelper.IsEnumValid<Borough>(src.Attributes["borough"].ToString()) && src.Attributes["borough"] != null ?
                    (int)Enum.Parse(typeof(Borough), src.Attributes["borough"].ToString()) : 0))
                .ForMember(dest => dest.BoroName, opt => opt.MapFrom(src => EnumHelper.IsEnumValid<Borough>(src.Attributes["borough"].ToString()) && src.Attributes["borough"] != null ?
                    src.Attributes["borough"].ToString() : null))
                .ForMember(dest => dest.BBL, opt => opt.MapFrom(src => src.Attributes["bbl"] != null ? Double.Parse(src.Attributes["bbl"].ToString()) : 0))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Attributes["address"].ToString()))
                .ForMember(dest => dest.Block, opt => opt.MapFrom(src => src.Attributes["bbl"] != null ? Double.Parse(src.Attributes["block"].ToString()) : 0))
                .ForMember(dest => dest.Lot, opt => opt.MapFrom(src => src.Attributes["bbl"] != null ? Double.Parse(src.Attributes["lot"].ToString()) : 0))
                .ForMember(dest => dest.ObjectId, opt => opt.MapFrom(src => src.Attributes["objectid"] != null ? Double.Parse(src.Attributes["objectid"].ToString()) : 0))
                .ForMember(dest => dest.DesignationDate, opt => opt.MapFrom(src => src.Attributes["date_des_d"].ToString()))
                .ForMember(dest => dest.AlternativeName, opt =>
                    opt.MapFrom(src => string.IsNullOrEmpty(src.Attributes["lpc_altern"].ToString()) ? null
                        : Regex.Replace(src.Attributes["lpc_altern"].ToString(), @"\u0000", string.Empty)))
                .ForMember(dest => dest.SiteDesignation, opt => opt.MapFrom(src => src.Attributes["lpc_site_d"].ToString()))
                .ForMember(dest => dest.LandmarkType, opt => opt.MapFrom(src => src.Attributes["landmark_t"].ToString()))
                .ForMember(dest => dest.DesignationStatus, opt => opt.MapFrom(src => src.Attributes["lpc_site_s"].ToString()))
                .ForMember(dest => dest.UrlReport, opt => opt.MapFrom(src => src.Attributes["url_report"].ToString()))
                .ForMember(dest => dest.ShapeArea, opt => opt.MapFrom(src => double.Parse(src.Attributes["shape_area"].ToString())))
                .ForMember(dest => dest.ShapeLength, opt => opt.MapFrom(src => double.Parse(src.Attributes["shape_leng"].ToString())))
                .ForMember(dest => dest.Geometry, opt => opt.MapFrom(src => src.Geometry))
                .ForMember(dest => dest.Feature, opt => opt.Ignore());
        }
    }

}
