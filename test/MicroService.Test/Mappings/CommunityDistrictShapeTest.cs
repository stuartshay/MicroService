using AutoMapper;
using MicroService.Service.Models;
using Xunit;

namespace MicroService.Test.Mappings
{
    public class CommunityDistrictShapeTest : BaseMapperTest<CommunityDistrictShape>
    {
        [InlineData(401, 401)]
        [Theory(DisplayName = "Validate Mappings")]
        public void TestAutomapperMapping(object value1, int expected1)
        {
            // Arrange
            var attributes = new List<KeyValuePair<string, object>>()
            {
                new("BoroCD", value1),
            };

            // Act
            var shape = Mapper.Map<CommunityDistrictShape>(attributes);

            // Assert
            Assert.IsType<CommunityDistrictShape>(shape);
            Assert.Equal(expected1, shape.BoroCd);
        }

        [InlineData("Invalid", "Manhattan")]
        [Theory(DisplayName = "Validate Mappings - Invalid")]
        public void TestAutomapperMapping_Invalid(string value1, string value2)
        {
            // Arrange
            var attributes = new List<KeyValuePair<string, object>>()
            {
                new("CD", value1),
                new("BoroCD", value2),
            };

            // Act & Assert
            Assert.Throws<AutoMapperMappingException>(() => Mapper.Map<CommunityDistrictShape>(attributes));
        }
    }
}