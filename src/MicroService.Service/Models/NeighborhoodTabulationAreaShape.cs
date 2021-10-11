using MicroService.Service.Models.Base;

namespace MicroService.Service.Models
{
    public class NeighborhoodTabulationAreaShape : ShapeBase
    {
        public int BoroCode { get; set; }

        public string BoroName { get; set; }

        public string CountyFIPS { get; set; }

        public string NTA2020 { get; set; }

        public string NTAName { get; set; }

        public string NTAAbbrev { get; set; }

        public int NTAType { get; set; }

        public string CDTA2020 { get; set; }

        public string CDTAName { get; set; }
    }
}
