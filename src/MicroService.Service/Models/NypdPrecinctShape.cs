using MicroService.Service.Models.Base;
using MicroService.Service.Models.Enum;
using MicroService.Service.Models.Enum.Attibutes;
using MicroService.Service.Models.Enum.Attributes;

namespace MicroService.Service.Models
{
    [ShapeProperties(ShapeProperties.NypdPolicePrecincts)]
    public class NypdPrecinctShape : ShapeBase
    {
        [FeatureName("Precinct")]
        public int Precinct { get; set; }
    }
}
