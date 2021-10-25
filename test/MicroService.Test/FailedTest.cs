using Xunit;

namespace MicroService.Test
{
    public class FailedTest
    {
        [Fact(DisplayName = "Failed_Test - Unit")]
        [Trait("Category", "Unit")]
        public void Failed_Test_Remove()
        {
            // Assert
            Assert.False(true);
        }
    }
}
