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

        // Pinned to the values CsvHelper 33's class map read from StationComplexes.csv.
        [Fact]
        public void GetAll_MapsComplexIdAndName()
        {
            var complexes = new StationComplexFlatFileService().GetAll().Cast<StationComplexFlatFile>().ToList();

            Assert.Equal(32, complexes.Count);
            Assert.Equal("14 St / 6 Av", complexes.Single(c => c.ComplexId == 601).ComplexName);
        }
    }
}
