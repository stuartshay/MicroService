using MicroService.Data.Enum;
using MicroService.Service.Helpers;
using MicroService.Service.Models;
using MicroService.Service.Models.Enum;
using Xunit;

namespace MicroService.Test.Functions
{
    public class EnumHelperTest
    {
        [Fact]
        public void GetValueFromDescription_ReturnsCorrectValue()
        {
            // Arrange
            var expected = Borough.MN;

            // Act
            var result = EnumHelper.GetValueFromDescription<Borough>("Manhattan");

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetValueFromDescription_ThrowsArgumentException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentException>(() => EnumHelper.GetValueFromDescription<Borough>("Invalid Description"));
        }

        [Fact]
        public void GetValueFromDescription_ThrowsInvalidOperationException()
        {
            // Arrange & Act & Assert
            Assert.Throws<InvalidOperationException>(() => EnumHelper.GetValueFromDescription<string>("Invalid Enum"));
        }

        [Fact]
        public void GetPropertiesWithFeatureCollectionAttribute_ReturnsCorrectProperties()
        {
            // Arrange
            var expectedProperties = new List<string> { "LPNumber", "AreaName", "BoroName", "BoroCode" };

            // Act
            var properties = EnumHelper.GetPropertiesWithAttribute<FeatureCollectionAttribute>(typeof(ScenicLandmarkShape));

            // Assert
            Assert.Equal(expectedProperties.Count, properties.Count);
            foreach (var expectedProperty in expectedProperties)
            {
                Assert.True(properties!.Any(p => p.Name == expectedProperty));
            }
        }

        [Fact]
        public void GetPropertiesWithoutExcludedAttribute_ReturnsOnlyPropertiesWithoutAttribute()
        {
            // Arrange
            var expectedProperties = typeof(HistoricDistrictShape)
                .GetProperties()
                .Where(p => !p.GetCustomAttributes(typeof(FeatureCollectionExcludeAttribute), true).Any())
                .ToList();

            // Act
            var actualProperties = EnumHelper.GetPropertiesWithoutExcludedAttribute<HistoricDistrictShape, FeatureCollectionExcludeAttribute>();

            // Assert
            Assert.Equal(expectedProperties.Count, actualProperties.Count);
            foreach (var expectedProperty in expectedProperties)
            {
                Assert.Contains(actualProperties, p => p.Name == expectedProperty.Name);
            }
        }
    }
}
