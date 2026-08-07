using MicroService.Service.Configuration;
using Xunit;

namespace MicroService.Test.Unit
{
    public class ShapeConfigurationTest
    {
        [Fact]
        public void ShapeSystemRootDirectory_ReturnsNull_WhenShapeRootDirectoryIsNull()
        {
            var config = new ShapeConfiguration();

            Assert.Null(config.ShapeSystemRootDirectory);
        }

        [Fact]
        public void ShapeSystemRootDirectory_ReturnsFullPath_WhenShapeRootDirectoryIsSet()
        {
            var config = new ShapeConfiguration { ShapeRootDirectory = "files" };

            var result = config.ShapeSystemRootDirectory;

            Assert.NotNull(result);
            Assert.True(Path.IsPathRooted(result));
        }
    }
}
