using MicroService.Service.Models.FlatFileModels;
using MicroService.Service.Services.FlatFileService;
using MicroService.Test.Fixture;
using Xunit;

namespace MicroService.Test.Unit
{
    public class StationComplexFlatFileServiceTest
    {
        [Fact]
        public void GetAll_ReturnsStationComplexRecords()
        {
            var service = new StationComplexFlatFileService(RepositoryFiles.Options());

            var sut = service.GetAll().ToList();

            Assert.NotNull(sut);
            Assert.NotEmpty(sut);
            Assert.All(sut, item => Assert.IsType<StationComplexFlatFile>(item));
        }

        // Pinned to the values CsvHelper 33's class map read from StationComplexes.csv.
        [Fact]
        public void GetAll_MapsComplexIdAndName()
        {
            var complexes = new StationComplexFlatFileService(RepositoryFiles.Options()).GetAll().Cast<StationComplexFlatFile>().ToList();

            Assert.Equal(32, complexes.Count);
            Assert.Equal("14 St / 6 Av", complexes.Single(c => c.ComplexId == 601).ComplexName);
        }

        // The CSV is read from the configured shape root (the files-shapes mount in Cloud Run),
        // not from a path relative to the assembly, which only exists in a source checkout.
        [Fact]
        public void GetAll_ReadsFromTheConfiguredShapeRoot()
        {
            var root = Directory.CreateTempSubdirectory("flat-file-root-");
            try
            {
                var folder = Directory.CreateDirectory(Path.Join(root.FullName, "Subway_Complex"));
                File.WriteAllText(Path.Join(folder.FullName, "StationComplexes.csv"), "Complex ID,Complex Name\n7,Test Complex\n");

                var complex = Assert.Single(new StationComplexFlatFileService(RepositoryFiles.Options(root.FullName)).GetAll().Cast<StationComplexFlatFile>());

                Assert.Equal(7, complex.ComplexId);
                Assert.Equal("Test Complex", complex.ComplexName);
            }
            finally
            {
                root.Delete(recursive: true);
            }
        }

        [Fact]
        public void Constructor_RequiresTheShapeRoot()
        {
            var options = Microsoft.Extensions.Options.Options.Create(new MicroService.Service.Configuration.ApplicationOptions());

            var exception = Assert.Throws<InvalidOperationException>(() => new StationComplexFlatFileService(options));

            Assert.Contains("ShapeConfiguration:ShapeRootDirectory", exception.Message);
        }
    }
}
