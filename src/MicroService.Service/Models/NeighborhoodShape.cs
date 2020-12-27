using MicroService.Service.Models.Base;

namespace MicroService.Service.Models
{
    public class NeighborhoodShape : ShapeBase
    {
        public int BoroCode { get; set; }

        public string BoroName { get; set; }

        public string CountyFIPS { get; set; }

        public string NTACode { get; set; }

        public string NTAName { get; set; }
    }
}
