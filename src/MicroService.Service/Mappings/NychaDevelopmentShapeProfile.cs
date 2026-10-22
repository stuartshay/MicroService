using MicroService.Service.Mappings.Base;
using MicroService.Service.Models;
using NetTopologySuite.Features;
using System;

namespace MicroService.Service.Mappings
{
    public class NychaDevelopmentShapeProfile : ShapeProfile<NychaDevelopmentShape>
    {
        protected NychaDevelopmentShapeProfile()
        {
        }

        public static NychaDevelopmentShape Map(Feature src)
        {
            ArgumentNullException.ThrowIfNull(src);

            return new NychaDevelopmentShape
            {
                Development = src.Attributes["DEVELOPMEN"]?.ToString()!,
                TdsNumber = src.Attributes["TDS_NUM"] != null ? src.Attributes["TDS_NUM"]?.ToString()! : null!,
                Borough = src.Attributes["BOROUGH"] != null ? src.Attributes["BOROUGH"]?.ToString()! : null!,
                BoundingBox = MapBoundingBox(src.BoundingBox),
                Geometry = src.Geometry,
            };
        }
    }
}
