using GeoAPI.Geometries;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;

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
        public static (double?, double?) Nad83TransformWgs84(double? x, double? y)
        {
            if (!x.HasValue || !y.HasValue)
                return (null, null);

            var csWgs84 = GeographicCoordinateSystem.WGS84;
            var csFact = new CoordinateSystemFactory();
            var utmNad83 = csFact.CreateFromWkt(Epsg2263EsriWkt);


            var ctFactory = new CoordinateTransformationFactory();
            var trans = ctFactory.CreateFromCoordinateSystems(utmNad83, csWgs84);
            var result = trans.MathTransform.Transform(new Coordinate { X = (double)x, Y = (double)y});
           
            return (result.X, result.Y);
        }


        /// <summary>
        /// Convert NAD83 to WGS84
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <returns></returns> 
        public static (double?, double?) Wgs84TransformNad83(double? latitude, double? longitude)
        {
            if (!latitude.HasValue || !longitude.HasValue)
                return (null, null);

            var csWgs84 = GeographicCoordinateSystem.WGS84;

            var csFact = new CoordinateSystemFactory();
            var utmNad83 = csFact.CreateFromWkt(Epsg2263EsriWkt);

            var ctFactory = new CoordinateTransformationFactory();
            var trans = ctFactory.CreateFromCoordinateSystems(csWgs84, utmNad83);
            var result = trans.MathTransform.Transform(new Coordinate { X = (double)longitude, Y = (double)latitude });

            return (result.X, result.Y);
        } 
    }
}
