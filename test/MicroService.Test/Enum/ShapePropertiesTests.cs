using MicroService.Service.Models;
using MicroService.Service.Models.Base;
using MicroService.Service.Models.Enum;
using MicroService.Service.Models.Enum.Attributes;
using System.ComponentModel;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace MicroService.Test.Enum;

public class ShapePropertiesTests
{
    private readonly ITestOutputHelper _output;

    public ShapePropertiesTests(ITestOutputHelper output)
    {
        _output = output;
    }
    [Fact]
    public void ShapeProperties_ShouldHaveRequiredShapeAttributes()
    {
        var invalidEnumValues = new List<string>();

        foreach (var enumValue in System.Enum.GetValues(typeof(ShapeProperties)).Cast<ShapeProperties>())
        {
            var shapeAttribute = enumValue.GetType()
                .GetMember(enumValue.ToString())[0]
                .GetCustomAttribute<ShapeAttribute>();

            var descriptionAttribute = enumValue.GetType()
                .GetMember(enumValue.ToString())[0]
                .GetCustomAttribute<DescriptionAttribute>();

            var shapeStyleAttribute = enumValue.GetType()
                .GetMember(enumValue.ToString())[0]
                .GetCustomAttribute<ShapeStyleAttribute>();

            if (shapeAttribute == null || descriptionAttribute == null || shapeStyleAttribute == null)
            {
                invalidEnumValues.Add(enumValue.ToString());
            }
        }

        Assert.Empty(invalidEnumValues);
    }

    [Fact]
    public void ShapeBase_DerivedClasses_ShouldHaveShapePropertiesAttribute()
    {
        // Arrange
        var shapeBaseType = typeof(ShapeBase);
        var modelAssembly = typeof(BoroughBoundaryShape).Assembly;
        var derivedTypes = modelAssembly.GetTypes()
            .Where(t => t.Namespace == "MicroService.Service.Models" && shapeBaseType.IsAssignableFrom(t) && !t.IsAbstract);
        var invalidTypes = new List<string>();

        // Act
        foreach (var derivedType in derivedTypes)
        {
            var shapePropertiesAttribute = derivedType.GetCustomAttribute<ShapePropertiesAttribute>();
            if (shapePropertiesAttribute == null)
            {
                if (derivedType.FullName != null) invalidTypes.Add(derivedType.FullName);
            }
        }

        // Assert
        _output.WriteLine($"Number of invalid types: {invalidTypes.Count}");
        Assert.Empty(invalidTypes);
    }

    [Fact]
    public void GetDatumFromHistoricDistrictShape_ShouldReturnCorrectValue()
    {
        // Arrange
        var instance = new HistoricDistrictShape();

        // Act
        var datum = GetDatumFromHistoricDistrictShape(instance);

        // Assert
        Assert.Equal(Datum.Wgs84, datum);
    }

    public static Datum GetDatumFromHistoricDistrictShape(HistoricDistrictShape instance)
    {
        // Get the ShapePropertiesAttribute from the instance
        var shapePropertiesAttribute = instance.GetType().GetCustomAttribute<ShapePropertiesAttribute>();
        if (shapePropertiesAttribute == null)
        {
            throw new InvalidOperationException("ShapePropertiesAttribute not found.");
        }

        // Get the ShapeProperties enum value from the attribute
        ShapeProperties shapeProperties = shapePropertiesAttribute.ShapeProperties;

        // Get the ShapeAttribute from the enum field
        var enumField = typeof(ShapeProperties).GetMember(shapeProperties.ToString()).FirstOrDefault();
        if (enumField == null)
        {
            throw new InvalidOperationException("Enum field not found.");
        }

        var shapeAttribute = enumField.GetCustomAttribute<ShapeAttribute>();
        if (shapeAttribute == null)
        {
            throw new InvalidOperationException("ShapeAttribute not found.");
        }

        // Return the Datum value
        return shapeAttribute.Datum;
    }


}