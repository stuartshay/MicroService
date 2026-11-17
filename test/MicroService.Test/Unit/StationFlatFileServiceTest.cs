using MicroService.Service.Models.FlatFileModels;
using MicroService.Service.Services.FlatFileService;
using MicroService.Test.Fixture;
using Xunit;

namespace MicroService.Test.Unit
{
    public class StationFlatFileServiceTest
    {
        [Fact]
        public void GetAll_ReturnsStationRecords()
        {
            var service = new StationFlatFileService(RepositoryFiles.Options());

            var sut = service.GetAll().ToList();

            Assert.NotNull(sut);
            Assert.NotEmpty(sut);
            Assert.All(sut, item => Assert.IsType<StationFlatFile>(item));
        }

        // The CSV column mapping (header names, ADA values, empty text as null), pinned to the
        // values CsvHelper 33's class map read from Stations.csv before the CsvHelper 34 upgrade.
        [Fact]
        public void GetAll_MapsEveryColumnOfStationsCsv()
        {
            var stations = new StationFlatFileService(RepositoryFiles.Options()).GetAll().Cast<StationFlatFile>().ToList();

            Assert.Equal(496, stations.Count);

            var first = stations.Single(s => s.StationId == 1);
            Assert.Equal(1, first.ComplexId);
            Assert.Equal("R01", first.GtfsStopId);
            Assert.Equal("BMT", first.Division);
            Assert.Equal("Astoria", first.Line);
            Assert.Equal("Astoria-Ditmars Blvd", first.StopName);
            Assert.Equal("Q", first.Borough);
            Assert.Equal("N W", first.DaytimeRoutes);
            Assert.Equal("Elevated", first.Structure);
            Assert.Equal(40.775036, first.GTFSLatitude);
            Assert.Equal(-73.912034, first.GTFSLongitude);
            Assert.Null(first.NorthDirectionLabel);
            Assert.Equal("Manhattan", first.SouthDirectionLabel);
            Assert.False(first.ADA);
            Assert.Null(first.ADANotes);

            // ADA "2" (partly accessible) reads as true, like "1"
            var partial = stations.Single(s => s.StationId == 10);
            Assert.True(partial.ADA);
            Assert.Equal("Uptown only", partial.ADANotes);
        }

        [Fact]
        public void GetAll_ReadsAdaOneAndTwoAsAccessible()
        {
            var stations = new StationFlatFileService(RepositoryFiles.Options()).GetAll().Cast<StationFlatFile>().ToList();

            // Stations.csv: 365 rows with ADA 0, 122 with 1 and 9 with 2
            Assert.Equal(131, stations.Count(s => s.ADA));
            Assert.Equal(365, stations.Count(s => !s.ADA));
            Assert.Equal(18, stations.Count(s => s.NorthDirectionLabel is null));
            Assert.Equal(487, stations.Count(s => s.ADANotes is null));
        }
    }
}
