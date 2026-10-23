using MicroService.Service.Mappings.Base;
using MicroService.Service.Models;
using NetTopologySuite.Features;
using System;

namespace MicroService.Service.Mappings
{
    public class BoroughBoundaryShapeProfile : ShapeProfile<BoroughBoundaryShape>
    {
        protected BoroughBoundaryShapeProfile()
        {
        }

        public static BoroughBoundaryShape Map(Feature src)
        {
            ArgumentNullException.ThrowIfNull(src);

            return new BoroughBoundaryShape
            {
                BoroCode = ParseInt(src, "BoroCode"),
                BoroName = src.Attributes["BoroName"]?.ToString() ?? string.Empty,
                ShapeArea = ParseDouble(src, "Shape_Area"),
                ShapeLength = ParseDouble(src, "Shape_Leng"),
                BoundingBox = MapBoundingBox(src.BoundingBox),
                Geometry = src.Geometry,
            };
        }
    }
}
