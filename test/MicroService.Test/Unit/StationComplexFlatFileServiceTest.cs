using MicroService.Service.Models.FlatFileModels;
using MicroService.Service.Services.FlatFileService;
using Xunit;

namespace MicroService.Test.Unit
{
    public class StationComplexFlatFileServiceTest
    {
        [Fact]
        public void GetAll_ReturnsStationComplexRecords()
        {
            var service = new StationComplexFlatFileService();

            var sut = service.GetAll().ToList();

            Assert.NotNull(sut);
            Assert.NotEmpty(sut);
            Assert.All(sut, item => Assert.IsType<StationComplexFlatFile>(item));
        }
    }
}
