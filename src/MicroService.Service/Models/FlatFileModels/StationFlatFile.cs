using CsvHelper;
using MicroService.Service.Models.Base;

namespace MicroService.Service.Models.FlatFileModels
{
    // Column names and conversions from the MTA Stations.csv header (data.ny.gov 39hk-dx4f).
    // Columns are matched by name, so their order doesn't matter; Georeference (a WKT copy of
    // the GTFS position) isn't read. The MTA export writes an empty text field as "NaN".
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

        // In the Central Business District (the congestion pricing zone)
        [CsvColumn("CBD")]
        [CsvBooleanValues("true", "false")]
        public bool CBD { get; set; }

        [CsvColumn("Daytime Routes")]
        public string DaytimeRoutes { get; set; } = string.Empty;

        [CsvColumn("Structure")]
        public string Structure { get; set; } = string.Empty;

        [CsvColumn("GTFS Latitude")]
        public double GTFSLatitude { get; set; }

        [CsvColumn("GTFS Longitude")]
        public double GTFSLongitude { get; set; }

        [CsvColumn("North Direction Label")]
        [CsvNullValues(["", "NaN"])]
        public string NorthDirectionLabel { get; set; } = string.Empty;

        [CsvColumn("South Direction Label")]
        [CsvNullValues(["", "NaN"])]
        public string SouthDirectionLabel { get; set; } = string.Empty;

        // 0 not accessible, 1 fully accessible, 2 partly accessible (see ADA Notes)
        [CsvColumn("ADA")]
        [CsvBooleanValues(["1", "2"], ["0"])]
        public bool ADA { get; set; }

        [CsvColumn("ADA Northbound")]
        [CsvBooleanValues("1", "0")]
        public bool ADANorthbound { get; set; }

        [CsvColumn("ADA Southbound")]
        [CsvBooleanValues("1", "0")]
        public bool ADASouthbound { get; set; }

        [CsvColumn("ADA Notes")]
        [CsvNullValues(["", "NaN"])]
        public string ADANotes { get; set; } = string.Empty;
    }
}
