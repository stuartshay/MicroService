using MicroService.Data.Enum;
using MicroService.Service.Helpers;
using MicroService.Service.Mappings.Base;
using MicroService.Service.Models;
using NetTopologySuite.Features;
using System;
using System.Text.RegularExpressions;

namespace MicroService.Service.Mappings
{
    public partial class IndividualLandmarkSiteShapeProfile : ShapeProfile<IndividualLandmarkSiteShape>
    {
        private const string BoroughField = "borough";

        public IndividualLandmarkSiteShapeProfile()
        {
            CreateMap<Feature, IndividualLandmarkSiteShape>()
                .ForMember(dest => dest.LPNumber, opt => opt.MapFrom(src => GetString(src, "lpc_lpnumb")))
                .ForMember(dest => dest.AreaName, opt => opt.MapFrom(src => GetString(src, "lpc_name")))
                .ForMember(dest => dest.BoroCode, opt => opt.MapFrom(src => GetBoroCode(src)))
                .ForMember(dest => dest.BoroName, opt => opt.MapFrom(src => GetValidBoroughName(src)))
                .ForMember(dest => dest.BBL, opt => opt.MapFrom(src => GetDouble(src, "bbl")))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => GetString(src, "address")))
                .ForMember(dest => dest.Block, opt => opt.MapFrom(src => GetDouble(src, "block")))
                .ForMember(dest => dest.Lot, opt => opt.MapFrom(src => GetDouble(src, "lot")))
                .ForMember(dest => dest.ObjectId, opt => opt.MapFrom(src => GetDouble(src, "objectid")))
                .ForMember(dest => dest.DesignationDate, opt => opt.MapFrom(src => GetString(src, "date_des_d")))
                .ForMember(dest => dest.AlternativeName, opt => opt.MapFrom(src => GetAlternativeName(src)))
                .ForMember(dest => dest.SiteDesignation, opt => opt.MapFrom(src => GetString(src, "lpc_site_d")))
                .ForMember(dest => dest.LandmarkType, opt => opt.MapFrom(src => GetString(src, "landmark_t")))
                .ForMember(dest => dest.DesignationStatus, opt => opt.MapFrom(src => GetString(src, "lpc_site_s")))
                .ForMember(dest => dest.UrlReport, opt => opt.MapFrom(src => GetString(src, "url_report")))
                .ForMember(dest => dest.ShapeArea, opt => opt.MapFrom(src => ParseDouble(src, "shape_area")))
                .ForMember(dest => dest.ShapeLength, opt => opt.MapFrom(src => ParseDouble(src, "shape_leng")))
                .ForMember(dest => dest.Geometry, opt => opt.MapFrom(src => src.Geometry))
                .ForMember(dest => dest.Feature, opt => opt.Ignore());
        }

        private static int GetBoroCode(Feature src)
        {
            var borough = GetString(src, BoroughField);
            return src.Attributes[BoroughField] != null && EnumHelper.IsEnumValid<Borough>(borough)
                ? (int)Enum.Parse<Borough>(borough)
                : 0;
        }

        private static string? GetValidBoroughName(Feature src)
        {
            var borough = GetString(src, BoroughField);
            return src.Attributes[BoroughField] != null && EnumHelper.IsEnumValid<Borough>(borough)
                ? borough
                : null;
        }

        private static string? GetAlternativeName(Feature src)
        {
            var alternativeName = GetString(src, "lpc_altern");
            return string.IsNullOrEmpty(alternativeName)
                ? null
                : NullCharacterRegex().Replace(alternativeName, string.Empty);
        }

        [GeneratedRegex(@"\u0000")]
        private static partial Regex NullCharacterRegex();
    }

}
