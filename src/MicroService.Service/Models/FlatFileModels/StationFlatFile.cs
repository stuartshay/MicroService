using MicroService.Service.Models.Base;

namespace MicroService.Service.Models.FlatFileModels
{
    public class StationFlatFile : FlatFileBase
    {
        public string StationId { get; set; }

        public string ComplexId { get; set; }

        public string GtfsStopId { get; set; }
    }
}
