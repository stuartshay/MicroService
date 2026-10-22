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
                BoroCode = int.Parse(src.Attributes["BoroCode"]?.ToString() ?? string.Empty),
                BoroName = src.Attributes["BoroName"]?.ToString() ?? string.Empty,
                ShapeArea = double.Parse(src.Attributes["Shape_Area"]?.ToString() ?? string.Empty),
                ShapeLength = double.Parse(src.Attributes["Shape_Leng"]?.ToString() ?? string.Empty),
                BoundingBox = MapBoundingBox(src.BoundingBox),
                Geometry = src.Geometry,
            };
        }
    }
}
