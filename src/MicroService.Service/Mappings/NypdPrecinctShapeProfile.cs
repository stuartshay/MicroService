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
                Precinct = ParseInt(src, "Precinct"),
                ShapeArea = ParseDouble(src, "Shape_Area"),
                ShapeLength = ParseDouble(src, "Shape_Leng"),
                BoundingBox = MapBoundingBox(src.BoundingBox),
                Geometry = src.Geometry,
            };
        }
    }
}
