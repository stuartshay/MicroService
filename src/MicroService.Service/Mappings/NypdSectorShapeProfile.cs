using MicroService.Service.Mappings.Base;
using MicroService.Service.Models;
using NetTopologySuite.Features;
using System;

namespace MicroService.Service.Mappings
{
    public class NypdSectorShapeProfile : ShapeProfile<NypdSectorShape>
    {
        protected NypdSectorShapeProfile()
        {
        }

        public static NypdSectorShape Map(Feature src)
        {
            ArgumentNullException.ThrowIfNull(src);

            return new NypdSectorShape
            {
                Pct = src.Attributes["pct"]?.ToString() ?? string.Empty,
                Sector = src.Attributes["sector"]?.ToString() ?? string.Empty,
                PatrolBoro = src.Attributes["patrol_bor"]?.ToString() ?? string.Empty,
                Phase = src.Attributes["phase"]?.ToString() ?? string.Empty,
                BoundingBox = MapBoundingBox(src.BoundingBox),
                Geometry = src.Geometry,
            };
        }
    }
}
