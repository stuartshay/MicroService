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

        protected IndividualLandmarkSiteShapeProfile()
        {
        }

        public static IndividualLandmarkSiteShape Map(Feature src)
        {
            ArgumentNullException.ThrowIfNull(src);

            return new IndividualLandmarkSiteShape
            {
                LPNumber = GetString(src, "lpc_lpnumb"),
                AreaName = GetString(src, "lpc_name"),
                BoroCode = GetBoroCode(src),
                BoroName = GetValidBoroughName(src)!,
                BBL = GetDouble(src, "bbl"),
                Address = GetString(src, "address"),
                Block = GetDouble(src, "block"),
                Lot = GetDouble(src, "lot"),
                ObjectId = GetDouble(src, "objectid"),
                DesignationDate = GetString(src, "date_des_d"),
                AlternativeName = GetAlternativeName(src)!,
                SiteDesignation = GetString(src, "lpc_site_d"),
                LandmarkType = GetString(src, "landmark_t"),
                DesignationStatus = GetString(src, "lpc_site_s"),
                UrlReport = GetString(src, "url_report"),
                ShapeArea = ParseDouble(src, "shape_area"),
                ShapeLength = ParseDouble(src, "shape_leng"),
                BoundingBox = MapBoundingBox(src.BoundingBox),
                Geometry = src.Geometry,
            };
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
