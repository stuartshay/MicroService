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
        // MTA station list of 2026-11-19 (data.ny.gov 39hk-dx4f). Its columns are in a different
        // order from the 2020 file's, and it has more; the mapping is by header name.
        [Fact]
        public void GetAll_MapsEveryColumnOfStationsCsv()
        {
            var stations = new StationFlatFileService(RepositoryFiles.Options()).GetAll().Cast<StationFlatFile>().ToList();

            // 496 rows for 493 stations: 151, 167 and 461 each have two GTFS stops
            Assert.Equal(496, stations.Count);
            Assert.Equal(493, stations.Select(s => s.StationId).Distinct().Count());

            var first = stations.Single(s => s.StationId == 1);
            Assert.Equal(1, first.ComplexId);
            Assert.Equal("R01", first.GtfsStopId);
            Assert.Equal("BMT", first.Division);
            Assert.Equal("Astoria", first.Line);
            Assert.Equal("Astoria-Ditmars Blvd", first.StopName);
            Assert.Equal("Q", first.Borough);
            Assert.False(first.CBD);
            Assert.Equal("N W", first.DaytimeRoutes);
            Assert.Equal("Elevated", first.Structure);
            Assert.Equal(40.775036, first.GTFSLatitude);
            Assert.Equal(-73.912034, first.GTFSLongitude);
            Assert.Equal("Last Stop", first.NorthDirectionLabel);
            Assert.Equal("Manhattan", first.SouthDirectionLabel);
            Assert.False(first.ADA);
            Assert.False(first.ADANorthbound);
            Assert.False(first.ADASouthbound);
            // The export writes an empty note as NaN
            Assert.Null(first.ADANotes);

            // ADA "2" (partly accessible) reads as true, like "1"; the direction columns say which way
            var partial = stations.Single(s => s.StationId == 10);
            Assert.Equal("49 St", partial.StopName);
            Assert.True(partial.CBD);
            Assert.True(partial.ADA);
            Assert.True(partial.ADANorthbound);
            Assert.False(partial.ADASouthbound);
            Assert.Equal("Uptown only", partial.ADANotes);
        }

        [Fact]
        public void GetAll_ReadsAdaOneAndTwoAsAccessible()
        {
            var stations = new StationFlatFileService(RepositoryFiles.Options()).GetAll().Cast<StationFlatFile>().ToList();

            // Stations.csv: 322 rows with ADA 0, 166 with 1 and 8 with 2
            Assert.Equal(174, stations.Count(s => s.ADA));
            Assert.Equal(322, stations.Count(s => !s.ADA));
            Assert.Equal(169, stations.Count(s => s.ADANorthbound));
            Assert.Equal(171, stations.Count(s => s.ADASouthbound));
            Assert.Equal(95, stations.Count(s => s.CBD));
            Assert.Equal(0, stations.Count(s => s.NorthDirectionLabel is null));
            Assert.Equal(488, stations.Count(s => s.ADANotes is null));
        }

        [Fact]
        public void GetAll_ReadsColumnsByName_InAnyOrder_IgnoringExtraColumns()
        {
            var root = Path.Join(Path.GetTempPath(), $"stations-{Guid.NewGuid():N}");
            var directory = Path.Join(root, "Subway_Station_Locations");
            Directory.CreateDirectory(directory);
            try
            {
                // The 2020 file's column order, the 2026 columns at the end, and a column the model doesn't read
                File.WriteAllText(Path.Join(directory, "Stations.csv"),
                    "Station ID,Complex ID,GTFS Stop ID,Division,Line,Stop Name,Borough,Daytime Routes,Structure,GTFS Latitude,GTFS Longitude,North Direction Label,South Direction Label,ADA,ADA Notes,Extra,CBD,ADA Southbound,ADA Northbound\n" +
                    "1,1,R01,BMT,Astoria,Astoria-Ditmars Blvd,Q,N W,Elevated,40.775036,-73.912034,,NaN,2,NaN,x,true,0,1\n");

                var station = Assert.IsType<StationFlatFile>(
                    Assert.Single(new StationFlatFileService(RepositoryFiles.Options(root)).GetAll()));

                Assert.Equal(1, station.StationId);
                Assert.Equal("R01", station.GtfsStopId);
                Assert.Equal("N W", station.DaytimeRoutes);
                // Empty and NaN both read as no label or note
                Assert.Null(station.NorthDirectionLabel);
                Assert.Null(station.SouthDirectionLabel);
                Assert.Null(station.ADANotes);
                Assert.True(station.ADA);
                Assert.True(station.ADANorthbound);
                Assert.False(station.ADASouthbound);
                Assert.True(station.CBD);
            }
            finally
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }
}
