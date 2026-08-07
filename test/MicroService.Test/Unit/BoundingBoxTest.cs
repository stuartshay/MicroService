using MicroService.Service.Models.Base;
using Xunit;

namespace MicroService.Test.Unit
{
    public class BoundingBoxTest
    {
        [Fact]
        public void Width_ReturnsDifferenceBetweenMaxAndMinX()
        {
            var box = new BoundingBox { MinX = 10, MaxX = 25 };

            Assert.Equal(15, box.Width);
        }

        [Fact]
        public void Height_ReturnsDifferenceBetweenMaxAndMinY()
        {
            var box = new BoundingBox { MinY = 5, MaxY = 30 };

            Assert.Equal(25, box.Height);
        }
    }
}
