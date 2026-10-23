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
                SourceId = long.Parse(src.Attributes["SOURCE_ID"]?.ToString() ?? string.Empty),
                FeatureCode = int.Parse(src.Attributes["FEAT_CODE"]?.ToString() ?? string.Empty),
                SubCode = int.Parse(src.Attributes["SUB_CODE"]?.ToString() ?? string.Empty),
                LandUse = src.Attributes["LANDUSE"]?.ToString() ?? string.Empty,
                System = src.Attributes["SYSTEM"]?.ToString() ?? string.Empty,
                Status = src.Attributes["STATUS"]?.ToString() ?? string.Empty,
                ShapeArea = double.Parse(src.Attributes["SHAPE_Area"]?.ToString() ?? string.Empty),
                ShapeLength = double.Parse(src.Attributes["SHAPE_Leng"]?.ToString() ?? string.Empty),
                BoundingBox = MapBoundingBox(src.BoundingBox),
                Geometry = src.Geometry,
            };
        }
    }
}
