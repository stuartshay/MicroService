using MicroService.Service.Mappings;
using MicroService.Service.Models;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using Xunit;

namespace MicroService.Test.Mappings;

public class BoroughBoundaryShapeTest
{
    [InlineData(1, "Manhattan", 1, "Manhattan")]
    [InlineData("1", "Manhattan", 1, "Manhattan")]
    [Theory(DisplayName = "Validate Mappings")]
    public void TestShapeMapping(object value1, object value2, int expected1, string expected2)
    {
        // Arrange: the profile maps from a shapefile Feature, keyed by the .dbf column names
        var feature = CreateFeature(value1, value2);

        // Act
        var shape = BoroughBoundaryShapeProfile.Map(feature);

        // Assert
        Assert.IsType<BoroughBoundaryShape>(shape);
        Assert.Equal(expected1, shape.BoroCode);
        Assert.Equal(expected2, shape.BoroName);
        Assert.Equal(1.5, shape.ShapeArea);
        Assert.Equal(2.5, shape.ShapeLength);
    }

    [InlineData("Invalid", "Manhattan")]
    [Theory(DisplayName = "Validate Mappings - Invalid")]
    public void TestShapeMapping_Invalid(string value1, string value2)
    {
        // Arrange
        var feature = CreateFeature(value1, value2);

        // Act & Assert
        Assert.Throws<FormatException>(() => BoroughBoundaryShapeProfile.Map(feature));
    }

    private static Feature CreateFeature(object boroCode, object boroName) =>
        new(new Point(0, 0), new AttributesTable(new Dictionary<string, object>
        {
            { "BoroCode", boroCode },
            { "BoroName", boroName },
            { "Shape_Area", 1.5 },
            { "Shape_Leng", 2.5 },
        }));
}
