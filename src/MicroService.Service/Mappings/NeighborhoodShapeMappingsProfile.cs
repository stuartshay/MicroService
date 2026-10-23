using MicroService.Service.Mappings.Base;
using MicroService.Service.Models;
using NetTopologySuite.Features;
using System;

namespace MicroService.Service.Mappings
{
    public class NeighborhoodShapeMappingsProfile : ShapeProfile<NeighborhoodShape>
    {
        protected NeighborhoodShapeMappingsProfile()
        {
        }

        public static NeighborhoodShape Map(Feature src)
        {
            ArgumentNullException.ThrowIfNull(src);

            return new NeighborhoodShape
            {
                BoroCode = ParseInt(src, "BoroCode"),
                BoroName = src.Attributes["BoroName"]?.ToString() ?? string.Empty,
                CountyFIPS = src.Attributes["CountyFIPS"]?.ToString() ?? string.Empty,
                NTACode = src.Attributes["NTACode"]?.ToString() ?? string.Empty,
                NTAName = src.Attributes["NTAName"]?.ToString() ?? string.Empty,
                ShapeArea = ParseDouble(src, "Shape_Area"),
                ShapeLength = ParseDouble(src, "Shape_Leng"),
                BoundingBox = MapBoundingBox(src.BoundingBox),
                Geometry = src.Geometry,
            };
        }
    }
}
