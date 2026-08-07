using MicroService.Service.Models.Base;
using NetTopologySuite.Geometries;
using System.Collections.Generic;

namespace MicroService.Service.Interfaces
{
    // T is intentionally unused in members: it identifies which shape type a
    // given point-lookup service supports (e.g. IPointService<SubwayShape>),
    // matching consumers that reference the closed generic type directly.
    public interface IPointService<out T> where T : ShapeBase // NOSONAR
    {
        public List<Point> FindPointsByRadius(Point center, double radius);
    }
}
