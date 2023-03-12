using MicroService.Service.Models.Base;
using MicroService.Service.Models.Enum.Attibutes;

namespace MicroService.Service.Models
{
    public class NypdPrecinctShape : ShapeBase
    {
        [FeatureName("Precinct")]
        public int Precinct { get; set; }
    }
}
