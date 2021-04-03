using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using MicroService.Service.Helpers;
using MicroService.Service.Models.Base;
using MicroService.Service.Models.Enum;
using MicroService.Service.Models.FlatFileModels;

namespace MicroService.Service.Services.FlatFileService
{
    public class StationFlatFileService : IFlatFileService<StationFlatFile>
    {
        public StationFlatFileService()
        {
            
        }

        public IEnumerable<FlatFileBase> GetAll()
        {
            var fileName = FlatFileProperties.SubwayStationLocations.GetAttribute<FlatFileAttributes>().FileName;
            var directory = FlatFileProperties.SubwayStationLocations.GetAttribute<FlatFileAttributes>().Directory;
            var inputPath = Path.Combine(FileHelpers.GetFilesDirectory(), directory, fileName);

            IEnumerable<StationFlatFile> list;
            using var reader = new StreamReader(inputPath);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            //csv.Configuration.RegisterClassMap<StationDataMap>();
            list = csv.GetRecords<StationFlatFile>();
            //var list = csv.GetRecordsAsync<StationFlatFile>();

            return list.ToList();
        }
    }
}
