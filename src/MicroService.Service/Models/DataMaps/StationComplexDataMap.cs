using CsvHelper.Configuration;
using MicroService.Service.Models.FlatFileModels;

namespace MicroService.Service.Models.DataMaps
{
    public sealed class StationComplexDataMap : ClassMap<StationComplexFlatFile>
    {
        public StationComplexDataMap()
        {
            Map(m => m.ComplexId).Name("Complex ID");
            Map(m => m.ComplexName).Name("Complex Name");
        }
        
    }
}
