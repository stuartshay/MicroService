using MicroService.Service.Models.Base;
using NetTopologySuite.Geometries;
using System.Collections.Generic;

namespace MicroService.Service.Interfaces
{
    public interface IPointService<out T> where T : ShapeBase
    {
        public List<Point> FindPointsByRadius(Point center, double radius);
    }
}
