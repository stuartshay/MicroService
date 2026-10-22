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
                BoroCode = int.Parse(src.Attributes["BoroCode"]?.ToString() ?? string.Empty),
                BoroName = src.Attributes["BoroName"]?.ToString() ?? string.Empty,
                CountyFIPS = src.Attributes["CountyFIPS"]?.ToString() ?? string.Empty,
                NTACode = src.Attributes["NTACode"]?.ToString() ?? string.Empty,
                NTAName = src.Attributes["NTAName"]?.ToString() ?? string.Empty,
                ShapeArea = double.Parse(src.Attributes["Shape_Area"]?.ToString() ?? string.Empty),
                ShapeLength = double.Parse(src.Attributes["Shape_Leng"]?.ToString() ?? string.Empty),
                BoundingBox = MapBoundingBox(src.BoundingBox),
                Geometry = src.Geometry,
            };
        }
    }
}
