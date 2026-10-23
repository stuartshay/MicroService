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
                Cd = int.Parse((src.Attributes[BoroCdField]?.ToString() ?? string.Empty).Substring(1, 2)),
                BoroCd = int.Parse(src.Attributes[BoroCdField]?.ToString() ?? string.Empty),
                BoroCode = int.Parse((src.Attributes[BoroCdField]?.ToString() ?? string.Empty).Substring(0, 1)),
                Borough = (src.Attributes[BoroCdField]?.ToString() ?? string.Empty).Substring(0, 1).ParseEnum<Borough>().ToString(),
                BoroName = (src.Attributes[BoroCdField]?.ToString() ?? string.Empty).Substring(0, 1).ParseEnum<Borough>().GetEnumDescription(),
                ShapeArea = double.Parse(src.Attributes["Shape_Area"]?.ToString() ?? string.Empty),
                ShapeLength = double.Parse(src.Attributes["Shape_Leng"]?.ToString() ?? string.Empty),
                BoundingBox = MapBoundingBox(src.BoundingBox),
                Geometry = src.Geometry,
            };
        }
    }
}
