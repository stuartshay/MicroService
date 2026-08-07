using MicroService.Data.Enum;
using MicroService.Service.Helpers;
using MicroService.Service.Models;
using MicroService.Service.Models.Base;
using MicroService.Service.Models.Enum.Attributes;
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
                Assert.Contains(properties, p => p.Name == expectedProperty);
            }
        }


        [Fact]
        public void GetPropertiesWithoutExcludedAttribute_ReturnsOnlyPropertiesWithoutAttribute()
        {
            // Arrange
            var expectedProperties = typeof(HistoricDistrictShape)
                .GetProperties()
                .Where(p => p.GetCustomAttributes(typeof(FeatureCollectionExcludeAttribute), true).Length == 0)
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

        [Fact]
        public void EnumToList_Returns_Expected_Values()
        {
            // Arrange
            var expectedValues = new List<Borough>
            {
                Borough.MN,
                Borough.BX,
                Borough.BK,
                Borough.QN,
                Borough.SI
            };

            // Act
            var actualValues = EnumHelper.EnumToList<Borough>();

            // Assert
            Assert.Equal(expectedValues, actualValues);
        }

        [Fact]
        public void EnumToList_InvalidEnumType_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => EnumHelper.EnumToList<ShapeBase>());
        }

        [Fact]
        public void GetAttribute_ThrowsArgumentException_WhenMemberHasNoMatchingAttribute()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => NoAttributeEnum.Value.GetAttribute<FeatureNameAttribute>());
        }

        [Fact]
        public void GetEnumDescription_ReturnsEnumName_WhenNoDescriptionAttributePresent()
        {
            // Act
            var result = NoAttributeEnum.Value.GetEnumDescription();

            // Assert
            Assert.Equal(nameof(NoAttributeEnum.Value), result);
        }

        [Fact]
        public void GetValueFromDescription_FallsBackToFieldName_WhenNoDescriptionAttributePresent()
        {
            // Act
            var result = EnumHelper.GetValueFromDescription<NoAttributeEnum>(nameof(NoAttributeEnum.Value));

            // Assert
            Assert.Equal(NoAttributeEnum.Value, result);
        }

        [Fact]
        public void GetEnumDescription_ReturnsToString_WhenValueIsNotADefinedMember()
        {
            // Arrange: an undefined enum value has no backing field to reflect over.
            var undefined = (NoAttributeEnum)999;

            // Act
            var result = undefined.GetEnumDescription();

            // Assert
            Assert.Equal(undefined.ToString(), result);
        }

        private enum NoAttributeEnum
        {
            Value
        }
    }
}
