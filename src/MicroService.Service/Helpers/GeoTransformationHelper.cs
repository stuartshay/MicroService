using NetTopologySuite.Geometries;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using System;
using Datum = MicroService.Service.Models.Enum.Attributes.Datum;

namespace MicroService.Service.Helpers
{
    public static class GeoTransformationHelper
    {
        const string Epsg2263EsriWkt = "PROJCS[\"NAD83 / New York Long Island (ftUS)\",GEOGCS[\"GCS_North_American_1983\",DATUM[\"D_North_American_1983\",SPHEROID[\"GRS_1980\",6378137,298.257222101]],PRIMEM[\"Greenwich\",0],UNIT[\"Degree\",0.017453292519943295]],PROJECTION[\"Lambert_Conformal_Conic\"],PARAMETER[\"standard_parallel_1\",41.03333333333333],PARAMETER[\"standard_parallel_2\",40.66666666666666],PARAMETER[\"latitude_of_origin\",40.16666666666666],PARAMETER[\"central_meridian\",-74],PARAMETER[\"false_easting\",984250.0000000002],PARAMETER[\"false_northing\",0],UNIT[\"Foot_US\",0.30480060960121924]]";

        /// <summary>
        /// Convert NAD83 to WGS84
        /// https://spatialreference.org/ref/epsg/nad83-new-york-long-island-ftus/
        /// Ersi WKT
        /// https://spatialreference.org/ref/epsg/nad83-new-york-long-island-ftus/esriwkt/
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static (double? Longitude, double? Latitude) ConvertNad83ToWgs84(double? x, double? y)
        {
            if (!x.HasValue || !y.HasValue)
                return (null, null);

            var csWgs84 = GeographicCoordinateSystem.WGS84;
            var csFact = new CoordinateSystemFactory();
            var utmNad83 = csFact.CreateFromWkt(Epsg2263EsriWkt);

            var ctFactory = new CoordinateTransformationFactory();
            var trans = ctFactory.CreateFromCoordinateSystems(utmNad83, csWgs84);

            var result = trans.MathTransform.Transform(new[] { x.Value, y.Value });

            return (result[0], result[1]);
        }

        public static double[] ConvertNad83ToWgs84(double[] xy)
        {
            if (xy == null || xy.Length != 2)
                return Array.Empty<double>();

            var csWgs84 = GeographicCoordinateSystem.WGS84;
            var csFact = new CoordinateSystemFactory();
            var utmNad83 = csFact.CreateFromWkt(Epsg2263EsriWkt);

            var ctFactory = new CoordinateTransformationFactory();
            var trans = ctFactory.CreateFromCoordinateSystems(utmNad83, csWgs84);

            var result = trans.MathTransform.Transform(xy);

            return new[] { result[0], result[1] };
        }

        /// <summary>
        /// Convert NAD83 to WGS84
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <returns></returns> 
        public static (double? X, double? Y) ConvertWgs84ToNad83(double? x, double? y)
        {
            if (!x.HasValue || !y.HasValue)
                return (null, null);

            var csWgs84 = GeographicCoordinateSystem.WGS84;
            var csFact = new CoordinateSystemFactory();
            var utmNad83 = csFact.CreateFromWkt(Epsg2263EsriWkt);

            var ctFactory = new CoordinateTransformationFactory();
            var trans = ctFactory.CreateFromCoordinateSystems(csWgs84, utmNad83);
            var result = trans.MathTransform.Transform(new[] { x.Value, y.Value });

            return (result[0], result[1]);
        }

        public static double[] ConvertWgs84ToNad83(double[] point)
        {
            if (point == null || point.Length != 2)
                throw new ArgumentException("Point must be an array of two doubles", nameof(point));

            var csWgs84 = GeographicCoordinateSystem.WGS84;
            var csFact = new CoordinateSystemFactory();
            var utmNad83 = csFact.CreateFromWkt(Epsg2263EsriWkt);

            var ctFactory = new CoordinateTransformationFactory();
            var trans = ctFactory.CreateFromCoordinateSystems(csWgs84, utmNad83);
            var result = trans.MathTransform.Transform(point);

            return new[] { result[1], result[0] };
        }

        public static Geometry TransformGeometry(Geometry geometry, Datum fromDatum, Datum toDatum)
        {
            bool wgs84ToNad83 = fromDatum == Datum.Wgs84 && toDatum == Datum.Nad83;
            bool nad83ToWgs84 = fromDatum == Datum.Nad83 && toDatum == Datum.Wgs84;

            if (!wgs84ToNad83 && !nad83ToWgs84)
            {
                throw new ArgumentException("Invalid datum combination.");
            }

            switch (geometry)
            {
                case Point point:
                    TransformCoordinates(point.Coordinates, wgs84ToNad83);
                    break;
                case LineString lineString:
                    TransformCoordinates(lineString.Coordinates, wgs84ToNad83);
                    break;
                case Polygon polygon:
                    TransformCoordinates(polygon.Shell.Coordinates, wgs84ToNad83);
                    foreach (var hole in polygon.Holes)
                    {
                        TransformCoordinates(hole.Coordinates, wgs84ToNad83);
                    }
                    break;
            }

            return geometry;
        }

        private static void TransformCoordinates(IEnumerable<Coordinate> coordinates, bool wgs84ToNad83)
        {
            foreach (var coordinate in coordinates)
            {
                var transformed = wgs84ToNad83
                    ? ConvertWgs84ToNad83(new[] { coordinate.X, coordinate.Y })
                    : ConvertNad83ToWgs84(new[] { coordinate.X, coordinate.Y });

                coordinate.X = transformed[0];
                coordinate.Y = transformed[1];
            }
        }

    }
}
