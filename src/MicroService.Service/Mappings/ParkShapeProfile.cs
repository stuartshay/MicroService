using MicroService.Service.Mappings.Base;
using MicroService.Service.Models;
using NetTopologySuite.Features;
using System;

namespace MicroService.Service.Mappings
{
    public class ParkShapeProfile : ShapeProfile<ParkShape>
    {
        protected ParkShapeProfile()
        {
        }

        public static ParkShape Map(Feature src)
        {
            ArgumentNullException.ThrowIfNull(src);

            return new ParkShape
            {
                ParkName = src.Attributes["PARK_NAME"]?.ToString() ?? string.Empty,
                ParkNumber = src.Attributes["PARKNUM"]?.ToString() ?? string.Empty,
                SourceId = ParseLong(src, "SOURCE_ID"),
                FeatureCode = ParseInt(src, "FEAT_CODE"),
                SubCode = ParseInt(src, "SUB_CODE"),
                LandUse = src.Attributes["LANDUSE"]?.ToString() ?? string.Empty,
                System = src.Attributes["SYSTEM"]?.ToString() ?? string.Empty,
                Status = src.Attributes["STATUS"]?.ToString() ?? string.Empty,
                ShapeArea = ParseDouble(src, "SHAPE_Area"),
                ShapeLength = ParseDouble(src, "SHAPE_Leng"),
                BoundingBox = MapBoundingBox(src.BoundingBox),
                Geometry = src.Geometry,
            };
        }
    }
}
