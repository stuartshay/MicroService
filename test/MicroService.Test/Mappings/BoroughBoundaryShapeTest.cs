using AutoMapper;
using MicroService.Service.Models;
using Xunit;

namespace MicroService.Test.Mappings;

public class BoroughBoundaryShapeTest : BaseMapperTest<BoroughBoundaryShape>
{
    [InlineData(1, "Manhattan", 1, "Manhattan")]
    [InlineData("1", "Manhattan", 1, "Manhattan")]
    [Theory(DisplayName = "Validate Mappings")]
    public void TestAutomapperMapping(object value1, object value2, int expected1, string expected2)
    {
        // Arrange
        var attributes = new List<KeyValuePair<string, object>>()
        {
            new("BoroCode", value1),
            new("BoroName", value2),
        };

        // Act
        var shape = Mapper.Map<BoroughBoundaryShape>(attributes);

        // Assert
        Assert.IsType<BoroughBoundaryShape>(shape);
        Assert.Equal(expected1, shape.BoroCode);
        Assert.Equal(expected2, shape.BoroName);
    }

    [InlineData("Invalid", "Manhattan")]
    [Theory(DisplayName = "Validate Mappings - Invalid")]
    public void TestAutomapperMapping_Invalid(string value1, string value2)
    {
        // Arrange
        var attributes = new List<KeyValuePair<string, object>>()
        {
            new("BoroCode", value1),
            new("BoroName", value2),
        };

        // Act & Assert
        Assert.Throws<AutoMapperMappingException>(() => Mapper.Map<BoroughBoundaryShape>(attributes));
    }


}