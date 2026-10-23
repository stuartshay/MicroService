using MicroService.Service.Mappings.Base;
using MicroService.Service.Models;
using NetTopologySuite.Features;
using System;

namespace MicroService.Service.Mappings
{
    public class SubwayShapeProfile : ShapeProfile<SubwayShape>
    {
        protected SubwayShapeProfile()
        {
        }

        public static SubwayShape Map(Feature src)
        {
            ArgumentNullException.ThrowIfNull(src);

            return new SubwayShape
            {
                Line = src.Attributes["line"]?.ToString() ?? string.Empty,
                Name = src.Attributes["name"]?.ToString() ?? string.Empty,
                ObjectId = int.Parse(src.Attributes["objectid"]?.ToString() ?? string.Empty),
                BoundingBox = MapBoundingBox(src.BoundingBox),
                Geometry = src.Geometry,
            };
        }
    }
}
