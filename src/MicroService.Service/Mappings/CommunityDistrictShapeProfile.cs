using MicroService.Data.Enum;
using MicroService.Service.Helpers;
using MicroService.Service.Mappings.Base;
using MicroService.Service.Models;
using NetTopologySuite.Features;
using System;

namespace MicroService.Service.Mappings
{
    public class CommunityDistrictShapeProfile : ShapeProfile<CommunityDistrictShape>
    {
        private const string BoroCdField = "BoroCD";

        protected CommunityDistrictShapeProfile()
        {
        }

        public static CommunityDistrictShape Map(Feature src)
        {
            ArgumentNullException.ThrowIfNull(src);

            return new CommunityDistrictShape
            {
                Cd = int.Parse(GetString(src, BoroCdField).AsSpan(1, 2), System.Globalization.CultureInfo.InvariantCulture),
                BoroCd = ParseInt(src, BoroCdField),
                BoroCode = int.Parse(GetString(src, BoroCdField).AsSpan(0, 1), System.Globalization.CultureInfo.InvariantCulture),
                Borough = GetString(src, BoroCdField).Substring(0, 1).ParseEnum<Borough>().ToString(),
                BoroName = GetString(src, BoroCdField).Substring(0, 1).ParseEnum<Borough>().GetEnumDescription(),
                ShapeArea = ParseDouble(src, "Shape_Area"),
                ShapeLength = ParseDouble(src, "Shape_Leng"),
                BoundingBox = MapBoundingBox(src.BoundingBox),
                Geometry = src.Geometry,
            };
        }
    }
}
