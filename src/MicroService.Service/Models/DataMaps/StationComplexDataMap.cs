using CsvHelper.Configuration;
using MicroService.Service.Models.FlatFileModels;

namespace MicroService.Service.Models.DataMaps
{
    public class StationDataMap : ClassMap<StationFlatFile>
    {
        public StationDataMap()
        {
            Map(m => m.StationId).Name("Station ID");
            Map(m => m.ComplexId).Name("Complex ID");
            Map(m => m.GtfsStopId).Name("GTFS Stop ID");

        }
        
    }
}
