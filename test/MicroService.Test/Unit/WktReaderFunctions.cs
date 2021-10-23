using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace MicroService.Test.Unit
{
    public static class WktReaderFunctions
    {
        public static void GeometryContainsPoint()
        {
            var reader = new NetTopologySuite.IO.WKTReader();
            Geometry poly = reader.Read(
                @"POLYGON ((428999.76819468878 360451.93329044303, 428998.25517286535 360420.80827007542,
429023.1119599645 360406.75878171506, 429004.52340613387 360451.71714446822, 
429004.52340613387 360451.71714446822, 428999.76819468878 360451.93329044303))");


            var points = new List<NetTopologySuite.Geometries.Geometry>(new[]
            {
                reader.Factory.CreatePoint(new Coordinate(429012.5, 360443.18)),   
                reader.Factory.CreatePoint(new NetTopologySuite.Geometries.Coordinate(429001.59, 360446.98)),
                reader.Factory.CreatePoint(new NetTopologySuite.Geometries.Coordinate(429003.31, 360425.45)),
                reader.Factory.CreatePoint(new NetTopologySuite.Geometries.Coordinate(429016.9, 360413.04))
            });


            var inside = new List<bool>(new[] { false, true, true, true });

            for (var i = 0; i < points.Count; i++)
            {
                var contains = poly.Contains(points[i]);
                System.Console.Write(poly.Contains(points[i]));
            }
        }

    }
}
