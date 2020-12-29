using CsvHelper.Configuration;
using MicroService.Service.Models.FlatFileModels;

namespace MicroService.Service.Models.DataMaps
{
    public sealed class StationDataMap : ClassMap<StationFlatFile>
    {
        public StationDataMap()
        {
            Map(m => m.StationId).Name("Station ID");
            
            Map(m => m.ComplexId).Name("Complex ID");
            
            Map(m => m.GtfsStopId).Name("GTFS Stop ID");
            
            Map(m => m.Division).Name("Division");

            Map(m => m.Line).Name("Line");

            Map(m => m.StopName).Name("Stop Name");

            Map(m => m.Borough).Name("Borough");

            Map(m => m.DaytimeRoutes).Name("Daytime Routes");

            Map(m => m.Structure).Name("Structure");

            Map(m => m.GTFSLatitude).Name("GTFS Latitude");

            Map(m => m.GTFSLongitude).Name("GTFS Longitude");

            Map(m => m.NorthDirectionLabel).Name("North Direction Label").Default(null); 

            Map(m => m.SouthDirectionLabel).Name("South Direction Label").Default(null); 

            Map(m => m.ADA).Name("ADA")
                .TypeConverterOption.BooleanValues(true, true, "1","2")
                .TypeConverterOption.BooleanValues(false, true, "0");

            Map(m => m.ADANotes).Name("ADA Notes").Default(null);
        }

    }
}
