using MicroService.Service.Mappings.Base;
using MicroService.Service.Models;
using NetTopologySuite.Features;
using System;

namespace MicroService.Service.Mappings
{
    public class NypdPrecinctShapeProfile : ShapeProfile<NypdPrecinctShape>
    {
        protected NypdPrecinctShapeProfile()
        {
        }

        public static NypdPrecinctShape Map(Feature src)
        {
            ArgumentNullException.ThrowIfNull(src);

            return new NypdPrecinctShape
            {
                Precinct = int.Parse(src.Attributes["Precinct"]?.ToString() ?? string.Empty),
                ShapeArea = double.Parse(src.Attributes["Shape_Area"]?.ToString() ?? string.Empty),
                ShapeLength = double.Parse(src.Attributes["Shape_Leng"]?.ToString() ?? string.Empty),
                BoundingBox = MapBoundingBox(src.BoundingBox),
                Geometry = src.Geometry,
            };
        }
    }
}
