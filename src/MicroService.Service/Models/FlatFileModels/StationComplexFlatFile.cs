using CsvHelper;
using MicroService.Service.Models.Base;

namespace MicroService.Service.Models.FlatFileModels
{
    [CsvRecord]
    public partial class StationComplexFlatFile : FlatFileBase
    {
        [CsvColumn("Complex ID")]
        public int ComplexId { get; set; }

        [CsvColumn("Complex Name")]
        public string ComplexName { get; set; } = string.Empty;
    }
}
