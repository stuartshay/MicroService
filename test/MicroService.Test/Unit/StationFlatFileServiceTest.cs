using MicroService.Service.Models.FlatFileModels;
using MicroService.Service.Services.FlatFileService;
using Xunit;

namespace MicroService.Test.Unit
{
    public class StationFlatFileServiceTest
    {
        [Fact]
        public void GetAll_ReturnsStationRecords()
        {
            var service = new StationFlatFileService();

            var sut = service.GetAll().ToList();

            Assert.NotNull(sut);
            Assert.NotEmpty(sut);
            Assert.All(sut, item => Assert.IsType<StationFlatFile>(item));
        }
    }
}
