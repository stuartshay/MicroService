using MicroService.Service.Models.Base;

namespace MicroService.Service.Models.FlatFileModels
{
    public class StationFlatFile : FlatFileBase
    {
        public int StationId { get; set; }

        public int ComplexId { get; set; }

        public string GtfsStopId { get; set; } = string.Empty;

        public string Division { get; set; } = string.Empty;

        public string Line { get; set; } = string.Empty;

        public string StopName { get; set; } = string.Empty;

        public string Borough { get; set; } = string.Empty;

        public string DaytimeRoutes { get; set; } = string.Empty;

        public string Structure { get; set; } = string.Empty;

        public double GTFSLatitude { get; set; }
        
        public double GTFSLongitude { get; set; }

        public string NorthDirectionLabel { get; set; } = string.Empty;

        public string SouthDirectionLabel { get; set; } = string.Empty;

        public bool ADA { get; set; }
        
        public string ADANotes { get; set; } = string.Empty;

    }
}
