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
                cfg.AddMaps(typeof(BoroughBoundaryShapeProfile).Assembly);
            });

            // Act and Assert
            mapperConfig.AssertConfigurationIsValid();
        }

    }
}

