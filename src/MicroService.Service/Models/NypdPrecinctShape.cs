using MicroService.Service.Models.Base;
using MicroService.Service.Models.Enum;

namespace MicroService.Service.Models
{
    public class NypdPrecinctShape : ShapeBase
    {
        [FeatureName("Precinct")]
        public int Precinct { get; set; }
    }
}
