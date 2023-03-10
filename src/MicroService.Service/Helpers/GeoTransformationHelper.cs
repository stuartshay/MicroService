using NetTopologySuite.Geometries;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using System;

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
        public static (double?, double?) ConvertNad83ToWgs84(double? x, double? y)
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
                return null;

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
        public static (double?, double?) ConvertWgs84ToNad83(double? latitude, double? longitude)
        {
            if (!latitude.HasValue || !longitude.HasValue)
                return (null, null);

            var csWgs84 = GeographicCoordinateSystem.WGS84;
            var csFact = new CoordinateSystemFactory();
            var utmNad83 = csFact.CreateFromWkt(Epsg2263EsriWkt);

            var ctFactory = new CoordinateTransformationFactory();
            var trans = ctFactory.CreateFromCoordinateSystems(csWgs84, utmNad83);
            var result = trans.MathTransform.Transform(new[] { longitude.Value, latitude.Value });

            return (result[1], result[0]);
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

        public static Geometry TransformGeometry(Geometry geometry, MicroService.Service.Models.Enum.Datum fromDatum, MicroService.Service.Models.Enum.Datum toDatum)
        {
            bool wgs84ToNad83 = fromDatum == Models.Enum.Datum.Wgs84 && toDatum == Models.Enum.Datum.Nad83;
            bool nad83ToWgs84 = fromDatum == Models.Enum.Datum.Nad83 && toDatum == Models.Enum.Datum.Wgs84;

            if (!wgs84ToNad83 && !nad83ToWgs84)
            {
                throw new ArgumentException("Invalid datum combination.");
            }

            if (geometry is Point point)
            {
                var xy = new[] { point.X, point.Y };
                xy = wgs84ToNad83 ? GeoTransformationHelper.ConvertWgs84ToNad83(xy) : GeoTransformationHelper.ConvertNad83ToWgs84(xy);
                point.X = xy[0];
                point.Y = xy[1];
            }
            else if (geometry is LineString lineString)
            {
                for (int i = 0; i < lineString.Coordinates.Length; i++)
                {
                    var xy = new[] { lineString.Coordinates[i].X, lineString.Coordinates[i].Y };
                    xy = wgs84ToNad83 ? GeoTransformationHelper.ConvertWgs84ToNad83(xy) : GeoTransformationHelper.ConvertNad83ToWgs84(xy);
                    lineString.Coordinates[i].X = xy[0];
                    lineString.Coordinates[i].Y = xy[1];
                }
            }
            else if (geometry is Polygon polygon)
            {
                for (int i = 0; i < polygon.Shell.Coordinates.Length; i++)
                {
                    var xy = new[] { polygon.Shell.Coordinates[i].X, polygon.Shell.Coordinates[i].Y };
                    xy = wgs84ToNad83 ? GeoTransformationHelper.ConvertWgs84ToNad83(xy) : GeoTransformationHelper.ConvertNad83ToWgs84(xy);
                    polygon.Shell.Coordinates[i].X = xy[0];
                    polygon.Shell.Coordinates[i].Y = xy[1];
                }
                for (int i = 0; i < polygon.Holes.Length; i++)
                {
                    for (int j = 0; j < polygon.Holes[i].Coordinates.Length; j++)
                    {
                        var xy = new[] { polygon.Holes[i].Coordinates[j].X, polygon.Holes[i].Coordinates[j].Y };
                        xy = wgs84ToNad83 ? GeoTransformationHelper.ConvertWgs84ToNad83(xy) : GeoTransformationHelper.ConvertNad83ToWgs84(xy);
                        polygon.Holes[i].Coordinates[j].X = xy[0];
                        polygon.Holes[i].Coordinates[j].Y = xy[1];
                    }
                }
            }

            return geometry;
        }


    }
}
