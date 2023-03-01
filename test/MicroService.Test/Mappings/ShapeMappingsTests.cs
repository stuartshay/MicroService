using AutoMapper;
using MicroService.Service.Mappings;
using Xunit;

namespace MicroService.Test.Mappings
{
    public class ShapeMappingsTests
    {
        [Fact]
        public void ValidateMappings_ShouldNotThrowException()
        {
            // Arrange
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<ShapeMappings>();
            });

            // Act and Assert
            mapperConfig.AssertConfigurationIsValid();
        }

    }
}

