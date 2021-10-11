using MicroService.Service.Models.Base;

namespace MicroService.Service.Models
{
    public class BoroughBoundaryShape : ShapeBase
    {
        public int BoroCode { get; set; }

        public string BoroName { get; set; }
    }
}
