using CsvHelper;
using MicroService.Service.Models.Base;

namespace MicroService.Service.Models.FlatFileModels
{
    // Column names and conversions from the MTA Stations.csv header.
    [CsvRecord]
    public partial class StationFlatFile : FlatFileBase
    {
        [CsvColumn("Station ID")]
        public int StationId { get; set; }

        [CsvColumn("Complex ID")]
        public int ComplexId { get; set; }

        [CsvColumn("GTFS Stop ID")]
        public string GtfsStopId { get; set; } = string.Empty;

        [CsvColumn("Division")]
        public string Division { get; set; } = string.Empty;

        [CsvColumn("Line")]
        public string Line { get; set; } = string.Empty;

        [CsvColumn("Stop Name")]
        public string StopName { get; set; } = string.Empty;

        [CsvColumn("Borough")]
        public string Borough { get; set; } = string.Empty;

        [CsvColumn("Daytime Routes")]
        public string DaytimeRoutes { get; set; } = string.Empty;

        [CsvColumn("Structure")]
        public string Structure { get; set; } = string.Empty;

        [CsvColumn("GTFS Latitude")]
        public double GTFSLatitude { get; set; }

        [CsvColumn("GTFS Longitude")]
        public double GTFSLongitude { get; set; }

        [CsvColumn("North Direction Label")]
        [CsvNullValues("")]
        public string NorthDirectionLabel { get; set; } = string.Empty;

        [CsvColumn("South Direction Label")]
        [CsvNullValues("")]
        public string SouthDirectionLabel { get; set; } = string.Empty;

        [CsvColumn("ADA")]
        [CsvBooleanValues(["1", "2"], ["0"])]
        public bool ADA { get; set; }

        [CsvColumn("ADA Notes")]
        [CsvNullValues("")]
        public string ADANotes { get; set; } = string.Empty;

    }
}
