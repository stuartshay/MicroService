using MicroService.Service.Models.Base;

namespace MicroService.Service.Models.FlatFileModels
{
    public class StationComplexFlatFile : FlatFileBase
    {
        public int ComplexId { get; set; }

        public string ComplexName { get; set; }
    }
}
