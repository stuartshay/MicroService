using CsvHelper;
using MicroService.Service.Configuration;
using MicroService.Service.Helpers;
using MicroService.Service.Models.Base;
using MicroService.Service.Models.Enum;
using MicroService.Service.Models.Enum.Attributes;
using MicroService.Service.Models.FlatFileModels;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Options;

namespace MicroService.Service.Services.FlatFileService
{
    public class StationComplexFlatFileService : IFlatFileService
    {
        private readonly string _rootDirectory;

        /// <summary>
        /// Reads the CSV from the shape root (ShapeConfiguration:ShapeRootDirectory): the
        /// files-shapes mount in Cloud Run, the repository's files/ folder elsewhere.
        /// </summary>
        public StationComplexFlatFileService(IOptions<ApplicationOptions> options)
        {
            ArgumentNullException.ThrowIfNull(options);
            _rootDirectory = options.Value.ShapeConfiguration?.ShapeSystemRootDirectory
                ?? throw new InvalidOperationException("ShapeConfiguration:ShapeRootDirectory configuration is required.");
        }

        public IEnumerable<FlatFileBase> GetAll()
        {
            var fileName = FlatFileProperties.SubwayStationComplex.GetAttribute<FlatFileAttribute>().FileName;
            var directory = FlatFileProperties.SubwayStationComplex.GetAttribute<FlatFileAttribute>().Directory;
            var inputPath = Path.Join(_rootDirectory, directory, fileName);

            using var reader = new StreamReader(inputPath);
            using var csv = CsvReader.Create(reader, CsvReaderOptions.Default, leaveOpen: false);
            var list = csv.GetRecords<StationComplexFlatFile>();

            return list.ToList();
        }
    }
}
