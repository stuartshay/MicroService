using MicroService.Service.Helpers;
using NetTopologySuite.Geometries;
using Xunit;

namespace MicroService.Test.Unit
{
    [Trait("Category", "Unit")]
    public class GeoTransformationCopyTest
    {
        private static readonly GeometryFactory Factory = new();

        // NAD83 State Plane Long Island (US feet) and WGS84 for the same point (FunctionHelperUnitTest)
        private const double X = 1016636.9999607186;
        private const double Y = 187747.02946839959;
        private const double Longitude = -73.8832294373166;
        private const double Latitude = 40.681939660888951;

        [Fact(DisplayName = "CopyNad83ToWgs84 - converts every coordinate and leaves the source unchanged")]
        public void CopyNad83ToWgs84_ConvertsACopy()
        {
            var source = Factory.CreateLineString([new Coordinate(X, Y), new Coordinate(X + 1000, Y + 1000)]);

            var copy = GeoTransformationHelper.CopyNad83ToWgs84(source);

            Assert.NotSame(source, copy);
            Assert.Equal(X, source.Coordinates[0].X);
            Assert.Equal(Y, source.Coordinates[0].Y);

            Assert.Equal(Longitude, copy.Coordinates[0].X, 6);
            Assert.Equal(Latitude, copy.Coordinates[0].Y, 6);
            var (longitude, latitude) = GeoTransformationHelper.ConvertNad83ToWgs84(X + 1000, Y + 1000);
            Assert.Equal(longitude!.Value, copy.Coordinates[1].X, 9);
            Assert.Equal(latitude!.Value, copy.Coordinates[1].Y, 9);
        }

        [Fact(DisplayName = "CopyNad83ToWgs84 - converts multi-polygons, holes included, and updates the envelope")]
        public void CopyNad83ToWgs84_ConvertsMultiPolygons()
        {
            var shell = Factory.CreateLinearRing(
                [new(X, Y), new(X + 5000, Y), new(X + 5000, Y + 5000), new(X, Y + 5000), new(X, Y)]);
            var hole = Factory.CreateLinearRing(
                [new(X + 1000, Y + 1000), new(X + 2000, Y + 1000), new(X + 2000, Y + 2000), new(X + 1000, Y + 1000)]);
            var source = Factory.CreateMultiPolygon([Factory.CreatePolygon(shell, [hole])]);

            var copy = GeoTransformationHelper.CopyNad83ToWgs84(source);

            Assert.IsType<MultiPolygon>(copy);
            Assert.Equal(source.NumPoints, copy.NumPoints);
            Assert.All(copy.Coordinates, c =>
            {
                Assert.InRange(c.X, -74.0, -73.8);
                Assert.InRange(c.Y, 40.6, 40.8);
            });
            Assert.Equal(Longitude, copy.EnvelopeInternal.MinX, 3);
            Assert.True(((Polygon)copy.GetGeometryN(0)).Holes.Single().Coordinates[0].X > copy.EnvelopeInternal.MinX);
            Assert.True(copy.IsValid);
        }

        [Fact(DisplayName = "CopyNad83ToWgs84 - a null geometry is an error")]
        public void CopyNad83ToWgs84_RejectsNull()
        {
            Assert.Throws<ArgumentNullException>(() => GeoTransformationHelper.CopyNad83ToWgs84(null!));
        }
    }
}
