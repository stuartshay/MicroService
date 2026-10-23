using MicroService.Data.Enum;
using MicroService.Service.Helpers;
using MicroService.Service.Mappings.Base;
using MicroService.Service.Models;
using NetTopologySuite.Features;
using System;
using System.Globalization;

namespace MicroService.Service.Mappings
{
    public class NationalRegisterHistoricPlacesShapeProfile : ShapeProfile<NationalRegisterHistoricPlacesShape>
    {
        protected NationalRegisterHistoricPlacesShapeProfile()
        {
        }

        public static NationalRegisterHistoricPlacesShape Map(Feature src)
        {
            ArgumentNullException.ThrowIfNull(src);

            return new NationalRegisterHistoricPlacesShape
            {
                ObjectId = ParseDouble(src, "objectid"),
                Bbl = ParseDouble(src, "bbl"),
                BoroName = GetString(src, "borough"),
                BoroCode = GetBoroCode(src),
                LPNumber = GetString(src, "lpc_lpnumb"),
                AreaName = GetString(src, "lpc_name"),
                SiteDesignation = GetString(src, "lpc_site_d"),
                LandmarkType = GetString(src, "landmark_t"),
                DesignationStatus = GetString(src, "lpc_site_s"),
                AlternativeName = GetSanitizedString(src, "lpc_altern")!,
                Address = GetString(src, "address"),
                DateDesignated = DateTime.Parse(GetString(src, "date_des_d"), CultureInfo.InvariantCulture),
                TimeDesignated = GetString(src, "time_des_d"),
                Block = double.Parse(GetString(src, "block"), CultureInfo.InvariantCulture),
                Lot = double.Parse(GetString(src, "lot"), CultureInfo.InvariantCulture),
                UrlReport = GetString(src, "url_report"),
                ShapeArea = ParseDouble(src, "shape_area"),
                ShapeLength = ParseDouble(src, "shape_leng"),
                BoundingBox = MapBoundingBox(src.BoundingBox),
                Geometry = src.Geometry,
            };
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
