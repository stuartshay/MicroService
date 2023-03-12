using CsvHelper;
using MicroService.Service.Helpers;
using MicroService.Service.Models.Base;
using MicroService.Service.Models.DataMaps;
using MicroService.Service.Models.Enum;
using MicroService.Service.Models.Enum.Attibutes;
using MicroService.Service.Models.FlatFileModels;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace MicroService.Service.Services.FlatFileService
{
    public class StationComplexFlatFileService : IFlatFileService
    {
        public StationComplexFlatFileService()
        {

        }

        public IEnumerable<FlatFileBase> GetAll()
        {
            var fileName = FlatFileProperties.SubwayStationComplex.GetAttribute<FlatFileAttribute>().FileName;
            var directory = FlatFileProperties.SubwayStationComplex.GetAttribute<FlatFileAttribute>().Directory;
            var inputPath = Path.Combine(FileHelpers.GetFilesDirectory(), directory, fileName);

            using var reader = new StreamReader(inputPath);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            csv.Context.RegisterClassMap<StationComplexDataMap>();
            var list = csv.GetRecords<StationComplexFlatFile>();

            return list.ToList();
        }
    }
}
