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
    public class StationFlatFileService : IFlatFileService
    {
        public StationFlatFileService()
        {

        }

        public IEnumerable<FlatFileBase> GetAll()
        {
            var fileName = FlatFileProperties.SubwayStationLocations.GetAttribute<FlatFileAttribute>().FileName;
            var directory = FlatFileProperties.SubwayStationLocations.GetAttribute<FlatFileAttribute>().Directory;
            var inputPath = Path.Combine(FileHelpers.GetFilesDirectory(), directory, fileName);

            using var reader = new StreamReader(inputPath);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            csv.Context.RegisterClassMap<StationDataMap>();

            var list = csv.GetRecords<StationFlatFile>();
            return list.ToList();
        }
    }
}
