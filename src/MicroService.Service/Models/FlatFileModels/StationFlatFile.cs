using MicroService.Service.Models.Base;

namespace MicroService.Service.Models.FlatFileModels
{
    public class StationFlatFile : FlatFileBase
    {
        public int StationId { get; set; }

        public int ComplexId { get; set; }

        public string GtfsStopId { get; set; }

        public string Division { get; set; }

        public string Line { get; set; }

        public string StopName { get; set; }

        public string Borough { get; set; }

        public string DaytimeRoutes { get; set; }

        public string Structure { get; set; }

        public double GTFSLatitude { get; set; }
        
        public double GTFSLongitude { get; set; }

        public string NorthDirectionLabel { get; set; }

        public string SouthDirectionLabel { get; set; }

        public bool ADA { get; set; }
        
        public string ADANotes { get; set; }

    }
}
