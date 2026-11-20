using MicroService.Service.Mappings.Base;
using MicroService.Service.Models;
using NetTopologySuite.Features;
using System;

namespace MicroService.Service.Mappings
{
    public class SubwayServiceLineShapeProfile : ShapeProfile<SubwayServiceLineShape>
    {
        protected SubwayServiceLineShapeProfile()
        {
        }

        public static SubwayServiceLineShape Map(Feature src)
        {
            ArgumentNullException.ThrowIfNull(src);

            return new SubwayServiceLineShape
            {
                // The export writes objectid as a numeric field with decimals (5.000...)
                ObjectId = (int)ParseDouble(src, "objectid"),
                Service = GetString(src, "service").Trim(),
                ServiceName = GetString(src, "service_na").Trim(),
                ShapeLength = ParseDouble(src, "shape_stle"),
                BoundingBox = MapBoundingBox(src.BoundingBox),
                Geometry = src.Geometry,
            };
        }
    }
}
