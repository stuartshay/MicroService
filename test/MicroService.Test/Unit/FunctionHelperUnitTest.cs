using System;
using System.Drawing;
using GeoAPI.Geometries;
using MicroService.Service.Constants;
using MicroService.Service.Helpers;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using Xunit;

namespace MicroService.Test.Unit
{
    public class FunctionHelperUnitTest
    {
        [Fact(DisplayName = "Calculate_Percentile - Unit")]
        [Trait("Category", "Unit")]
        public void Can_Calculate_Percentile()
        {
            double result1 = 3.4d;
            var calculated1 = FunctionHelper.Percentile(new double[] { 1, 2, 3, 4 }, 0.8);

            double result2 = 8.05d;
            var calculated2 = FunctionHelper.Percentile(new double[] { 7, 8, 9, 20 }, 0.35);

            double result3 = 1.9d;
            var calculated3 = FunctionHelper.Percentile(new double[] { 1, 2, 3, 4 }, 0.3);

            // Assert
            Assert.Equal(result1, Math.Round(calculated1, DataConstants.PercentilePrecision));
            Assert.Equal(result2, Math.Round(calculated2, DataConstants.PercentilePrecision));
            Assert.Equal(result3, Math.Round(calculated3, DataConstants.PercentilePrecision));
        }


        [Fact]
        public void Projection_Transform_ESRI102718_to_WGS84()
        {
            //ProjCoordinate pSource = new ProjCoordinate();
            //ProjCoordinate pTarget = new ProjCoordinate();
            //double tolerance = 0.1;

            //pSource.X = 989827.532190999;
            //pSource.Y = 227456.77442;
            //pTarget = ProjectionHelper.TransformProjection(pSource);

            //double dx = Math.Abs(pTarget.X - -73.972510000572626);
            //double dy = Math.Abs(pTarget.Y - 40.770205001318345);
            //Boolean isInTol = dx <= tolerance && dy <= tolerance;

            //Console.WriteLine(pSource.X + " " + pSource.Y);
            //Console.WriteLine(pTarget.X + " " + pTarget.Y);
            //Console.WriteLine("Is In Tolerance:" + isInTol);

            //Assert.IsTrue(isInTol);
        }

        [Fact]
        public void TestTransformListOfCoordinates()
        {
            var csFact = new CoordinateSystemFactory();
            var ctFact = new CoordinateTransformationFactory();
            
            var csWgs84 = GeographicCoordinateSystem.WGS84;
            var csUtm35N = ProjectedCoordinateSystem.WGS84_UTM(35, true);

            var fileText = System.IO.File.ReadAllText(@"F:\Build3\MicroService\test\MicroService.Test\Files\EPSG-2263-Ersi.wkt");





            //var utmNad83 = csFact.CreateFromWkt(
            //    "PROJCS["\NAD83 / New York Long Island (ftUS)\", GEOGCS[\"GCS_North_American_1983", DATUM[\"D_North_American_1983", SPHEROID["GRS_1980", 6378137, 298.257222101]], PRIMEM["Greenwich", 0], UNIT["Degree", 0.017453292519943295]], PROJECTION["Lambert_Conformal_Conic"], PARAMETER["standard_parallel_1", 41.03333333333333], PARAMETER["standard_parallel_2", 40.66666666666666], PARAMETER["latitude_of_origin", 40.16666666666666], PARAMETER["central_meridian", -74], PARAMETER["false_easting", 984250.0000000002], PARAMETER["false_northing", 0], UNIT["Foot_US", 0.30480060960121924]]";

            var utm35ETRS = csFact.CreateFromWkt(
                "PROJCS[\"ETRS89 / ETRS-TM35\",GEOGCS[\"ETRS89\",DATUM[\"D_ETRS_1989\",SPHEROID[\"GRS_1980\",6378137,298.257222101]],PRIMEM[\"Greenwich\",0],UNIT[\"Degree\",0.017453292519943295]],PROJECTION[\"Transverse_Mercator\"],PARAMETER[\"latitude_of_origin\",0],PARAMETER[\"central_meridian\",27],PARAMETER[\"scale_factor\",0.9996],PARAMETER[\"false_easting\",500000],PARAMETER[\"false_northing\",0],UNIT[\"Meter\",1]]");

            var utm33 = ProjectedCoordinateSystem.WGS84_UTM(33, true);

            var trans = ctFact.CreateFromCoordinateSystems(utm35ETRS, utm33);
            var result = trans.MathTransform.Transform(new Coordinate {X = 1, Y = 1});

            var x = result;





            //ProjNet.Geometries.XY[] points =
            //{
            //    new XY(290586.087, 6714000), new XY(290586.392, 6713996.224),
            //    new XY(290590.133, 6713973.772), new XY(290594.111, 6713957.416),
            //    new XY(290596.615, 6713943.567), new XY(290596.701, 6713939.485)
            //};

            //var tpoints = (XY[])points.Clone();
            //trans.MathTransform.Transform(tpoints);
            //for (int i = 0; i < points.Length; i++)
            //{
            //    double expectedX = points[i].X;
            //    double expectedY = points[i].Y;
            //    trans.MathTransform.Transform(ref expectedX, ref expectedY);

            //    double actualX = tpoints[i].X;
            //    double actualY = tpoints[i].Y;

            //    Assert.That(actualX, Is.EqualTo(expectedX).Within(1E-8));
            //    Assert.That(actualY, Is.EqualTo(expectedY).Within(1E-8));
            //}
        }

    }
}
