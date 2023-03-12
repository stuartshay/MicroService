using MicroService.Service.Constants;
using MicroService.Service.Helpers;
using MicroService.Service.Models.Enum.Attibutes;
using NetTopologySuite.Geometries;
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


        [InlineData(1016636.9999607186, 187747.02946839959, 40.68193974177307, -73.8832294373166)]
        [Theory]
        [Trait("Category", "Unit")]
        public void Projection_Transform_ESRI102718_to_WGS84(double x, double y, double latitude, double longitude)
        {
            // Transform Nad83 to Wgs84
            var resultWgs = GeoTransformationHelper.ConvertNad83ToWgs84(x, y);

            // Assert
            Assert.Equal(latitude, resultWgs.Latitude!.Value, 6);
            Assert.Equal(longitude, resultWgs.Longitude!.Value, 6);

            // Transform Wgs84 to Nad83
            var resultNad = GeoTransformationHelper.ConvertWgs84ToNad83(latitude: resultWgs.Latitude, longitude: longitude);

            Assert.Equal(resultNad.X!.Value, x, 0);
            Assert.Equal(resultNad.Y!.Value, y, 0);
        }


        [InlineData(40.681939660888951, -73.8832294373166, 1016636.9999607186, 187747.02946839959)]
        [Theory]
        [Trait("Category", "Unit")]
        public void Projection_Transform_WGS84_to_ESRI102718(double latitude, double longitude, double x, double y)
        {
            var result = GeoTransformationHelper.ConvertWgs84ToNad83(latitude, longitude);

            // Assert
            Assert.Equal(x, result.X!.Value, 10);
            Assert.Equal(y, result.Y!.Value, 10);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ConvertNad83ToWgs84_WhenLatitudeIsNull_ReturnsNullValues()
        {
            // Arrange
            double? x = 123.456;
            double? y = null;

            // Act
            var result = GeoTransformationHelper.ConvertNad83ToWgs84(x, y);

            // Assert
            Assert.Null(result.Item1);
            Assert.Null(result.Item2);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ConvertWgs84ToNad83_WithNullValues_ReturnsNull()
        {
            // Arrange
            double? latitude = null;
            double? longitude = null;

            // Act
            var result = GeoTransformationHelper.ConvertWgs84ToNad83(latitude, longitude);

            // Assert
            Assert.Null(result.Item1);
            Assert.Null(result.Item2);
        }


        [InlineData(40.681939660888951, -73.8832294373166, 1016637, 187747.0295)]
        [Theory]
        [Trait("Category", "Unit")]
        public void Projection_Convert_Wgs84_To_Nad83(double latitude, double longitude, double x, double y)
        {
            var result = GeoTransformationHelper.ConvertWgs84ToNad83(new[] { longitude, latitude });

            Assert.Equal(y, result[0], 4);
            Assert.Equal(x, result[1], 4);
        }

        [InlineData(1016637, 187747, 40.681939660888951, -73.8832294373166)]
        [Theory]
        [Trait("Category", "Unit")]
        public void Projection_Convert_Nad83_To_Wgs84(double x, double y, double latitude, double longitude)
        {
            var result = GeoTransformationHelper.ConvertNad83ToWgs84(new[] { x, y });

            Assert.Equal(latitude, result[1], 4);
            Assert.Equal(longitude, result[0], 4);
        }

        [Fact]
        public void ConvertNad83ToWgs84_ReturnsEmptyArray_WhenInputIsNull()
        {
            // Arrange
            double[]? xy = null;

            // Act
            var result = GeoTransformationHelper.ConvertNad83ToWgs84(xy);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void TransformGeometry_ShouldTransformLineString()
        {
            // Arrange
            var fromDatum = Datum.Wgs84;
            var toDatum = Datum.Nad83;
            var lineString = new LineString(new[]
            {
                new Coordinate(12.34, 56.78),
                new Coordinate(90.12, 34.56),
                new Coordinate(78.90, 12.34)
            });


            // Act
            var result = GeoTransformationHelper.TransformGeometry(lineString, fromDatum, toDatum);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<LineString>(result);

            var transformedLineString = (LineString)result;
            Assert.Equal(lineString.Coordinates.Length, transformedLineString.Coordinates.Length);

            for (int i = 0; i < lineString.Coordinates.Length; i++)
            {
                //var expected = wgs84ToNad83 ? ConvertWgs84ToNad83(new[] { lineString.Coordinates[i].X, lineString.Coordinates[i].Y }) : ConvertNad83ToWgs84(new[] { lineString.Coordinates[i].X, lineString.Coordinates[i].Y });
                //var actual = new[] { transformedLineString.Coordinates[i].X, transformedLineString.Coordinates[i].Y };
                //Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void TransformGeometry_ShouldTransformPolygon()
        {
            // Arrange
            var fromDatum = Datum.Wgs84;
            var toDatum = Datum.Nad83;
            var exteriorRing = new[]
            {
                new Coordinate(12.34, 56.78),
                new Coordinate(90.12, 34.56),
                new Coordinate(78.90, 12.34),
                new Coordinate(12.34, 56.78)
            };
            var interiorRing = new[]
            {
                new Coordinate(34.56, 78.90),
                new Coordinate(56.78, 90.12),
                new Coordinate(78.90, 34.56),
                new Coordinate(34.56, 78.90)
            };
            var polygon = new Polygon(new LinearRing(exteriorRing), new[] { new LinearRing(interiorRing) });

            // Act
            var result = GeoTransformationHelper.TransformGeometry(polygon, fromDatum, toDatum);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Polygon>(result);

            var transformedPolygon = (Polygon)result;
            Assert.Equal(exteriorRing.Length, transformedPolygon.ExteriorRing.Coordinates.Length);
            Assert.Equal(interiorRing.Length, transformedPolygon.InteriorRings[0].Coordinates.Length);

            //for (int i = 0; i < exteriorRing.Length; i++)
            //{
            //    var expected = wgs84ToNad83 ? ConvertWgs84ToNad83(new[] { exteriorRing[i].X, exteriorRing[i].Y }) : ConvertNad83ToWgs84(new[] { exteriorRing[i].X, exteriorRing[i].Y });
            //    var actual = new[] { transformedPolygon.ExteriorRing.Coordinates[i].X, transformedPolygon.ExteriorRing.Coordinates[i].Y };
            //    Assert.Equal(expected, actual);
            //}

            //for (int i = 0; i < interiorRing.Length; i++)
            //{
            //    var expected = wgs84ToNad83 ? ConvertWgs84ToNad83(new[] { interiorRing[i].X, interiorRing[i].Y }) : ConvertNad83ToWgs84(new[] { interiorRing[i].X, interiorRing[i].Y });
            //    var actual = new[] { transformedPolygon.InteriorRings[0].Coordinates[i].X, transformedPolygon.InteriorRings[0].Coordinates[i].Y };
            //    Assert.Equal(expected, actual);
            //}
        }

        [Fact]
        public void ConvertNad83ToWgs84_ReturnsEmptyArray_WhenInputHasIncorrectLength()
        {
            // Arrange
            double[] xy = { 1.0, 2.0, 3.0 };

            // Act
            var result = GeoTransformationHelper.ConvertNad83ToWgs84(xy);

            // Assert
            Assert.Empty(result);
        }


        [InlineData(40.681939660888951, -73.8832294373166, 187747.0294683996, 1016636.9999607186)]
        [Theory]
        [Trait("Category", "Unit")]
        public void TestTransformGeometry(double latitude, double longitude, double x, double y)
        {
            // Create a point in WGS84
            var point = new Point(longitude, latitude) { SRID = 4326 };

            // Transform the point to NAD83
            var transformedPoint = GeoTransformationHelper.TransformGeometry(point, Datum.Wgs84, Datum.Nad83);

            //// Assert that the transformed point is in NAD83
            //Assert.Equal(2263, transformedPoint.SRID);

            Assert.Equal(x, transformedPoint.Coordinates[0].X, 4);
            Assert.Equal(y, transformedPoint.Coordinates[0].Y, 4);

            // Transform the point back to WGS84
            var backTransformedPoint = GeoTransformationHelper.TransformGeometry(transformedPoint, Datum.Nad83, Datum.Wgs84);
            //Assert.Equal(longitude, backTransformedPoint.Coordinates[0].X, 4);
            //Assert.Equal(latitude, backTransformedPoint.Coordinates[0].Y, 4);

            // Assert that the back-transformed point is in WGS84 and equal to the original point

            //Assert.Equal(4326, backTransformedPoint.SRID);
            //Assert.Equal(point.X, backTransformedPoint.Coordinates[0].X, 1e-6);
            //Assert.Equal(point.Y, backTransformedPoint.Coordinates[0].Y, 1e-6);
        }





    }
}
