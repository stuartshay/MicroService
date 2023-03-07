using MicroService.Service.Helpers;
using Xunit;

namespace MicroService.Test.Functions
{
    public class ReflectionExtensionsTests
    {
        private class TestObject
        {
            public string? Property1 { get; set; }
            public int Property2 { get; set; }
            public bool? Property3 { get; set; }
        }

        private class TestObjectWithAttribute
        {
            [MyCustomAttribute]
            public string? Property1 { get; set; }
            public int Property2 { get; set; }
            [MyCustomAttribute]
            public bool? Property3 { get; set; }
        }

        private class MyCustomAttribute : Attribute
        {
        }

        [Fact]
        public void ArePropertiesNotNull_AllPropertiesAreNotNull_ReturnsTrue()
        {
            // Arrange
            var obj = new TestObject
            {
                Property1 = "value1",
                Property2 = 2,
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
                Property1 = "value1",
                Property2 = 2,
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
    }

}
