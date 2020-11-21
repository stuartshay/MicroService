using MicroService.Service.Models.Base;

namespace MicroService.Service.Models
{
    public class NypdSectorShape : ShapeBase
    {
        public string Pct { get; set; }

        public string Sector { get; set; }

        public string PatrolBoro { get; set; }
    }
}
