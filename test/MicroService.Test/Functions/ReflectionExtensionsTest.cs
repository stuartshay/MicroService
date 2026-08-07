using MicroService.Service.Helpers;
using Xunit;

namespace MicroService.Test.Functions
{
    public class ReflectionExtensionsTests
    {
        private class TestObject
        {
            public bool? Property3 { get; set; }
        }

        private class TestObjectWithAttribute
        {
            [MyCustom]
            public string? Property1 { get; set; }
            public int Property2 { get; set; }
            [MyCustom]
            public bool? Property3 { get; set; }
        }

        [AttributeUsage(AttributeTargets.All)]
        private class MyCustomAttribute : Attribute
        {
        }

        [Fact]
        public void ArePropertiesNotNull_AllPropertiesAreNotNull_ReturnsTrue()
        {
            // Arrange
            var obj = new TestObject
            {
                Property3 = true
            };

            // Act
            var result = obj.ArePropertiesNotNull();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ArePropertiesNotNull_AtLeastOnePropertyIsNull_ReturnsFalse()
        {
            // Arrange
            var obj = new TestObject
            {
                Property3 = null
            };

            // Act
            var result = obj.ArePropertiesNotNull();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetAttributeFromProperty_PropertyHasAttribute_ReturnsAttribute()
        {
            // Arrange
            var obj = new TestObjectWithAttribute();
            var propertyName = nameof(TestObjectWithAttribute.Property1);

            // Act
            var result = ReflectionExtensions.GetAttributeFromProperty<MyCustomAttribute>(obj, propertyName);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void GetAttributeFromProperty_PropertyDoesNotHaveAttribute_ReturnsNull()
        {
            // Arrange
            var obj = new TestObjectWithAttribute();
            var propertyName = nameof(TestObjectWithAttribute.Property2);

            // Act
            var result = ReflectionExtensions.GetAttributeFromProperty<MyCustomAttribute>(obj, propertyName);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetAttributeFromProperty_PropertyDoesNotExist_ThrowsArgumentException()
        {
            // Arrange
            var obj = new TestObjectWithAttribute();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => ReflectionExtensions.GetAttributeFromProperty<MyCustomAttribute>(obj, "NotAProperty"));
        }

        [Fact]
        public void GetPropertiesWithCustomAttribute_TypeHasPropertiesWithAttribute_ReturnsPropertiesWithAttribute()
        {
            // Arrange
            var type = typeof(TestObjectWithAttribute);

            // Act
            var result = type.GetPropertiesWithCustomAttribute<MyCustomAttribute>();

            // Assert
            Assert.Collection(result,
                p => Assert.Equal(nameof(TestObjectWithAttribute.Property1), p.Name),
                p => Assert.Equal(nameof(TestObjectWithAttribute.Property3), p.Name));
        }

        [Fact]
        public void GetPropertiesWithCustomAttribute_TypeDoesNotHavePropertiesWithAttribute_ReturnsEmptyArray()
        {
            // Arrange
            var type = typeof(TestObject);

            // Act
            var result = type.GetPropertiesWithCustomAttribute<MyCustomAttribute>();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetPropertiesWithCustomAttribute_ReturnsPropertyDecoratedWithMappingKeyAttribute()
        {
            // Arrange
            var type = typeof(MicroService.Service.Models.IndividualLandmarkHistoricDistrictsShape);

            // Act
            var result = type.GetPropertiesWithCustomAttribute<MicroService.Service.Models.Enum.Attributes.MappingKeyAttribute>();

            // Assert
            Assert.Contains(result, p => p.Name == nameof(MicroService.Service.Models.IndividualLandmarkHistoricDistrictsShape.LPNumber));
        }
    }

}
